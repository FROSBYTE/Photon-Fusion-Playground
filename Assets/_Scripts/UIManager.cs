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

    private FusionGameManager fusionGameManager;

    void Start()
    {
        fusionGameManager = FindObjectOfType<FusionGameManager>();
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
        if (statusText != null)
            statusText.text = message;
    }

    public void OnGameConnected(bool isHost)
    {
        if (isHost)
        {
            UpdateStatus("Connected as Host");
        }
        else
        {
            UpdateStatus("Connected as Client");
        }
        gameplayPanel.SetActive(false);
    }
}
