[System.Serializable]
public class Net_PlayerDamage : NetMessage
{
    public Net_PlayerDamage()
    {
        code = NetCode.PlayerDamage;
    }

    public int playerID;
    public int newHealth;
}
