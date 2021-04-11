using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tom.Utility;

public class ShopManager : MonoBehaviour
{
    [Header ("Structures")]
    public StructureInfo[] structures;
    public static float money;

    [Header("References")]
    public TMPro.TextMeshProUGUI moneyDisplay;

    [Header("Temporary")]
    public ShopItem buttonPrefab;
    public Transform buttonParent;
    public Transform nyaa;
    public float spawnRadius = 10f;

    private void Start()
    {
        CreateTemporaryButtons();
    }

    private void CreateTemporaryButtons() {
        if (!buttonParent) return;
        for (int i = 0; i < structures.Length; i++) {
            int h = i;
            StructureInfo structure = structures[i];
            ShopItem item = Instantiate(buttonPrefab, buttonParent);
            item.transform.localPosition = Vector3.zero;
            item.costLabel.text = $"{GameSettings.currency} {structure.cost}";
            item.itemLabel.texture = IconHub.QueryStructureIcon(structure.type);
            item.button.onClick.RemoveAllListeners();
            item.button.onClick.AddListener(delegate { PurchaseStructure(h); });
        }
    }

    private void OnEnable()
    {
        money = 10000;
    }

    public void PurchaseStructure(int index)
    {
        if (index < structures.Length)
        {
            StructureInfo structure = structures [index];

            if (CanBuy(structure))
            {
                Vector3 pos = (nyaa.position + Random.insideUnitSphere * spawnRadius).ReplaceY(0.1f);
                money -= structure.cost;

                Structure structureObject = Instantiate(structure.prefab, pos, structure.prefab.transform.rotation)
                    .GetComponent<Structure>();
                structureObject.type = (StructureType)index;
                structureObject.stats = structure.stats;
                GridManager.instance.builtStructures.Add(structureObject);
            }
        }
    }

    private bool CanBuy(StructureInfo structure)
    {
        if (structure == null) return false;
        return money >= structure.cost;
    }

    private void Update()
    {
        moneyDisplay.text = $"{GameSettings.currency} {money}";
    }

    private void OnDrawGizmos()
    {
        if (!nyaa) return;
        Gizmos.color = new Color(1f, 0f, 1f, 0.4f);
        Gizmos.DrawSphere(nyaa.position, spawnRadius);
    }

    public void SellSelected() {
        Structure structure = GridManager.instance.CurrentStructure;
        if (structure && structure.sellable) {
            StructureInfo info = structures[(int)structure.type];
            GridManager.instance.UnhookCurrentStructure();
            GridManager.instance.sellButton.SetActive(false);
            GridManager.instance.builtStructures.Remove(structure);
            Destroy(structure.gameObject);
            UpdateStatsForStructure(info);
        }
    }

    private void UpdateStatsForStructure(StructureInfo info) {
        money += info.cost / 2f;
        //More to come
    }
}

[System.Serializable]
public class StructureInfo
{
    public string name;
    public StructureType type;
    public int cost;
    public GameObject prefab;
    public Stats stats;
}

[System.Serializable]
public struct Stats
{
    public byte slurryStorage;
    public byte slurryProduction;
    public byte cowStorage;
    public byte moneyGeneration;
    public byte accidentRatePerAccidentInterval;
    public PrecautionType[] precautions;
}

[System.Serializable]
public enum StructureType : byte
{
    SlurryTank,
    Shed,
    WalledShed,
    Cows,
    Barn,
    Tractor,
    MilkingParlor,
    FarmHouse
}

[System.Serializable]
public enum PrecautionType : byte
{ //Color-Coded Precautions
    //Maintenance - Timed
    //Calving Shed
    //Shed Upgrade - Slide Gates
    Fence,
    Signs,
    StrongerFence,
    ElectricFence,
    PPE,
    SlideGate,
    Maintenance
}

