#pragma warning disable 618

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
    public Vector3 oldPosition;
    public Vector3 newPosition;
    public bool isMoving = false;
    public Vector3 dir;
}

public class Client : MonoBehaviour
{
   /* private static Client _instance;

    public static Client m_Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Client>();
            }

            return _instance;
        }
    }*/

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

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

    private string ourPlayerName;

    public GameObject playerPrefab;
    public Player ownPlayer;

    private float updateTime;

    [SerializeField] public List<Player> players = new List<Player>();

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

        ourPlayerName = pName;

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

        RecievePackets();

        UpdateOtherPlayerPosition();
    }

    private void UpdateOtherPlayerPosition()
    {
        foreach (Player p in players)
        {
            //Don't update ourself
            if (p.connectionID == ourClientID) continue;

            if (updateTime > 0f)
            {
                updateTime -= Time.deltaTime;
                p.avatar.transform.position = Vector3.Lerp(p.oldPosition, p.newPosition, .1f);
            }
            else if (p.isMoving)
            {
                // Dead Reckoning

                // Assume the players never change speed and they are using the same speed 

                p.avatar.transform.position += p.dir * (ownPlayer.avatar.GetComponent<PlayerController>().moveSpeed * Time.deltaTime);
            }
        }
    }

    private void RecievePackets()
    {
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
                OnNewPlayer((Net_NewPlayerJoin) msg);
                break;
            case NetCode.AskPosition:
                OnAskPosition((Net_AskPosition) msg);
                break;
            case NetCode.Disconnect:
                PlayerDisconnected((Net_Disconnect) msg);
                break;
            case NetCode.SpawnBullet:
                OnSpawnBullet((Net_SpawnBullet)msg);
                break;
                
        }
    }


    private void OnSpawnBullet(Net_SpawnBullet msg)
    {
        Debug.Log("BulletSpawn Message Recieved");

        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 dir = new Vector3(msg.xDir, 0f, msg.zDir);
        
        GameObject g = Instantiate(FindObjectOfType<PlayerController>().bulletPrefab, pos, Quaternion.identity);
        g.transform.forward = dir;
        Destroy(g.GetComponent<Collider>());
    }

    private void OnNewPlayer(Net_NewPlayerJoin msg)
    {
        if (msg.cnnID == ourClientID) return;

        SpawnPlayer(msg.playerName, msg.cnnID);
    }

    private void OnAskName(Net_AskName msg)
    {
        // Set this clients ID
        ourClientID = msg.clientID;

        Net_NameIs nameIs = new Net_NameIs();
        nameIs.playerName = ourPlayerName;

        // Send our name to server
        SendServer(nameIs);

        // Create all other players
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
            if (ourClientID == msg.playerPositions[i].cnnID) continue;

            Vector3 pos = new Vector3(msg.playerPositions[i].x, msg.playerPositions[i].y, msg.playerPositions[i].z);
            Player p = players.Find(x => x.connectionID == msg.playerPositions[i].cnnID);

            Vector3 dir = new Vector3(msg.playerPositions[i].dirX, 0, msg.playerPositions[i].dirZ);

            Debug.Log("Current ID: " + msg.playerPositions[i].cnnID);

            if (msg.playerPositions[i].cnnID == 0) continue;

            p.oldPosition = p.newPosition;
            p.avatar.transform.position = p.newPosition;
            p.newPosition = pos;
            p.dir = dir;

            p.isMoving = msg.playerPositions[i].isMoving;
            updateTime = 0.1f;
        }

        // Send our position
        Vector3 myPos = ownPlayer.avatar.transform.position;

        Vector3 movement = ownPlayer.avatar.GetComponent<PlayerController>().movement;
        
        Net_MyPosition myPosition = new Net_MyPosition
            {ownID = ourClientID, x = myPos.x, y = myPos.y, z = myPos.z, dirX = movement.x, dirZ = movement.z, isMoving = true};

        if (movement == Vector3.zero)
            myPosition.isMoving = false;
        
        // Debug.Log(movement);

        SendServer(myPosition);
    }

    private void SpawnPlayer(string playerName, int cnnID)
    {
        GameObject go = Instantiate(playerPrefab, transform.position, Quaternion.identity);

        Player p = new Player();
        p.avatar = go;
        p.playerName = playerName;
        p.connectionID = cnnID;
        p.avatar.GetComponentInChildren<TextMesh>().text = playerName;

        var position = p.avatar.transform.position;
        p.oldPosition = position;
        p.newPosition = position;

        if (cnnID == ourClientID)
        {
            // Remove canvas
            GameObject.Find("Canvas").SetActive(false);
            go.AddComponent<PlayerController>();
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;

            go.AddComponent<CapsuleCollider>();
            p.avatar.GetComponentInChildren<TextMesh>().text = ourPlayerName;

            ownPlayer = p;
            isStarted = true;
        }

        players.Add(p);
    }

    private void PlayerDisconnected(Net_Disconnect msg)
    {
        Destroy(players.Find(x => x.connectionID == msg.cnnID).avatar);
        players.Remove(players.Find(x => x.connectionID == msg.cnnID));
    }

    public void OnBulletSpawn(GameObject bullet)
    {
        Vector3 vec = bullet.transform.position;
        Vector3 dir = bullet.transform.forward;
        //Send over spawn position and direction
        Net_SpawnBullet msg = new Net_SpawnBullet
        {
            ownerID = ourClientID,
            x = vec.x,
            y = vec.y,
            z = vec.z,
            xDir = dir.x,
            zDir = dir.z
        };
        
        Debug.Log("BulletSpawn Message Sent");
        SendServer(msg);
        
    }
    
    private void SendServer(NetMessage msg)
    {
        byte[] buffer = new byte[BYTE_SIZE];

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        NetworkTransport.Send(hostID, connectionID, reliableChannel, buffer, BYTE_SIZE, out error);
    }
}
#pragma warning restore 618