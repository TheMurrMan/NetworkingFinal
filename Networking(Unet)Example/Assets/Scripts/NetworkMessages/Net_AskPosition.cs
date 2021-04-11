[System.Serializable]
public class Net_AskPosition : NetMessage
{
    public Net_AskPosition()
    {
        code = NetCode.AskPosition;
    }

    [System.Serializable]
    public struct position
    {
        public int cnnID;
        public float x;
        public float y;
        public float z;
    }

    public position[] playerPositions;
}