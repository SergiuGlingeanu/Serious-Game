using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header ("Structures")]

    public StructureInfo[] structures;
    public static float money;

    private void OnEnable()
    {
        money = 100;
    }

    public void PurchaseStructure(int index)
    {
        if (index < structures.Length)
        {
            StructureInfo structure = structures [index];

            if (CanBuy(structure))
            {
                money -= structure.cost;
                Instantiate(structure.prefab);
            }
        }
    }

    private bool CanBuy(StructureInfo structure)
    {
        if (structure == null) return false;
        return money >= structure.cost;
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
    FarmHouse
}

[System.Serializable]
public enum PrecautionType : byte
{
    Fence,
    Signs,
    StrongerFence,
    EvenStrongerFence
}

