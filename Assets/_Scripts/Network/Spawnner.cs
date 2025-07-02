using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;

public class Spawnner : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPlayer PlayerPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        Debug.Log("Object Enter AOI");
    }
   
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log("Disconnected from server");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("Connect Failed");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("Connect Request");
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("Host Migration");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        SceneManager.LoadScene(0);
        Debug.Log("Shutdown");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if(runner.IsServer)
        {
            Debug.Log("Spawning Player");
            runner.Spawn(PlayerPrefab, Vector3.zero, Quaternion.identity, player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        
        // Gather movement input
        data.movementInput = new Vector3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical")
        );
        
        input.Set(data);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        Debug.Log("Object Exit AOI");
        //throw new System.NotImplementedException();
    }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        Debug.Log("Reliable Data Received");
        //throw new System.NotImplementedException();
    }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        Debug.Log("Reliable Data Progress");
        //throw new System.NotImplementedException();
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server");
        //throw new System.NotImplementedException();
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        Debug.Log("User Simulation Message");
        //throw new System.NotImplementedException();
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        Debug.Log("Input Missing");
        //throw new System.NotImplementedException();
    }
   
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Left");
        //throw new System.NotImplementedException();
    }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        Debug.Log("Custom Authentication Response");
        //throw new System.NotImplementedException();
    }
  
    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("Scene Load Start");
        //throw new System.NotImplementedException();
    }
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene Load Done");
        //throw new System.NotImplementedException();
    }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("Session List Updated");
        //throw new System.NotImplementedException();
    }
}
