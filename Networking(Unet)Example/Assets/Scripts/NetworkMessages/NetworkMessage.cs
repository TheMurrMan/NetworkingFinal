public static class NetCode
{
    public const int None = 0;
    public const int AskName = 1;
    public const int NameIs = 2;
    public const int NewPlayer = 3;
    public const int AskPosition = 4;
    public const int MyPosition = 5;
    public const int Disconnect = 6;
    public const int SpawnBullet = 7;
}


[System.Serializable]
public abstract class NetMessage
{
    public byte code { set; get; }

    public NetMessage()
    {
        code = NetCode.None;
    }
}