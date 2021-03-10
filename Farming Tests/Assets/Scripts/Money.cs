using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Money : MonoBehaviour
{
    public float money;
    public float cows;
    public float sheds;
    public float maxCows;

    public GameObject moneyCount;
    public GameObject cowCount;
    public GameObject shedCount;
    public GameObject maxCowCount;

    public GameObject buyCow;
    public GameObject buyShed;

    public GameObject sellCow;
    public GameObject sellShed;

    void Start()
    {
        cows = 1;
        maxCows = 20;
        sheds = 1;
    }

    void Update()
    {
        money += Time.deltaTime * cows;

        moneyCount.GetComponent<Text>().text = "Money: €" + money.ToString("F0");
        cowCount.GetComponent<Text>().text = "Cows: " + cows.ToString("F0");
        shedCount.GetComponent<Text>().text = "Sheds: " + sheds.ToString("F0");
        maxCowCount.GetComponent<Text>().text = "Max Cows: " + maxCows.ToString("F0");

    }

    public void BuyCow()
    {
        if (money >= 10 && cows < maxCows)
        {
            money -= 10;
            cows += 1;
        }
    }

    public void BuyShed()
    {
        if (money >= 100)
        {
            money -= 100;
            sheds += 1;
            maxCows += 20;
        }
    }

    public void SellCow()
    {
        if (cows > 1)
        {
            cows -= 1;
            money += 5;
        }
    }

    public void SellShed()
    {
        if (sheds > 1 && cows <= maxCows - (sheds - 1) *20)
        {
            sheds -= 1;
            maxCows -= 20;
            money += 50;
        }
    }
}
