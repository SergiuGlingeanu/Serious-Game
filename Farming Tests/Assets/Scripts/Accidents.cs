using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accidents : MonoBehaviour
{
    public float chance;
    public float timeStamp = 0;
    public float coolDown = 30.0f;

    public GameObject Shop;

    void Start()
    {
        timeStamp = Time.time + coolDown;
        chance = 50;
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
    }

    public void AccidentOccur()
    {
        Shop.GetComponent<Money>().money -= 50;
        Debug.Log("ya done fucked up");
    }
}
