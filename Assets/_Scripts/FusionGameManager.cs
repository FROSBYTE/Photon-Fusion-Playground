using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

// Network input structure for player movement
public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
}

public class FusionGameManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner runnerPrefab;
    public GameObject playerPrefab;

    private NetworkRunner _runner;
    private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
    private UIManager _uiManager;

    void Start()
    {
        _uiManager = FindObjectOfType<UIManager>();
    }

    public async void CreateRoom(string roomName)
    {

        _runner = Instantiate(runnerPrefab);
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            // When creating a room, you're always the host
            _uiManager?.OnGameConnected(true);
        }
        else
        {
            Debug.LogError($"Failed to create room: {result.ShutdownReason}");
            //_uiManager?.OnGameFailed($"Failed to create room: {result.ShutdownReason}");
        }
    }

    public async void JoinRoom(string roomName)
    {

        _runner = Instantiate(runnerPrefab);
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            // When joining an existing room, you're a client
            _uiManager?.OnGameConnected(false);
        }
        else
        {
            Debug.LogError($"Failed to join room: {result.ShutdownReason}");
            //_uiManager?.OnGameFailed($"Failed to join room: {result.ShutdownReason}");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedPlayers.ContainsKey(player))
        {
            return;
        }
        
        if (runner.CanSpawn && player == runner.LocalPlayer)
        {
            Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-2f, 2f), 0.5f, UnityEngine.Random.Range(-2f, 2f));
            var spawnedPlayer = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
            
            if (spawnedPlayer != null)
            {
                _spawnedPlayers[player] = spawnedPlayer;
            }
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) 
    { 
        var data = new NetworkInputData();
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            data.direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            data.direction += Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            data.direction += Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            data.direction += Vector3.right;
        
        input.Set(data);
    }
    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    { 
        Debug.Log($"Network shutdown: {shutdownReason}");
        
        // Clean up all spawned players regardless of shutdown reason
        CleanupAllPlayers();
        
        _runner = null;
    }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) 
    { 
        Debug.Log($"Disconnected from server: {reason}");
        
        // If we disconnect, clean up our local player references
        CleanupAllPlayers();
    }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    { 
        Debug.Log($"Player {player} left the game");
        
        // Check if this is the local player leaving
        bool isLocalPlayer = player == runner.LocalPlayer;
        if (isLocalPlayer)
        {
            Debug.Log("Local player is leaving the game");
        }
        
        if (_spawnedPlayers.TryGetValue(player, out NetworkObject playerObject))
        {
            if (playerObject != null)
            {
                // In Shared mode or if we have spawning authority, despawn the object
                if (runner.GameMode == GameMode.Shared || runner.IsServer)
                {
                    Debug.Log($"Despawning player object for {player}");
                    runner.Despawn(playerObject);
                }
                else
                {
                    // If we can't despawn it through Fusion, at least destroy the GameObject locally
                    Debug.Log($"Destroying player object locally for {player}");
                    Destroy(playerObject.gameObject);
                }
            }
            _spawnedPlayers.Remove(player);
        }
        else
        {
            Debug.Log($"No spawned player found for {player} in dictionary");
        }
    }
    
    private void CleanupAllPlayers()
    {
        foreach (var kvp in _spawnedPlayers.ToArray())
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }
        _spawnedPlayers.Clear();
    }
    
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
}
