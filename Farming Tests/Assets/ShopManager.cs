using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tom.Utility;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopManager : MonoBehaviour
{
    [Header ("Structures")]
    public StructureInfo[] structures;

    [Header("References")]
    public TMPro.TextMeshProUGUI moneyDisplay;
    public ShopItem buttonPrefab;
    public ScrollRect buyMenuPane;
    public Transform contentContainer;

    public static List<Structure> PendingPurchases { get; } = new List<Structure>();

    private void Start() => CreateTemporaryButtons();

    private void CreateTemporaryButtons() {
        if (!buyMenuPane) return;
        for (int i = 0; i < structures.Length; i++) {
            int h = i;
            StructureInfo structure = structures[i];
            ShopItem item = Instantiate(buttonPrefab, contentContainer);
            EventTrigger customEventHandler = item.GetComponent<EventTrigger>();
            EventTrigger.Entry dragEvent = new EventTrigger.Entry();
            dragEvent.eventID = EventTriggerType.Drag;
            dragEvent.callback.AddListener(delegate { OnShopItemDragged(h); });
            customEventHandler.triggers.Add(dragEvent);

            EventTrigger.Entry dragEndEvent = new EventTrigger.Entry();
            dragEndEvent.eventID = EventTriggerType.EndDrag;
            dragEndEvent.callback.AddListener(delegate { OnShopItemDragEnded(h); });
            customEventHandler.triggers.Add(dragEndEvent);

            item.transform.localPosition = Vector3.zero;
            item.costLabel.text = $"{GameSettings.currency} {structure.cost}";
            item.itemLabel.texture = IconHub.QueryStructureIcon(structure.type);
            //item.button.onClick.RemoveAllListeners();
            //item.button.onClick.AddListener(delegate { PurchaseStructure(h); });
        }
    }

    private Structure _currentStructure;
    private bool _retractBuyMenu;

    private void OnShopItemDragged(int id) {
        buyMenuPane.enabled = false;
        if (Tom.Utility.Utilities.IsPointerOverUIMobileFriendly()) return;
        if (id >= structures.Length) return;

        if (!_currentStructure)
        {
            _retractBuyMenu = true;
            GridManager.instance.DeselectStructure();
            StructureInfo structure = structures[id];
            _currentStructure = Instantiate(structure.prefab, Vector3.zero.ReplaceY(GridManager.instance.buildingY), structure.prefab.transform.rotation)
                        .GetComponent<Structure>();
            _currentStructure.type = (StructureType)id;
            _currentStructure.stats = structure.stats;

            Renderer renderer = _currentStructure.GetComponent<Renderer>();
            if (renderer.material) {
                Color c = renderer.material.color;
                Tom.Utility.Utilities.SetBlendMode(renderer.material, BlendMode.Fade);
                renderer.material.color = new Color(c.r, c.g, c.b, 0.4f);
            }

            GridManager.instance.HookStructure(_currentStructure);
            GridManager.instance.BuiltStructures.Add(_currentStructure);
            PendingPurchases.Add(_currentStructure);
        }
        else
            GridManager.instance.OnCurrentStructureDragged();
    }

    private void OnShopItemDragEnded(int id) {
        if (!_currentStructure) return;
        buyMenuPane.enabled = true;
        _retractBuyMenu = false;
        GridManager.instance.OnCurrentStructureDragEnded();
        if (GridManager.instance.CheckForInvalidPlacement(_currentStructure))
            AbortPlacement(_currentStructure);
        _currentStructure = null;
    }

    public void AbortPlacement(Structure structure) {
        GridManager.instance.DeselectStructure();
        GridManager.instance.BuiltStructures.Remove(structure);

        if (!PendingPurchases.Remove(structure))
            Debug.LogWarning($"Failed to remove pending purchase for [{structure.name}]"); //Make failsafe

        Destroy(structure.gameObject);
    }

    public void PurchaseStructure(int index)
    {

        //if (index < structures.Length)
        //{
        //    StructureInfo structure = structures [index];

        //    if (CanBuy(structure))
        //    {
        //        Vector3 pos = (nyaa.position + Random.insideUnitSphere * spawnRadius).ReplaceY(0.1f);
        //        money -= structure.cost;

        //        Structure structureObject = Instantiate(structure.prefab, pos, structure.prefab.transform.rotation)
        //            .GetComponent<Structure>();
        //        structureObject.type = (StructureType)index;
        //        structureObject.stats = structure.stats;
        //        GridManager.instance.BuiltStructures.Add(structureObject);
        //    }
        //}
    }

    public bool FinalizePurchaseForCurrentStructure() { //Disable Green Button (POLISH)
        Structure current = GridManager.instance.CurrentStructure;
        if (!current) return false;
        if (current.Purchased) return true;
        if (current.type == StructureType.FarmHouse) return true;
        StructureInfo info = structures[(byte)current.type];
        if (CanBuy(info))
        {
            if (!PendingPurchases.Remove(current))
                Debug.LogWarning($"Failed to remove pending purchase for [{current.name}]");
            UpdateStatsForStructure(info, StructureUpdateType.Buy);
            Renderer renderer = current.GetComponent<Renderer>();
            if (renderer.material)
            {
                Color c = renderer.material.color;
                Tom.Utility.Utilities.SetBlendMode(renderer.material, BlendMode.Opaque);
                renderer.material.color = new Color(c.r, c.g, c.b, 1f);
            }
            current.Purchased = true;
            return true;
        }
        else
            Debug.Log($"You can't afford [{info.name}]!");

        return false;
    }

    private bool CanBuy(StructureInfo structure)
    {
        if (structure == null) return false;
        return GameEventManager.Money >= structure.cost;
    }

    private void Update()
    {
        moneyDisplay.text = $"{GameSettings.currency} {GameEventManager.Money}";

        if (_retractBuyMenu) {
            buyMenuPane.transform.localScale =
           buyMenuPane.transform.localScale.ReplaceY(Mathf.MoveTowards(buyMenuPane.transform.localScale.y, 0f, Time.deltaTime * 10f));
        } else if(!_retractBuyMenu && buyMenuPane.transform.localScale.y != 1f)
            buyMenuPane.transform.localScale =
           buyMenuPane.transform.localScale.ReplaceY(Mathf.MoveTowards(buyMenuPane.transform.localScale.y, 1f, Time.deltaTime * 10f));
    }

    private void OnDrawGizmos()
    {
        //if (!nyaa) return;
        //Gizmos.color = new Color(1f, 0f, 1f, 0.4f);
        //Gizmos.DrawSphere(nyaa.position, spawnRadius);
    }

    public void SellSelected() {
        Structure structure = GridManager.instance.CurrentStructure;
        if (structure && structure.sellable) {
            StructureInfo info = structures[(int)structure.type];
            GridManager.instance.UnhookCurrentStructure();
            GridManager.instance.sellButton.SetActive(false);
            GridManager.instance.BuiltStructures.Remove(structure);
            Destroy(structure.gameObject);
            UpdateStatsForStructure(info, StructureUpdateType.Sell);
        }
    }

    private void UpdateStatsForStructure(StructureInfo info, StructureUpdateType updateType) {
        if (updateType == StructureUpdateType.Sell)
        {
            GameEventManager.Money += info.cost / 2f;
            GameEventManager.GlobalStatistics -= info.stats;
        }
        else {
            GameEventManager.Money -= info.cost;
            GameEventManager.GlobalStatistics += info.stats;
        }
    }

    public void VerifyStructureStatuses() {
        for (int i = PendingPurchases.Count - 1; i >= 0; i--) {
            AbortPlacement(PendingPurchases[i]);
        }
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

    public static Stats Default => new Stats() { 
        slurryStorage = 0,
        slurryProduction = 0,
        cowStorage = 0,
        moneyGeneration = 0,
        accidentRatePerAccidentInterval = 30
    };

    public static Stats operator -(Stats stats, Stats subtractee) {
        stats.slurryStorage -= subtractee.slurryStorage;
        stats.slurryProduction -= subtractee.slurryProduction;
        stats.cowStorage -= subtractee.cowStorage;
        stats.moneyGeneration -= subtractee.moneyGeneration;
        return stats;
    }

    public static Stats operator +(Stats stats, Stats subtractee)
    {
        stats.slurryStorage += subtractee.slurryStorage;
        stats.slurryProduction += subtractee.slurryProduction;
        stats.cowStorage += subtractee.cowStorage;
        stats.moneyGeneration += subtractee.moneyGeneration;
        return stats;
    }

    public override string ToString()
    {
        return $"Slurry Storage: {slurryStorage}\n" +
            $"Slurry Production: {slurryProduction}\n" +
            $"Cow Storage: {cowStorage}\n" +
            $"Money Generation: {moneyGeneration}";
    }
}

[System.Serializable]
public enum StructureType : byte
{
    SlurryTank,
    Shed,
    WalledShed,
    Cows,
    Barn,
    MilkingParlor,
    LargeStorageShed,
    Tractor,
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

public enum StructureUpdateType { 
    Sell,
    Buy,

}

