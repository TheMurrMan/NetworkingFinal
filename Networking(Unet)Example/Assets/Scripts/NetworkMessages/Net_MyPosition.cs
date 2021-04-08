[System.Serializable]
public class Net_MyPosition : NetMessage
{
    public Net_MyPosition()
    {
        code = NetCode.MyPosition;
    }

    public int ownID;
    public float x;
    public float y;
}
