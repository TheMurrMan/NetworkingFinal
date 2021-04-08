[System.Serializable]
public class Net_Disconnect : NetMessage
{
   public Net_Disconnect()
   {
      code = NetCode.Disconnect;
   }

   //Id of user that disconnected
   public int cnnID;

}
