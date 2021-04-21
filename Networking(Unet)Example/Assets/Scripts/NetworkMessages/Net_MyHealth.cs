[System.Serializable]
public class Net_MyHealth : NetMessage
{
    public Net_MyHealth()
    {
        code = NetCode.MyHealth;
    }

    public int ownID;

    public int health;
}
