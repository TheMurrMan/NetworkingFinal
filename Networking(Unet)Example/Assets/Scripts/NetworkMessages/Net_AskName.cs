
[System.Serializable]
public class Net_AskName : NetMessage
{
    public Net_AskName()
    {
        code = NetCode.AskName;
    }

    public int clientID = -1;
    public string[] currentPlayers;
    public int[] currentIDs;
}