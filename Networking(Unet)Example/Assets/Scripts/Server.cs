#pragma warning disable 618

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class ServerClient
{
    public int connectionID;
    public string playerName;
    public Vector3 position;

    public int health;
    public int score;
    public float dirX;
    public float dirZ;
    public bool isMoving;
}

public class Server : MonoBehaviour
{
    private const int MAX_CONNECTION = 100;

    private int port = 5701;

    private const int BYTE_SIZE = 1024;

    private int hostID;

    private int webHostID;

    private int reliableChannel;
    private int unreliableChannel;


    private bool isStarted = false;

    private byte error;

    public List<ServerClient> clients = new List<ServerClient>();

    private float lastMovementUpdate;
    private float movementUpdateRate = 0.1f;

    private AIManager AIManager;

    private void Awake()
    {
        AIManager = FindObjectOfType<AIManager>();
    }

    private void Start()
    {
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostID = NetworkTransport.AddHost(topo, port, null);

        webHostID = NetworkTransport.AddWebsocketHost(topo, port, null);

        isStarted = true;
    }

    private void Update()
    {
        if (!isStarted)
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
            case NetworkEventType.ConnectEvent:
                Debug.Log("Player " + connectionId + " Has Connected");
                OnConnection(connectionId);
                break;

            case NetworkEventType.DataEvent:

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetMessage msg = (NetMessage) formatter.Deserialize(ms);

                OnData(connectionId, channelId, recHostId, msg);
                break;

            case NetworkEventType.DisconnectEvent:
                Debug.Log("Player " + connectionId + " Has Disconnected");
                OnDisconnection(connectionId);
                break;
        }

