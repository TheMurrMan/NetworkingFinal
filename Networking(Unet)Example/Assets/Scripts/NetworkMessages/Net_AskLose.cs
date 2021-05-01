[System.Serializable]

public class Net_AskLose : NetMessage
{
    public Net_AskLose()
    {
        code = NetCode.AskLose;
    }

    public int ownID;
}
