[System.Serializable]
public class Net_SpawnBullet : NetMessage
{
    public Net_SpawnBullet()
    {
        code = NetCode.SpawnBullet;
    }

    //So we know who the owner is
    public int ownerID;

    public int bulletID;
    //Position
    public float x;
    public float y;
    public float z;
    
    //Direction
    public float xDir;
    public float zDir;
}
