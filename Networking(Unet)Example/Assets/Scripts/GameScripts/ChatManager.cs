using System;
using TMPro;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text chatMessages;
    private Client client;
    private void Awake()
    {
        client = FindObjectOfType<Client>();
    }

    public void SendChatMessage(string message)
    {
        if (!Input.GetKeyDown(KeyCode.Return)) return;
        if (string.IsNullOrWhiteSpace(message)) return;
        
        //Call Function in Client to send message
        client.SendChatMessage(message);
        
        inputField.text = String.Empty;
    }

    public void HandleNewMessage(string message)
    {
        chatMessages.text += "\n" + message;
    }
}