[System.Serializable]

public class Net_AskScore : NetMessage
{
    public Net_AskScore()
    {
        code = NetCode.AskScore;
    }

    public int ownID;

    public float score;
    public string[] currentPlayers;
    public int[] currentIDs;
}
