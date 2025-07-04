using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

// Network input structure to sync movement data across clients
public struct NetworkInputData : INetworkInput
{
    public Vector3 movementInput;
}

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local {get; set;}
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    // Use a networked position property instead of NetworkTransform
    [Networked] public Vector3 NetworkedPosition { get; set; }
    
    void Start()
    {
        
    }

    public override void Spawned()
    {
        // Initialize networked position
        if (Object.HasStateAuthority)
        {
            NetworkedPosition = transform.position;
        }
        
        if(Object.HasInputAuthority)
        {
            Local = this;
            Debug.Log($"Spawned Local Player - InputAuthority: {Object.InputAuthority}, StateAuthority: {Object.StateAuthority}");
        }
        else
        {
            Debug.Log($"Spawned Remote Player - InputAuthority: {Object.InputAuthority}, StateAuthority: {Object.StateAuthority}");
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        // Get input for this player
        if (GetInput<NetworkInputData>(out var input))
        {
            Vector3 move = new Vector3(input.movementInput.x, 0, input.movementInput.z);
            
            // Only the state authority can modify networked properties
            if (Object.HasStateAuthority && move.magnitude > 0.01f)
            {
                Vector3 movement = move.normalized * moveSpeed * Runner.DeltaTime;
                NetworkedPosition += movement;
                Debug.Log($"StateAuth moving player {Object.InputAuthority}: {NetworkedPosition}");
            }
        }
    }
    
    public override void Render()
    {
        // All clients apply the networked position to their visual transform
        transform.position = NetworkedPosition;
    }

    public void PlayerLeft(PlayerRef player)
    {
        if(player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}