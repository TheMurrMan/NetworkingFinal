using System;

[Serializable]
public class Net_EnemyDamage : NetMessage
{
    public Net_EnemyDamage()
    {
        code = NetCode.EnemyDamage;
    }

    public int enemyID;
    public int newHealth;
    public int bulletID;
}