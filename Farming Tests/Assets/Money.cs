﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Money : MonoBehaviour
{
    public float money;
    public float cows;
    public float sheds;
    public float maxCows;

    public float cowCost = 10;
    public float shedCost = 100;

    public GameObject moneyCount;
    public GameObject cowCount;
    public GameObject shedCount;
    public GameObject maxCowCount;

    public GameObject buyCow;
    public GameObject buyShed;
    public GameObject buyPrecaution;

    public GameObject sellCow;
    public GameObject sellShed;

    void Start()
    {
        money = 0;
        cows = 1;
        sheds = 1;
        maxCows = 20;
    }

    void Update()
    {
        ShopManager.money += Time.deltaTime;

        moneyCount.GetComponent<Text>().text = "Money: €" + ShopManager.money.ToString("F0");
        cowCount.GetComponent<Text>().text = "Cows: " + cows;
        shedCount.GetComponent<Text>().text = "Sheds: " + sheds;
        maxCowCount.GetComponent<Text>().text = "Max Cows: " + maxCows;
    }

    public void BuyCow()
    {
        if (money >= cowCost && cows < maxCows)
        {
            money -= cowCost;
            cows += 1;
        }
    }

    public void BuyShed()
    {
        if (money >= shedCost)
        {
            money -= shedCost;
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
        if (sheds > 1 && cows <= maxCows - 20)
        {
            sheds -= 1;
            maxCows -= 20;
            money += 50;
        }
    }

}
