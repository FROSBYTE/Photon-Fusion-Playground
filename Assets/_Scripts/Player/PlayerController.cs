using Fusion;
using UnityEngine;
using TMPro;

public struct NetworkInputData : INetworkInput
{
    public Vector3 movementInput;
}

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Combat Settings")]
    [SerializeField] private float damageCooldown = 2f;

    [Header("References")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI healthText;

    [Networked, OnChangedRender(nameof(OnHealthChanged))] 
    public int Health { get; set; } = 100;
    
    [Networked, OnChangedRender(nameof(OnPlayerNameChanged))] 
    public string PlayerName { get; set; }
    
    [Networked] public TickTimer DamageCooldownTimer { get; set; }
    
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Health = 100;
        }
        
        if (Object.HasInputAuthority)
        {
            RPC_SetName(UIManager.LocalPlayerName);
        }
        
        UpdateNameText();
        UpdateHealthText();
    }

    public override void FixedUpdateNetwork()
    {
        // Get input from the client that has input authority for this object
        if (GetInput<NetworkInputData>(out var input))
        {
            Vector3 move = new Vector3(input.movementInput.x, 0, input.movementInput.z);
            
            // Apply movement directly to transform
            // Fusion will automatically sync this via NetworkTransform or interpolation
            if (move.magnitude > 0.01f)
            {
                Vector3 movement = move.normalized * moveSpeed * Runner.DeltaTime;
                transform.position += movement;
            }
        }
    }
    
    private void OnHealthChanged()
    {
        UpdateHealthText();
        
        if (Health <= 0)
        {
            Debug.Log($"Player {PlayerName} is dead");
            
            if (Object.HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
        }
    }
    
    private void OnPlayerNameChanged()
    {
        UpdateNameText();
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetName(string newName)
    {
        PlayerName = newName;
    }
    
    private void UpdateNameText()
    {
        if (nameText != null)
        {
            nameText.text = PlayerName;
        }
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + Health.ToString();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (Object.HasStateAuthority && collision.gameObject.CompareTag("Player"))
        {
            if (DamageCooldownTimer.ExpiredOrNotRunning(Runner))
            {
                TakeDamage(10);
                Debug.Log($"Player {PlayerName} hit another player - damage applied");
                
                DamageCooldownTimer = TickTimer.CreateFromSeconds(Runner, damageCooldown);
            }
            else
            {
                Debug.Log($"Player {PlayerName} hit another player - but damage is on cooldown");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (!Object.HasStateAuthority)
            return;
            
        Health -= damage;
    }
}
