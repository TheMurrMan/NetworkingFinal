[System.Serializable]

public class Net_AskHealth : NetMessage
{
    public Net_AskHealth()
    {
        code = NetCode.AskHealth;
    }

    public int ownID;

    public float health;
    public string[] currentPlayers;
    public int[] currentIDs;
}
