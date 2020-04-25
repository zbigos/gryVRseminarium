using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gmanager : MonoBehaviour
{
    public GameObject Gtile;
    public GameObject ServerGO;
    public GameObject[,] Tiles;
    // Start is called before the first frame update
    void Start()
    {
        Tiles = new GameObject[3, 3];
        for(int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
            {
                GameObject hook = Instantiate(Gtile, new Vector3(1.1f * i, 1.1f * j, 0.0f), Quaternion.identity, transform);
                hook.name = $"GameTile_{i}_{j}";
                Tiles[i, j] = hook;
            }

    }

    public void ClientRespond(string msg)
    {
        Debug.Log($"[ client ] Server responded with {msg}");
        string[] separator = { "_" };
        string[] strlist = msg.Split(separator, 20, System.StringSplitOptions.RemoveEmptyEntries); // 1 3 4

        int pid = Convert.ToInt32(strlist[1]);
        int x = Convert.ToInt32(strlist[3]);
        int y = Convert.ToInt32(strlist[4]);

        Debug.Log($"[ client ] Server issued operation Pid {pid} -> {x}x{y}");
        if (pid == 0)
            Tiles[x, y].GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        if (pid == 1)
            Tiles[x, y].GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        if (pid == 2)
            Tiles[x, y].GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                print($"My object is clicked by mouse {hit.transform.name}");
                ServerGO.GetComponent<ServerDispatcher>().SendMessageToServer(hit.transform.name);
            }
        }
    }
}
