using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class PhotonChatUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public Button sendButton;
    public TMP_Text chatDisplay;      // should be inside a ScrollRect

    void OnEnable()
    {
        PhotonChatManager.OnMessageReceived += AppendMessage;
        sendButton.onClick.AddListener(OnSendClicked);
    }

    void OnDisable()
    {
        PhotonChatManager.OnMessageReceived -= AppendMessage;
        sendButton.onClick.RemoveListener(OnSendClicked);
    }

    private void AppendMessage(string sender, string message)
    {
        chatDisplay.text += $"<b>{sender}:</b> {message}\n";
        // Optionally scroll to bottom here
    }

    private void OnSendClicked()
    {
        string msg = inputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        var mgr = FindObjectOfType<PhotonChatManager>();
        if (mgr != null)
        {
            mgr.SendChatMessage(msg);
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }
}
