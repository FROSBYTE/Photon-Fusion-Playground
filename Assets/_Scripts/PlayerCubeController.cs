using Fusion;
using UnityEngine;

public class PlayerCubeController : NetworkBehaviour
{
    public float speed = 5f;

    void Start()
    {
        // Debug info to track spawning
        Debug.Log($"PlayerCubeController started - HasStateAuthority: {HasStateAuthority}, HasInputAuthority: {HasInputAuthority}, Object: {Object}");
        
        // Visual feedback - change color based on authority
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (HasInputAuthority)
            {
                renderer.material.color = Color.green; // Your player
            }
            else
            {
                renderer.material.color = Color.red; // Other players
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Use Fusion's networked input system
        if (GetInput<NetworkInputData>(out NetworkInputData input))
        {
            Vector3 movement = input.direction.normalized * speed * Runner.DeltaTime;
            transform.position += movement;
        }
    }
}
