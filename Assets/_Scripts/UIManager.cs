using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameplayPanel;
    public TMP_InputField createRoomInput;
    public TMP_InputField joinRoomInput;
    public TextMeshProUGUI statusText;
    public Button quitSessionButton; // Only visible to host

    private FusionGameManager fusionGameManager;

    void Start()
    {
        fusionGameManager = FindObjectOfType<FusionGameManager>();
        
        // Initially hide the quit session button
        if (quitSessionButton != null)
            quitSessionButton.gameObject.SetActive(false);
    }

    public void OnCreateRoomClicked()
    {
        string roomName = createRoomInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            createRoomInput.text = roomName;
        }

        UpdateStatus("Creating room: " + roomName);
        Debug.Log("Creating room: " + roomName);
        fusionGameManager.CreateRoom(roomName);
        gameplayPanel.SetActive(false);
    }

    public void OnJoinRoomClicked()
    {
        string roomName = joinRoomInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            UpdateStatus("Please enter a room name to join");
            Debug.Log("Please enter a room name to join");
            return;
        }

        UpdateStatus("Joining room: " + roomName);
        Debug.Log("Joining room: " + roomName);
        fusionGameManager.JoinRoom(roomName);
        gameplayPanel.SetActive(false);
    }

    public void UpdateStatus(string message)
    {
        if (statusText != null && statusText.gameObject != null)
            statusText.text = message;
    }

    public void OnGameConnected(bool isHost)
    {
        if (isHost)
        {
            UpdateStatus("Connected as Host");
            // Show quit session button only for host
            if (quitSessionButton != null)
                quitSessionButton.gameObject.SetActive(true);
        }
        else
        {
            UpdateStatus("Connected as Client");
            // Hide quit session button for clients
            if (quitSessionButton != null)
                quitSessionButton.gameObject.SetActive(false);
        }
        gameplayPanel.SetActive(false);
    }

    /// <summary>
    /// Quits the current session, disconnecting all players and cleaning up the network state
    /// </summary>
    public void QuitSession()
    {
        UpdateStatus("Quitting session...");
        Debug.Log("Quitting session initiated by user");
        
        if (fusionGameManager != null)
        {
            fusionGameManager.QuitSession();
        }
        
        // Hide quit session button
        if (quitSessionButton != null)
            quitSessionButton.gameObject.SetActive(false);
        
        // Reset UI to allow rejoining
        gameplayPanel.SetActive(true);
        UpdateStatus("Session ended. You can create or join a new room.");
        
        // Clear input fields
        if (createRoomInput != null)
            createRoomInput.text = "";
        if (joinRoomInput != null)
            joinRoomInput.text = "";
    }

    /// <summary>
    /// Disconnects only the local player from the session, leaving other players connected
    /// </summary>
    public void DisconnectSelf()
    {
        UpdateStatus("Disconnecting...");
        Debug.Log("Disconnecting local player from session");
        
        if (fusionGameManager != null)
        {
            fusionGameManager.DisconnectSelf();
        }
        
        // Hide quit session button
        if (quitSessionButton != null)
            quitSessionButton.gameObject.SetActive(false);
        
        // Reset UI to allow rejoining
        gameplayPanel.SetActive(true);
        UpdateStatus("Disconnected. You can create or join a new room.");
        
        // Clear input fields
        if (createRoomInput != null)
            createRoomInput.text = "";
        if (joinRoomInput != null)
            joinRoomInput.text = "";
    }

    /// <summary>
    /// Called when the session ends (for any reason) - shows gameplay panel for all players
    /// </summary>
    public void OnSessionEnded(string reason)
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("UIManager or its GameObject has been destroyed, skipping OnSessionEnded");
            return;
        }
        
        Debug.Log($"Session ended: {reason}");
        
        // Hide quit session button
        if (quitSessionButton != null && quitSessionButton.gameObject != null)
            quitSessionButton.gameObject.SetActive(false);
        
        // Reset UI to allow rejoining
        if (gameplayPanel != null)
            gameplayPanel.SetActive(true);
            
        UpdateStatus($"Session ended: {reason}. You can create or join a new room.");
        
        // Clear input fields
        if (createRoomInput != null)
            createRoomInput.text = "";
        if (joinRoomInput != null)
            joinRoomInput.text = "";
    }
}
