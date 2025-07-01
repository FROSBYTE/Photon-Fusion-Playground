using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

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
            SceneManager = null // Don't use scene manager to avoid camera destruction
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
            SceneManager = null // Don't use scene manager to avoid camera destruction
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
        
        // Notify UI first, before any cleanup
        if (_uiManager != null && _uiManager.gameObject != null)
        {
            _uiManager.OnSessionEnded($"Session ended: {shutdownReason}");
        }
        
        // Clean up all spawned players regardless of shutdown reason
        CleanupAllPlayers();
        
        _runner = null;
    }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) 
    { 
        Debug.Log($"Disconnected from server: {reason}");
        
        // Notify UI first if it still exists
        if (_uiManager != null && _uiManager.gameObject != null)
        {
            _uiManager.OnSessionEnded($"Disconnected from server: {reason}");
        }
        
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
        if (_spawnedPlayers != null)
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
    }
    

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ForceDisconnect()
    {
        Debug.Log("Received RPC_ForceDisconnect → Disconnecting self");
        FindObjectOfType<FusionGameManager>()?.DisconnectSelf();
    }


    public async void QuitSession()
    {
        if (_runner != null)
        {
            Debug.Log("Ending session...");

            // 🟢 Step 1: Broadcast to all peers to disconnect (via RPC)
            foreach (var kvp in _spawnedPlayers)
        {
            if (kvp.Key == _runner.LocalPlayer && kvp.Value != null)
            {
                // Try to get PlayerCubeController and call the RPC on it
                if (kvp.Value.TryGetBehaviour(out PlayerCubeController controller))
                {
                    // Call the RPC method on the FusionGameManager instead of the controller
                    RPC_ForceDisconnect();
                    Debug.Log("Sent RPC_ForceDisconnect to all players");
                }
                else
                {
                    Debug.LogWarning("Player prefab does not have PlayerCubeController or was not set up correctly for RPC.");
                }
                break;
            }
        }


            // Small delay to let RPC propagate
            await Task.Delay(300);

            // 🟡 Step 2: Protect the main camera from being destroyed
            Camera mainCamera = Camera.main;
            bool wasMainCameraDontDestroy = false;
            if (mainCamera != null)
            {
                if (mainCamera.gameObject.scene.name == "DontDestroyOnLoad")
                {
                    wasMainCameraDontDestroy = true;
                }
                else
                {
                    DontDestroyOnLoad(mainCamera.gameObject);
                    Debug.Log("Protected main camera during shutdown");
                }
            }

            // 🟣 Step 3: Inform UI
            _uiManager?.OnSessionEnded("Session ended");

            try
            {
                // Step 4: Shutdown runner
                _runner.RemoveCallbacks(this);
                await _runner.Shutdown();

                if (_runner != null && _runner.gameObject != null)
                    Destroy(_runner.gameObject);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Exception during runner shutdown: {ex.Message}");
            }

            _runner = null;

            // Step 5: Clean up players
            CleanupAllPlayers();

            // 🔵 Step 6: Restore camera back to scene
            if (mainCamera != null && !wasMainCameraDontDestroy)
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(mainCamera.gameObject, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                Debug.Log("Restored main camera to scene after shutdown");
            }
        }
    }


    /// <summary>
    /// Disconnects only the local player from the session, leaving other players connected
    /// </summary>
  public void DisconnectSelf()
{
    if (_runner != null)
    {
        Debug.Log("Disconnecting local player from session...");
        
        // Protect the main camera during disconnect
        Camera mainCamera = Camera.main;
        bool wasMainCameraDontDestroy = false;
        if (mainCamera != null)
        {
            // Check if camera is already DontDestroyOnLoad
            if (mainCamera.gameObject.scene.name == "DontDestroyOnLoad")
            {
                wasMainCameraDontDestroy = true;
            }
            else
            {
                DontDestroyOnLoad(mainCamera.gameObject);
                Debug.Log("Protected main camera during disconnect");
            }
        }
        
        // Notify UI first, before any cleanup
        if (_uiManager != null)
        {
            _uiManager.OnSessionEnded("Disconnected from session");
        }
        
        try
        {
            _runner.RemoveCallbacks(this);
            
            // In Shared mode, shutdown the runner to disconnect the local player
            _ = _runner.Shutdown();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Exception during disconnect: {ex.Message}");
        }

        CleanupAllPlayers();

        _runner = null;

        // Restore camera to scene if we protected it
        if (mainCamera != null && !wasMainCameraDontDestroy)
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(mainCamera.gameObject, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("Restored main camera to scene after disconnect");
        }

        Debug.Log("Local player disconnected successfully");
    }
}

    
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
}
