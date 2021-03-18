using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{

    public StructureInfo structureInfo;

    [System.Serializable]
    public class StructureInfo
    {
        public string name;
        public StructureType type;
        public uint cost;
        public GameObject prefab;
        public Stats stats;
    }

    [System.Serializable]
    public struct Stats
    {
        public byte slurryStorage;
        public byte cowStorage;
        public byte moneyGeneration;
        public byte accidentRatePerAccidentInterval;
        public PrecautionType[] precautions;
    }

    [System.Serializable]
    public enum StructureType
    {
        SlurryTank,
        Shed,
        WalledShed,
        FarmHouse
    }

    [System.Serializable]
    public enum PrecautionType
    {
        Fence,
        Signs,
        StrongerFence,
        EvenStronerFence
    }

}
