using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using Fusion.Sockets;
using System.Threading.Tasks;

public class NetWorkRunnerHandler : MonoBehaviour
{
    public NetworkRunner netWorkRunnerPrefab;
    
    private NetworkRunner networkRunner;
    
    // Callback for UI updates
    public System.Action<bool, string> OnNetworkStarted;
    
    void Start()
    {
        // Don't auto-start network anymore - let UI control it
        Debug.Log("NetworkRunnerHandler ready - waiting for UI commands");
    }
    
    public async void StartAsHost(string roomName, System.Action<bool, string> callback = null)
    {
        OnNetworkStarted = callback;
        
        try
        {
            await StartNetwork(GameMode.Host, roomName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start as host: {e.Message}");
            OnNetworkStarted?.Invoke(false, e.Message);
        }
    }
    
    public async void StartAsClient(string roomName, System.Action<bool, string> callback = null)
    {
        OnNetworkStarted = callback;
        
        try
        {
            await StartNetwork(GameMode.Client, roomName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start as client: {e.Message}");
            OnNetworkStarted?.Invoke(false, e.Message);
        }
    }
    
    private async Task StartNetwork(GameMode gameMode, string sessionName)
    {
        if (networkRunner != null)
        {
            Debug.LogWarning("Network runner already exists");
            return;
        }
        
        // Create network runner
        networkRunner = Instantiate(netWorkRunnerPrefab);
        networkRunner.name = $"Network Runner ({gameMode})";
        
        // Don't destroy on load
        DontDestroyOnLoad(networkRunner.gameObject);
        
        Debug.Log($"Starting {gameMode} with session: {sessionName}");
        
        var result = await InitializeNetworkRunner(
            networkRunner, 
            gameMode, 
            NetAddress.Any(), 
            SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), 
            sessionName,
            OnGameStarted
        );
        
        if (result.Ok)
        {
            Debug.Log($"Successfully started as {gameMode}");
            OnNetworkStarted?.Invoke(true, $"Connected as {gameMode}");
        }
        else
        {
            Debug.LogError($"Failed to start network: {result.ShutdownReason}");
            OnNetworkStarted?.Invoke(false, result.ShutdownReason.ToString());
            
            // Cleanup on failure
            if (networkRunner != null)
            {
                Destroy(networkRunner.gameObject);
                networkRunner = null;
            }
        }
    }
    
    private void OnGameStarted(NetworkRunner runner)
    {
        Debug.Log("Game started successfully");
    }
    
    public void LeaveGame()
    {
        if (networkRunner != null)
        {
            Debug.Log("Local player leaving game");
            
            // Find and despawn local player's prefab
            if (NetworkPlayer.Local != null)
            {
                Debug.Log("Despawning local player");
                networkRunner.Despawn(NetworkPlayer.Local.Object);
            }
            
            // Disconnect only this client
            networkRunner.Shutdown(true, ShutdownReason.Ok);
            Destroy(networkRunner.gameObject);
            networkRunner = null;
        }
    }
    
    public void EndSession()
    {
        if (networkRunner != null && networkRunner.IsServer)
        {
            Debug.Log("Host ending session for all players");
            
            // Despawn all player objects
            var allPlayers = FindObjectsOfType<NetworkPlayer>();
            foreach (var player in allPlayers)
            {
                if (player.Object != null)
                {
                    networkRunner.Despawn(player.Object);
                }
            }
            
            // Shutdown the entire session
            networkRunner.Shutdown(true, ShutdownReason.HostMigration);
            Destroy(networkRunner.gameObject);
            networkRunner = null;
        }
        else
        {
            Debug.LogWarning("Only host can end session");
        }
    }
    
    public void Shutdown()
    {
        if (networkRunner != null)
        {
            Debug.Log("Shutting down network runner");
            networkRunner.Shutdown();
            Destroy(networkRunner.gameObject);
            networkRunner = null;
        }
    }
    
    protected virtual Task<StartGameResult> InitializeNetworkRunner(
        NetworkRunner runner, 
        GameMode gameMode, 
        NetAddress address, 
        SceneRef scene, 
        string sessionName,
        Action<NetworkRunner> initialized)
    {
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
        if (sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        
        runner.ProvideInput = true;
        
        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            SceneManager = sceneManager,
            OnGameStarted = initialized,
        });
    }
    
    // Public properties for UI to check state
    public bool IsConnected => networkRunner != null && networkRunner.IsRunning;
    public bool IsHost => networkRunner != null && networkRunner.IsServer;
    public bool IsClient => networkRunner != null && networkRunner.IsClient;
    public int PlayerCount => networkRunner != null ? networkRunner.ActivePlayers.Count() : 0;
    
    void OnDestroy()
    {
        Shutdown();
    }
}
