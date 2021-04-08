[System.Serializable]
public class Net_NameIs : NetMessage
{
    public Net_NameIs()
    {
        code = NetCode.NameIs;
    }

    public string playerName;
}
