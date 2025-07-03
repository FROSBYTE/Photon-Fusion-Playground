using Fusion;
using UnityEngine;
using TMPro;

// Network input structure to sync movement data across clients
public struct NetworkInputData : INetworkInput
{
    public Vector3 movementInput;
}

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("References")]
    [SerializeField] TextMeshProUGUI nameText;
    
    // Networked property to sync player name across all clients
    [Networked] public string PlayerName { get; set; }
    
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            // Set the name only for the local player (the one with state authority)
            RPC_SetName(UIManager.LocalPlayerName);
        }
        
        // Update the name text when spawned
        UpdateNameText();
    }

    public override void FixedUpdateNetwork()
    {
        // Only process movement for objects with state authority
        if (GetInput<NetworkInputData>(out var input))
        {
            // Handle movement
            Vector3 move = new Vector3(input.movementInput.x, 0, input.movementInput.z);
            
            // Apply movement using transform
            transform.Translate(move * moveSpeed * Runner.DeltaTime);
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetName(string newName)
    {
        PlayerName = newName;
        UpdateNameText();
    }
    
    private void UpdateNameText()
    {
        if (nameText != null)
        {
            nameText.text = PlayerName;
        }
    }
}
