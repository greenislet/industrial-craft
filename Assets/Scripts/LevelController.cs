using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public GameObject CommonBlock;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                //Instantiate(CommonBlock, new Vector3(i, Math.Sin(i * j) * 3, j)
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
