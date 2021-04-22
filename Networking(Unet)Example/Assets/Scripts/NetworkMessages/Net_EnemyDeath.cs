[System.Serializable]
public class Net_EnemyDeath : NetMessage
{
   public Net_EnemyDeath()
   {
      code = NetCode.EnemyDeath;
   }

   public int enemyID;
}
