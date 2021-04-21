[System.Serializable]
public class Net_SpawnEnemy : NetMessage
{
    public Net_SpawnEnemy()
    {
        code = NetCode.SpawnEnemy;
    }

    //So we know who the owner is
    public int enemyID;

    //Position
    public float x;
    public float y;
    public float z;

    //Direction
    public float xDir;
    public float zDir;
}
