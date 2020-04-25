using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class ServerDispatcher : MonoBehaviour
{
    public string Sip;
    Telepathy.Client client = new Telepathy.Client();
    Telepathy.Server server = new Telepathy.Server();
    public GameObject GManager;
    [SerializeField]
    public int[,] tileowner;

    public void SendMessageToServer(string message)
    {
        byte[] btText;
        btText = System.Text.Encoding.UTF8.GetBytes(message);
        client.Send(btText);
    }

    void Awake()
    {
        // update even if window isn't focused, otherwise we don't receive.
        Application.runInBackground = true;

        // use Debug.Log functions for Telepathy so we can see it in the console
        Telepathy.Logger.Log = Debug.Log;
        Telepathy.Logger.LogWarning = Debug.LogWarning;
        Telepathy.Logger.LogError = Debug.LogError;
    }

    void MPBroadcastMessage(List<int> clients, string message)
    {
        byte[] btText;
        btText = System.Text.Encoding.UTF8.GetBytes(message);

        foreach (int Cid in clients)
            server.Send(Cid, btText);

    }

    public int activepid = 0;
    void ServerDriver(List<int> clients, int ConnID, byte[] message)
    {
        var builder = new StringBuilder();
        string msg;
        foreach (byte B in message)
            builder.Append(Convert.ToChar(B));

        msg = builder.ToString();
        Debug.Log($"[ server driver ] PID {ConnID} MSG {msg}"); // 1 2
        string[] separator = { "_" };
        string[] strlist = msg.Split(separator, 20, System.StringSplitOptions.RemoveEmptyEntries);
        int proposedmove_x = Convert.ToInt32(strlist[1]);
        int proposedmove_y = Convert.ToInt32(strlist[2]);
        int proposed_pid = ConnID;

       // if (proposed_pid == activepid)
        //{
            if(tileowner[proposedmove_x, proposedmove_y] == 0)
            {
                tileowner[proposedmove_x, proposedmove_y] = proposed_pid;
                MPBroadcastMessage(clients, $"Pid_{proposed_pid}_on_{proposedmove_x}_{proposedmove_y}");
            }
       // }
    }


    void Update()
    {
        // client
        if (client.Connected)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                client.Send(new byte[] { 0x1 });

            // show all new messages
            Telepathy.Message msg;
            while (client.GetNextMessage(out msg))
            {
                switch (msg.eventType)
                {
                    case Telepathy.EventType.Connected:
                        Debug.Log("Connected");
                        break;
                    case Telepathy.EventType.Data:

                        var builder = new StringBuilder();
                        string parsed;
                        foreach (byte B in msg.data)
                            builder.Append(Convert.ToChar(B));
                        parsed = builder.ToString();

                        Debug.Log("Data: " + parsed);
                        GManager.GetComponent<Gmanager>().ClientRespond(parsed);
                        break;
                    case Telepathy.EventType.Disconnected:
                        Debug.Log("Disconnected");
                        break;
                }
            }
        }

        // server
        if (server.Active)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                server.Send(0, new byte[] { 0x2 });

            // show all new messages
            Telepathy.Message msg;
            while (server.GetNextMessage(out msg))
            {
                switch (msg.eventType)
                {
                    case Telepathy.EventType.Connected:
                        Debug.Log(msg.connectionId + " Connected");
                        break;
                    case Telepathy.EventType.Data:
                        Debug.Log($"klientID: {msg.connectionId}  Data:  {msg.data}");
                        List<int> clients = new List<int>();
                        clients.Add(1);
                        clients.Add(2);
                        ServerDriver(clients, msg.connectionId, msg.data);
                        break;
                    case Telepathy.EventType.Disconnected:
                        Debug.Log(msg.connectionId + " Disconnected");
                        break;
                }
            }
        }
    }

    void OnGUI()
    {
        // client
        GUI.enabled = !client.Connected;
        if (GUI.Button(new Rect(0, 0, 120, 20), "Connect Client"))
            client.Connect(Sip, 1337);

        GUI.enabled = client.Connected;
        if (GUI.Button(new Rect(130, 0, 120, 20), "Disconnect Client"))
            client.Disconnect();

        // server
        GUI.enabled = !server.Active;
        if (GUI.Button(new Rect(0, 25, 120, 20), "Start Server"))
        {
            tileowner = new int[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    tileowner[i, j] = 0;
            
            server.Start(1337);
        }

        GUI.enabled = server.Active;
        if (GUI.Button(new Rect(130, 25, 120, 20), "Stop Server"))
            server.Stop();

        GUI.enabled = true;
    }



    void OnApplicationQuit()
    {
        // the client/server threads won't receive the OnQuit info if we are
        // running them in the Editor. they would only quit when we press Play
        // again later. this is fine, but let's shut them down here for consistency
        client.Disconnect();
        server.Stop();
    }
}