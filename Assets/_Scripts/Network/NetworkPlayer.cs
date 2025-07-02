using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local {get; set;}
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void Spawned()
    {
        if(Object.HasStateAuthority)
        {
            Local = this;

            Debug.Log("Spawned Local Player");
        }
        else
        {
            Debug.Log("Spawned Remote Player");
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if(player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}