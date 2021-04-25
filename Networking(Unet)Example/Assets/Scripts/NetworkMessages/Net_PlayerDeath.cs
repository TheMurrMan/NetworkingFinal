[System.Serializable]
public class Net_PlayerDeath : NetMessage
{
    public Net_PlayerDeath()
    {
        code = NetCode.PlayerDeath;
    }
    public int playerID;
}
