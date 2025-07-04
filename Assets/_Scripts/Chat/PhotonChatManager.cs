using System;
using UnityEngine;
using Photon.Chat;
using ExitGames.Client.Photon;

public class PhotonChatManager : MonoBehaviour, IChatClientListener
{
    [Header("Photon Chat Settings")]
    [Tooltip("Your Photon Chat AppId (the Chat Key you provided)")]
    public string ChatAppId;
    [Tooltip("Unique username for the chat (e.g. PhotonNetwork.NickName)")]
    public string UserName;
    [Tooltip("Channel name to join/publish (e.g. room name or \"Global\")")]
    public string ChannelName = "Global";

    private ChatClient chatClient;

    // Fired when any message is received or sent
    public static event Action<string, string> OnMessageReceived;

    void Start()
    {
        ConnectToChat();
    }

    void Update()
    {
        // Pump the chat service
        if (chatClient != null)
            chatClient.Service();
    }

    public void ConnectToChat()
    {
        if (chatClient != null && chatClient.CanChat)
            return;

        chatClient = new ChatClient(this);
        chatClient.ChatRegion = "US";  // change if needed
        chatClient.Connect(ChatAppId, "1.0", new Photon.Chat.AuthenticationValues(UserName));
    }

    /// <summary>
    /// Call this to send a new chat message.
    /// </summary>
    public void SendChatMessage(string message)
    {
        if (chatClient != null && chatClient.CanChat)
        {
            chatClient.PublishMessage(ChannelName, message);
            // show your own sent message immediately
            OnMessageReceived?.Invoke(UserName, message);
        }
    }

    #region IChatClientListener

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log($"[Chat][{level}] {message}");
    }

    public void OnChatStateChange(ChatState state) { /* optional */ }

    public void OnConnected()
    {
        Debug.Log("Photon Chat: Connected");
        chatClient.Subscribe(new string[] { ChannelName });
    }

    public void OnDisconnected()
    {
        Debug.Log("Photon Chat: Disconnected");
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < messages.Length; i++)
        {
            OnMessageReceived?.Invoke(senders[i], messages[i].ToString());
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName) { }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log($"Photon Chat: Subscribed to {string.Join(", ", channels)}");
    }

    public void OnUnsubscribed(string[] channels)
    {
        Debug.Log($"Photon Chat: Unsubscribed from {string.Join(", ", channels)}");
    }

    public void OnUserSubscribed(string channel, string user) { }

    public void OnUserUnsubscribed(string channel, string user) { }

    #endregion
}
