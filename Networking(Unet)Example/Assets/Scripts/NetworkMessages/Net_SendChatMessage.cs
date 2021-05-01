[System.Serializable]
public class Net_SendChatMessage : NetMessage
{
    public Net_SendChatMessage()
    {
        code = NetCode.ChatMessage;
    }

    public string username;
    public string message;
}