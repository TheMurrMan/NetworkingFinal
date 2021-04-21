[System.Serializable]
public class Net_MyScore : NetMessage
{
    public Net_MyScore()
    {
        code = NetCode.MyScore;
    }

    public int ownID;

    public int score;
}
