using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewMoney : MonoBehaviour
{
    public float money;
    public float cows;
    public float sheds;
    public float maxCows;

    public float Cost;

    

    public GameObject moneyCount;
    public GameObject cowCount;
    public GameObject shedCount;
    public GameObject maxCowCount;

    public GameObject buy;

    public GameObject sell;


    void Start()
    {
        money = 0;
        cows = 1;
        sheds = 1;
        maxCows = 20;
    }

    void Update()
    {
        if (money <= 0) { money = 0; }

        money += Time.deltaTime * cows;

        moneyCount.GetComponent<Text>().text = "Money: €" + money.ToString("F0");
        cowCount.GetComponent<Text>().text = "Cows: " + cows;
        shedCount.GetComponent<Text>().text = "Sheds: " + sheds;
        maxCowCount.GetComponent<Text>().text = "Max Cows: " + maxCows;
    }

    public void Buy()
    {
        if (money >= Cost)
        {
            money -= Cost;
        }
    }

    public void Sell()
    {
        money += Cost;
    }

}
