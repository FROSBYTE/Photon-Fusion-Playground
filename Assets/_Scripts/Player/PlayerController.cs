using Fusion;
using UnityEngine;

// Network input structure to sync movement data across clients
public struct NetworkInputData : INetworkInput
{
    public Vector3 movementInput;
}

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

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
}
