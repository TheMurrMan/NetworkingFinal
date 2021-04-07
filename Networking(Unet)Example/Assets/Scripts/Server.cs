using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;

public class ServerClient
{
    public int connectionID;
    public string playerName;
    public Vector3 position;
}
public class Server : MonoBehaviour
{
    private const int MAX_CONNECTION = 100;

    private int port = 5701;

    private int hostID;

    private int webHostID;

    private int reliableChannel;
    private int unreliableChannel;


    private bool isStarted = false;

    private byte error;

    private List<ServerClient> clients = new List<ServerClient>();

    private float lastMovementUpdate;
    private float movementUpdateRate = 0.5f;

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
        if(!isStarted)
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

        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.ConnectEvent:
                Debug.Log("Player " + connectionId + " Has Connected");
                OnConnection(connectionId);
                break;

            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Recieving from " + connectionId + " : " + msg);

                string[] splitData = msg.Split('|');
                switch (splitData[0])
                {
                    case "NAMEIS":
                        OnNameIs(connectionId,splitData[1]);
                        break;

                    case "MYPOSITION":
                        OnMyPosition(connectionId,float.Parse(splitData[1]), float.Parse(splitData[2]));
                        break;

                    default:
                        Debug.Log("Invalid Message: " + msg);
                        break;
                }
                break;

            case NetworkEventType.DisconnectEvent:
                Debug.Log("Player " + connectionId + " Has Disconnected");
                OnDisconnection(connectionId);
                break;
        }

        // Ask player for position
        if(Time.time - lastMovementUpdate > movementUpdateRate)
		{
            lastMovementUpdate = Time.time;
            string message = "ASKPOSITION|";

            foreach(ServerClient sc in clients)
			{
                 message += sc.connectionID.ToString() + '%' + sc.position.x.ToString() + '%' + sc.position.y.ToString() + '|';
			}
            message = message.Trim('|');
            Send(message, unreliableChannel, clients);
        }
    }

    private void OnConnection(int cnnID)
	{
        // Add him to list
        ServerClient c = new ServerClient();
        c.connectionID = cnnID;
        c.playerName = "Temp";

        clients.Add(c);

        // When player joins server, sey ID

        // Request name, send name of all other players
        string msg = "ASKNAME|" + cnnID + "|";

        foreach(ServerClient sc in clients)
		{
            msg += sc.playerName + "%" + sc.connectionID + "|";
		}

        msg = msg.Trim('|');

        Send(msg, reliableChannel, cnnID);
	}
    private void OnDisconnection(int cnnID)
    {
        // remove this player from list
        clients.Remove(clients.Find(x => x.connectionID == cnnID));

        // tell all player some has disconnected
        Send("DC|" + cnnID, reliableChannel, clients);
    }
    private void OnNameIs(int cnnID, string playerName)
	{
        // Link name to connection ID
        clients.Find(x => x.connectionID == cnnID).playerName = playerName;

		// Tell everyone new player connected
		Send("CNN|" + playerName + "|"+ cnnID, reliableChannel,clients);

	}

    private void OnMyPosition(int cnnID, float x, float y)
	{
        clients.Find(c => c.connectionID == cnnID).position = new Vector3(x, y, 0);
	}
    private void Send(string message, int channelID, int cnnID)
	{
        List<ServerClient> c = new List<ServerClient>();
        c.Add(clients.Find(x => x.connectionID == cnnID));
        Send(message, channelID, c);

	}
    private void Send(string message, int channelID, List<ServerClient> c)
    {
        Debug.Log("Sending: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);

        foreach(ServerClient sc in c)
		{
            NetworkTransport.Send(hostID, sc.connectionID, channelID, msg, message.Length * sizeof(char), out error);
		}
    }
}
