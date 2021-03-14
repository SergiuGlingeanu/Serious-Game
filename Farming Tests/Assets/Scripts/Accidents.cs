using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Accidents : MonoBehaviour
{
    public float chance;
    public float timeStamp = 0;
    public float coolDown = 30.0f;
    public float precautions;
    public float maxPrecautions;
    public float accidentChance;

    public float precautionsCost;

    public GameObject Shop;
    public GameObject chancesOfAccident;
    public GameObject precautionCount;

    void Start()
    {
        timeStamp = Time.time + coolDown;
        chance = 50;
        precautions = 0;
        maxPrecautions = 5;
        accidentChance = 50;
        precautionsCost = 15;
    }

    void Update()
    {
        if (timeStamp <= Time.time)
        {
            timeStamp = Time.time + coolDown;
            if (Random.Range(0.0f, 100.0f) >= chance)
            {
                AccidentOccur();
            }
        }

        chancesOfAccident.GetComponent<Text>().text = "Chances Of An Accident: " + (accidentChance - precautions * 10) + "%";
        precautionCount.GetComponent<Text>().text = "Precautions: " + (precautions);
    }

    public void AccidentOccur()
    {
        Shop.GetComponent<Money>().money -= 50;
        Debug.Log("ya done fucked up");
    }

    public void BuyPrecaution()
    {
        if (Shop.GetComponent<Money>().money >= precautionsCost && precautions < maxPrecautions)
        {
            precautions += 1;
            Shop.GetComponent<Money>().money -= precautionsCost;
            chance += 10;
        }
    }
}
