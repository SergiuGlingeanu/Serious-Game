using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameEventManager : MonoBehaviour
{
    [Header("Economy")]
    public float moneyGenerationIntervalSeconds = 2f;
    private float _moneyGenerationTimer;

    [Header("Accidents")]
    public float chance = 50f;
    public float accidentInterval = 30f;
    [SerializeField] private float _accidentTimer = 0;

    public static Stats GlobalStatistics { get; set; }
    public static float Money { get; set; }

    private void Awake()
    {
        Money = GameSettings.kStartMoney;
        GlobalStatistics = Stats.Default;
        chance = GlobalStatistics.accidentRatePerAccidentInterval;
    }

    private void Update()
    {
        if (GameSettings.currentGameMode == GameMode.BuyMode) return;

        if (_moneyGenerationTimer >= moneyGenerationIntervalSeconds)
        {
            _moneyGenerationTimer = 0;
            Money += GlobalStatistics.moneyGeneration / GameSettings.kMoneyGenerationDivider;
        }
        else
            _moneyGenerationTimer += Time.deltaTime;

        if (_accidentTimer >= accidentInterval)
        {
            _accidentTimer = 0f;
            if (Random.Range(0f, 100f) >= chance)
                CheckAccidentEligibility();
        }
        else
            _accidentTimer += Time.deltaTime;
    }

    private void CheckAccidentEligibility()
    {
        foreach (Structure structure in GridManager.instance.BuiltStructures)
            structure.CheckForAccident();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameEventManager))]
public class GameEventManagerEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox($"Money: {GameEventManager.Money}\n" +
            $"Global Stats: {GameEventManager.GlobalStatistics}", MessageType.Info, true);
    }
}
#endif