        // Ask player for position
        if (Time.time - lastMovementUpdate > movementUpdateRate)
        {
            lastMovementUpdate = Time.time;

            //Sending Player Position

            Net_AskPosition askPosition = new Net_AskPosition
            {
                playerPositions = new Net_AskPosition.position[clients.Count + 1]
            };

            for (int i = 0; i < clients.Count; i++)
            {
                askPosition.playerPositions[i].x = clients[i].position.x;
                askPosition.playerPositions[i].y = clients[i].position.y;
                askPosition.playerPositions[i].z = clients[i].position.z;
                askPosition.playerPositions[i].cnnID = clients[i].connectionID;
                askPosition.playerPositions[i].dirX = clients[i].dirX;
                askPosition.playerPositions[i].dirZ = clients[i].dirZ;
                askPosition.playerPositions[i].isMoving = clients[i].isMoving;
            }

            Send(askPosition, clients);

            //Sending Enemy Position
            Net_UpdateEnemyPosition enemyPosition = new Net_UpdateEnemyPosition
            {
                enemies = new Net_UpdateEnemyPosition.position[AIManager.aiList.Count + 1]
            };

            for (int i = 0; i < AIManager.aiList.Count; i++)
            {
                GameObject e = AIManager.aiList[i];
                Vector3 pos = e.transform.position;
                Vector3 forward = e.transform.forward;

                enemyPosition.enemies[i].enemyID = e.GetComponent<AIController>().myId;
                enemyPosition.enemies[i].x = pos.x;
                enemyPosition.enemies[i].y = pos.y;
                enemyPosition.enemies[i].z = pos.z;
                enemyPosition.enemies[i].xDir = forward.x;
                enemyPosition.enemies[i].zDir = forward.z;
            }
            
            Send(enemyPosition, clients);
        }
    }

    //This will handle our network messages
    private void OnData(int ccnId, int channelId, int recHostId, NetMessage msg)
    {
        switch (msg.code)
        {
            case NetCode.None:
                break;
            case NetCode.NameIs:
                OnNameIs(ccnId, (Net_NameIs) msg);
                break;
            case NetCode.MyPosition:
                OnMyPosition((Net_MyPosition) msg);
                break;
            case NetCode.SpawnBullet:
                OnSpawnBullet((Net_SpawnBullet) msg);
                break;
            case NetCode.MyHealth:
                OnMyHealth((Net_MyHealth)msg);
                break;
            case NetCode.MyScore:
                OnMyScore((Net_MyScore)msg);
                break;
        }
    }

    private void OnSpawnBullet(Net_SpawnBullet msg)
    {
        //Spawn Bullets in server scene
        //Send Positions back to everyone

        Send(msg, clients);
    }

    private void OnConnection(int cnnID)
    {
        // Add him to list
        ServerClient c = new ServerClient();
        c.connectionID = cnnID;
        c.playerName = "Temp";

        clients.Add(c);

        // When player joins server, say ID

        // Request name, send name of all other players

        Net_AskName askName = new Net_AskName();
        askName.clientID = cnnID;
        askName.currentPlayers = new string[clients.Count + 1];
        askName.currentIDs = new int[clients.Count + 1];


        for (int i = 0; i < clients.Count; i++)
        {
            askName.currentPlayers[i] = clients[i].playerName;
            askName.currentIDs[i] = clients[i].connectionID;
        }


        if (clients.Count >= 2)
        {
            CreateAIManager();
        }

        Send(askName, cnnID);
    }

    private void CreateAIManager()
    {
        Instantiate(Resources.Load("AIManager") as GameObject);
    }

    public void SpawnEnemy(GameObject enemy)
    {
        Vector3 vec = enemy.transform.position;
        Vector3 dir = enemy.transform.forward;

        //Send over spawn position and direction
        Net_SpawnEnemy msg = new Net_SpawnEnemy
        {
            enemyID = enemy.GetComponent<AIController>().myId,
            x = vec.x,
            y = vec.y,
            z = vec.z,
            xDir = dir.x,
            zDir = dir.z
        };

        Debug.Log("Enemy Spawn Message Sent");
        Send(msg, clients);
    }

    public void OnMyHealth(Net_MyHealth msg)
    {
        ServerClient client = clients.Find(c => c.connectionID == msg.ownID);
        client.health = msg.health;
    }

    public void OnMyScore(Net_MyScore msg)
    {
        ServerClient client = clients.Find(c => c.connectionID == msg.ownID);
        client.score = msg.score;
    }
    private void OnDisconnection(int cnnID)
    {
        // remove this player from list
        clients.Remove(clients.Find(x => x.connectionID == cnnID));

        // tell all player some has disconnected
        Net_Disconnect msg = new Net_Disconnect {cnnID = cnnID};
        Send(msg, clients);
    }

    private void OnNameIs(int cnnID, Net_NameIs msg)
    {
        // Link name to connection ID
        clients.Find(x => x.connectionID == cnnID).playerName = msg.playerName;

        // Tell everyone new player connected
        Net_NewPlayerJoin newPlayerJoin = new Net_NewPlayerJoin();
        newPlayerJoin.playerName = msg.playerName;
        newPlayerJoin.cnnID = cnnID;

        Send(newPlayerJoin, clients);
    }

    private void OnMyPosition(Net_MyPosition msg)
    {
        ServerClient client = clients.Find(c => c.connectionID == msg.ownID);
        // Debug.Log(client.connectionID);
        //Debug.Log(msg.ownID);
        client.position = new Vector3(msg.x, msg.y, msg.z);
        client.dirX = msg.dirX;
        client.dirZ = msg.dirZ;
        client.isMoving = msg.isMoving;
    }

    private void Send(NetMessage msg, int cnnID)
    {
        List<ServerClient> c = new List<ServerClient>();
        c.Add(clients.Find(x => x.connectionID == cnnID));
        Send(msg, c);
    }

    private void Send(NetMessage msg, List<ServerClient> c)
    {
        byte[] buffer = new byte[BYTE_SIZE];

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        foreach (ServerClient sc in c)
        {
            NetworkTransport.Send(hostID, sc.connectionID, reliableChannel, buffer, BYTE_SIZE, out error);
        }
    }
}
#pragma warning enable 618