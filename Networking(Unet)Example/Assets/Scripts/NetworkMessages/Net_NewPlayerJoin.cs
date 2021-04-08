[System.Serializable]
public class Net_NewPlayerJoin : NetMessage
{
   public Net_NewPlayerJoin()
   {
      code = NetCode.NewPlayer;
   }

   public string playerName;
   public int cnnID;
}
