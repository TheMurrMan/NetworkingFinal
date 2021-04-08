using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[Serializable]
public class Player
{
    public string playerName;
    public GameObject avatar;
    public int connectionID;
}

public class Client : MonoBehaviour
{
    private const int MAX_CONNECTION = 100;
    private const int PORT = 5701;
    private const int BYTE_SIZE = 1024;

    private int hostID;

    private int webHostID;

    private int reliableChannel;
    private int unreliableChannel;

    private int ourClientID;
    private int connectionID;

    private float connectionTime;
    private bool isConnected = false;
    private bool isStarted = false;
    private byte error;

    private string playerName;

    public GameObject playerPrefab;
    public List<Player> players = new List<Player>();

    public void Connect()
    {
        Debug.Log("Connecting");
        // Does the player have a name
        string pName = GameObject.Find("NameInput").GetComponent<InputField>().text;

        if (pName == "")
        {
            Debug.Log("Please Enter A Name", this);
            return;
        }

        playerName = pName;

        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostID = NetworkTransport.AddHost(topo, 0);

        connectionID = NetworkTransport.Connect(hostID, "127.0.0.1", PORT, 0, out error);

        connectionTime = Time.time;

        isConnected = true;
    }

    private void Update()
    {
        if (!isConnected)
        {
            return;
        }

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData =
            NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetMessage msg = (NetMessage) formatter.Deserialize(ms);

                OnData(connectionId, channelId, recHostId, msg);

                /* string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                 Debug.Log("Recieving: " + msg);
 
                 string[] splitData = msg.Split('|');
                 switch (splitData[0])
                 {
                     case "ASKNAME":
                         OnAskName(splitData);
                         break;
 
                     case "CNN":
                         SpawnPlayer(splitData[1], int.Parse(splitData[2]));
                         break;
 
                     case "DC":
                         PlayerDisconnected(int.Parse(splitData[1]));
                         break;
 
                     case "ASKPOSITION":
                         OnAskPosition(splitData);
                         break;
 
                     default:
                         Debug.Log("Invalid Message: " + msg);
                         break;
                 }*/

                break;
        }
    }


    //This will handle our network messages
    private void OnData(int ccnId, int channelId, int recHostId, NetMessage msg)
    {
        switch (msg.code)
        {
            case NetCode.None:
                break;
            case NetCode.AskName:
                OnAskName((Net_AskName) msg);
                break;
            case NetCode.NewPlayer:
                Net_NewPlayerJoin newPlayer = (Net_NewPlayerJoin) msg;
                SpawnPlayer(newPlayer.playerName, newPlayer.cnnID);
                break;
            case NetCode.AskPosition:
                OnAskPosition((Net_AskPosition) msg);
                break;
            case NetCode.Disconnect:
                PlayerDisconnected((Net_Disconnect) msg);
                break;
        }
    }

    private void OnAskName(Net_AskName msg)
    {
        // Set this clients ID
        ourClientID = msg.clientID;

        Net_NameIs nameIs = new Net_NameIs();
        nameIs.playerName = playerName;

        // Send our name to server
        //Send("NAMEIS|" + playerName, reliableChannel);
        SendServer(nameIs);

        // Create all other players
        /*for (int i = 2; i < data.Length - 1; ++i)
        {
            string[] d = data[i].Split('%');
            SpawnPlayer(d[0], int.Parse(data[1]));
        }*/

        for (int i = 0; i < msg.currentPlayers.Length; i++)
        {
            //We don't want to spawn ourselves;
            if (i == ourClientID) continue;
            SpawnPlayer(msg.currentPlayers[i], msg.currentIDs[i]);
        }
    }

    private void OnAskPosition(Net_AskPosition msg)
    {
        if (!isStarted)
        {
            return;
        }

        // Update everyone else
        for (int i = 0; i < msg.playerPositions.Length; ++i)
        {
            /*string[] d = data[i].Split('%');

            // Prevent server from updating us
            if (ourClientID != int.Parse(d[0]))
            {
                Vector3 pos = new Vector3(float.Parse(d[1]), float.Parse(d[2]));

                players[int.Parse(d[0])].avatar.transform.position = pos;
            }*/

            if (ourClientID == msg.playerPositions[i].cnnID) continue;

            Vector3 pos = new Vector3(msg.playerPositions[i].x, msg.playerPositions[i].y);
            Player p = players.Find(x => x.connectionID == msg.playerPositions[i].cnnID);

            foreach (var t in players)
            {
                if (t.connectionID == msg.playerPositions[i].cnnID)
                {
                    Debug.Log("Found ID: " + t.connectionID + "in list");
                }
            }
            
            p.avatar.transform.position = pos;
            //players[msg.playerPositions[i].cnnID].avatar.transform.position = pos;
        }

        // Send our position
        Vector3 myPos = players.Find(x => x.connectionID == ourClientID).avatar.transform.position;
        Net_MyPosition myPosition = new Net_MyPosition {ownID = ourClientID, x = myPos.x, y = myPos.y};
        SendServer(myPosition);
    }

    private void SpawnPlayer(string playerName, int cnnID)
    {
        GameObject go = Instantiate(playerPrefab, transform.position, Quaternion.identity);

        if (cnnID == ourClientID)
        {
            // Remove canvas
            GameObject.Find("Canvas").SetActive(false);
            go.AddComponent<PlayerMovement>();
            isStarted = true;
        }

        Player p = new Player();
        p.avatar = go;
        p.playerName = playerName;
        p.connectionID = cnnID;
        p.avatar.GetComponentInChildren<TextMesh>().text = playerName;
        players.Add(p);
    }

    private void PlayerDisconnected(Net_Disconnect msg)
    {
        Destroy(players[msg.cnnID].avatar);
        players.Remove(players.Find(x => x.connectionID == msg.cnnID));
    }

    /* private void Send(string message, int channelID)
     {
         Debug.Log("Sending: " + message);
         byte[] msg = Encoding.Unicode.GetBytes(message);
 
         NetworkTransport.Send(hostID, connectionID, channelID, msg, message.Length * sizeof(char), out error);
     }*/

    private void SendServer(NetMessage msg)
    {
        byte[] buffer = new byte[BYTE_SIZE];

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        NetworkTransport.Send(hostID, connectionID, reliableChannel, buffer, BYTE_SIZE, out error);
    }
}