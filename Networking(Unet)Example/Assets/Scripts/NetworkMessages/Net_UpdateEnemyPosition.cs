[System.Serializable]
public class Net_UpdateEnemyPosition : NetMessage
{
    public Net_UpdateEnemyPosition()
    {
        code = NetCode.UpdateEnemyPosition;
    }

    [System.Serializable]
    public struct position
    {
        //So we know who the owner is
        public int enemyID;

        //Position
        public float x;
        public float y;
        public float z;

        //Direction
        public float xDir;
        public float zDir;
        
        public bool isMoving;

    }

    public position[] enemies;

}
