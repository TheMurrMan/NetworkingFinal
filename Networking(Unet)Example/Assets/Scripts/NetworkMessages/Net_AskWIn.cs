[System.Serializable]

public class Net_AskWin : NetMessage
{
    public Net_AskWin()
    {
        code = NetCode.AskWin;
    }

    public int ownID;
}

