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
    public const int SpawnEnemy = 8;
    public const int MyHealth = 9;
    public const int AskHealth = 10;
    public const int MyScore = 11;
    public const int AskScore = 12;
    public const int UpdateEnemyPosition = 13;
    public const int EnemyDeath = 14;
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