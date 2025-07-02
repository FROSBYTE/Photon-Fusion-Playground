using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gamePanel;
    
    [Header("Menu UI Elements")]
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_Text statusText;
    
    [Header("Game UI Elements")]
    [SerializeField] private Button leaveGameButton;
    [SerializeField] private Button endSessionButton;
    
    [Header("Network Settings")]
    [SerializeField] private NetWorkRunnerHandler networkHandler;
    
    private bool isHost = false;
    private string currentRoomName = "";
    private int currentPlayerCount = 0;
    
    void Start()
    {
        SetupUI();
        ShowMenuPanel();
        UpdateStatusText("Ready to connect");
    }
    
    void SetupUI()
    {
        // Setup button listeners
        if (createLobbyButton != null)
            createLobbyButton.onClick.AddListener(CreateLobby);
            
        if (joinLobbyButton != null)
            joinLobbyButton.onClick.AddListener(JoinLobby);
            
        if (leaveGameButton != null)
            leaveGameButton.onClick.AddListener(LeaveGame);
            
        if (endSessionButton != null)
            endSessionButton.onClick.AddListener(EndSession);
            
        // Set default room name
        if (roomNameInput != null)
            roomNameInput.text = "TestRoom";
    }
    
    public void CreateLobby()
    {
        string roomName = roomNameInput != null ? roomNameInput.text : "TestRoom";
        
        if (string.IsNullOrEmpty(roomName))
        {
            UpdateStatusText("Please enter a room name");
            return;
        }
        
        currentRoomName = roomName;
        isHost = true;
        
        UpdateStatusText("Creating lobby...");
        networkHandler.StartAsHost(roomName, OnNetworkStarted);
    }
    
    public void JoinLobby()
    {
        string roomName = roomNameInput != null ? roomNameInput.text : "TestRoom";
        
        if (string.IsNullOrEmpty(roomName))
        {
            UpdateStatusText("Please enter a room name");
            return;
        }
        
        currentRoomName = roomName;
        isHost = false;
        
        UpdateStatusText("Joining lobby...");
        networkHandler.StartAsClient(roomName, OnNetworkStarted);
    }
    
    public void LeaveGame()
    {
        UpdateStatusText("Leaving game...");
        
        // Only local player leaves - destroy local player and disconnect
        networkHandler.LeaveGame();
        
        isHost = false;
        currentRoomName = "";
        currentPlayerCount = 0;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ShowMenuPanel();
        UpdateStatusText("Left the game");
    }
    
    public void EndSession()
    {
        if (!isHost)
        {
            UpdateStatusText("Only host can end session");
            return;
        }
        
        UpdateStatusText("Ending session...");
        
        // Host ends session for everyone - removes all players and shuts down
        networkHandler.EndSession();
        
        isHost = false;
        currentRoomName = "";
        currentPlayerCount = 0;
        
        ShowMenuPanel();
        UpdateStatusText("Session ended");
    }
    
    private void OnNetworkStarted(bool success, string message)
    {
        if (success)
        {
            ShowGamePanel();
            string roleText = isHost ? "Host" : "Client";
            UpdateStatusText($"Connected as {roleText} to {currentRoomName}");
        }
        else
        {
            UpdateStatusText($"Failed to connect: {message}");
        }
    }
    
    public void OnPlayerCountChanged(int playerCount)
    {
        currentPlayerCount = playerCount;
        string roleText = isHost ? "Host" : "Client";
        UpdateStatusText($"Connected as {roleText} | Players: {playerCount} | Room: {currentRoomName}");
    }
    
    private void ShowMenuPanel()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
        
        // Enable/disable buttons based on state
        if (createLobbyButton != null) createLobbyButton.interactable = true;
        if (joinLobbyButton != null) joinLobbyButton.interactable = true;
    }
    
    private void ShowGamePanel()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
        
        // Show end session button only for host
        if (endSessionButton != null) 
            endSessionButton.gameObject.SetActive(isHost);
    }
    
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;
        
        Debug.Log($"Status: {message}");
    }
    
    // Called when network events occur
    public void OnPlayerJoined(PlayerRef player, int totalPlayers)
    {
        OnPlayerCountChanged(totalPlayers);
    }
    
    public void OnPlayerLeft(PlayerRef player, int totalPlayers)
    {
        OnPlayerCountChanged(totalPlayers);
    }
    
    public void OnNetworkError(string error)
    {
        UpdateStatusText($"Network Error: {error}");
        ShowMenuPanel();
    }
    
    public void OnLocalPlayerLeft()
    {
        // Called when only the local player has left
        isHost = false;
        currentRoomName = "";
        currentPlayerCount = 0;
        ShowMenuPanel();
        UpdateStatusText("Left the game");
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (createLobbyButton != null) createLobbyButton.onClick.RemoveAllListeners();
        if (joinLobbyButton != null) joinLobbyButton.onClick.RemoveAllListeners();
        if (leaveGameButton != null) leaveGameButton.onClick.RemoveAllListeners();
        if (endSessionButton != null) endSessionButton.onClick.RemoveAllListeners();
    }
} 