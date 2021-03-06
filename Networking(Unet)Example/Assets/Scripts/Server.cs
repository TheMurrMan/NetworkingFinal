#pragma warning disable 618

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;


[System.Serializable]
public class ServerClient
{
    public int connectionID;
    public string playerName;
    public Vector3 position;

    public int health = 100;
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

    [SerializeField] private int playersDead = 0;
    private int bulletID = 0;
    public List<BulletController> bullets = new List<BulletController>();

    public bool lose = false;
    public bool win = false;
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

            AIManager aiManager = FindObjectOfType<AIManager>();

            if (!aiManager) return;

            //Sending Enemy Position
            Net_UpdateEnemyPosition enemyPosition = new Net_UpdateEnemyPosition
            {
                enemies = new Net_UpdateEnemyPosition.position[aiManager.aiList.Count + 1]
            };

            for (int i = 0; i < aiManager.aiList.Count; i++)
            {
                GameObject e = aiManager.aiList[i];
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

            if(playersDead == 2)
			{
                Debug.Log("Lose");
                OnLose();
			}
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
                OnMyHealth((Net_MyHealth) msg);
                break;
            case NetCode.MyScore:
                OnMyScore((Net_MyScore) msg);
                break;
            case NetCode.ChatMessage:
                OnChatMessage((Net_SendChatMessage) msg);
                break;
        }
    }

    private void OnChatMessage(Net_SendChatMessage msg)
    {
        Net_SendChatMessage newMessage = new Net_SendChatMessage()
        {
            message = msg.username + ": " + msg.message
        };
        
        Send(newMessage, clients);
    }
    
    private void OnSpawnBullet(Net_SpawnBullet msg)
    {
        //Spawn Bullets in server scene
        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 dir = new Vector3(msg.xDir, 0f, msg.zDir);
        GameObject bullet = Resources.Load("Bullet") as GameObject;
        GameObject g = Instantiate(bullet, pos, Quaternion.identity);
        g.transform.forward = dir;
        g.GetComponent<BulletController>().myID = bulletID;
        msg.bulletID = bulletID;
        bulletID++;

        bullets.Add(g.GetComponent<BulletController>());

        Send(msg, clients);
    }

    public void RemoveBulletFromList(int bulletID)
    {
        foreach (var b in bullets)
        {
            if (b.myID == bulletID)
            {
                bullets.Remove(b);
                Destroy(b.gameObject);
                break;
            }
        }
    }

    private void OnConnection(int cnnID)
    {
        // When player joins server, say ID

        // Request name, send name of all other players

        Net_AskName askName = new Net_AskName();
        askName.clientID = cnnID;
        askName.currentPlayers = new string[clients.Count];
        askName.currentIDs = new int[clients.Count];


        for (int i = 0; i < clients.Count; i++)
        {
            askName.currentPlayers[i] = clients[i].playerName;
            askName.currentIDs[i] = clients[i].connectionID;
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

    public void OnWin()
	{
        Net_AskWin msg = new Net_AskWin()
        {
            
        };
        Send(msg, clients);
    }

    public void OnLose()
    {
        Net_AskLose msg = new Net_AskLose()
        {
            
        };
        Send(msg, clients);
    }

    public void OnEnemyDeath(int id)
    {
        Net_EnemyDeath msg = new Net_EnemyDeath()
        {
            enemyID = id
        };
        Send(msg, clients);
    }

    public void OnPlayerDeath(int id)
	{
        Net_PlayerDeath msg = new Net_PlayerDeath() { playerID = id};
        playersDead++;
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
        // Add him to list
        ServerClient c = new ServerClient();
        c.connectionID = cnnID;
        c.playerName = msg.playerName;

        clients.Add(c);

        // Link name to connection ID
        //clients.Find(x => x.connectionID == cnnID).playerName = msg.playerName;

        // Tell everyone new player connected
        Net_NewPlayerJoin newPlayerJoin = new Net_NewPlayerJoin();
        newPlayerJoin.playerName = msg.playerName;
        newPlayerJoin.cnnID = cnnID;

        Send(newPlayerJoin, clients);

        if (clients.Count >= 2)
        {
            CreateAIManager();
        }
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

    public void OnTakeDamage(AIController enemy, int id)
    {
        Net_EnemyDamage enemyDamage = new Net_EnemyDamage()
        {
            enemyID = enemy.myId,
            newHealth = enemy.GetHealth(),
            bulletID = id
        };

        RemoveBulletFromList(id);
        Send(enemyDamage, clients);
    }

    public void OnPlayerTakeDamage(ServerClient client)
    {
        client.health -= 2;

        if (client.health > 0)
        {
            Net_PlayerDamage playerDamage = new Net_PlayerDamage()
            {
                newHealth = client.health,
                playerID = client.connectionID,
            };
            Send(playerDamage, clients);
        }

        else
		{
            Debug.Log("die");
            OnPlayerDeath(client.connectionID);
		}
        
    }

    private void Send(NetMessage msg, int cnnID)
    {
        byte[] buffer = new byte[BYTE_SIZE];

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        NetworkTransport.Send(hostID, cnnID, reliableChannel, buffer, BYTE_SIZE, out error);
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