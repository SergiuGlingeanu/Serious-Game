using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Accidents : MonoBehaviour
{
    public float chance = 50f;
    public float accidentInterval = 30f;
    [SerializeField] private float _timer = 0;

    void Start()
    {
        _timer = 0f;
    }

    void Update()
    {
        if (GameSettings.currentGameMode == GameMode.BuyMode) return;
        if (_timer >= accidentInterval)
        {
            _timer = 0f;
            if (Random.Range(0f, 100f) >= chance)
                CheckAccidentEligibility();
        }
        else
            _timer += Time.deltaTime;
    }

    private void CheckAccidentEligibility() {
        foreach (Structure structure in GridManager.instance.builtStructures)
            structure.CheckForAccident();
    }

    //public void AccidentOccur()
    //{
    //    Shop.GetComponent<Money>().money -= 50;
    //}

    //public void BuyPrecaution()
    //{
    //    if (Shop.GetComponent<Money>().money >= precautionsCost && precautions < maxPrecautions)
    //    {
    //        precautions += 1;
    //        Shop.GetComponent<Money>().money -= precautionsCost;
    //        chance += 10;
    //    }
    //}
}
