using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class ruchator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    int p = 0;
    void FixedUpdate()
    {
        if (p++ > 10)
        {
            p = 0;

            float a = UnityEngine.Random.Range(0, 10);
            float b = UnityEngine.Random.Range(0, 10);
            float c = UnityEngine.Random.Range(0, 10);

            transform.position = new Vector3(a, b, c);
        }
    }
}
