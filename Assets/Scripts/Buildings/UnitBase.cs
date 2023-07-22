using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;


public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health;

    public static event Action<UnitBase> ServerOnBaseSpawned;
    public static event Action<UnitBase> ServerOnBaseDespawned;

    #region Server
    //look up docs on how action subscribing works in unity
    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;

        ServerOnBaseSpawned?.Invoke(this);
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);

        
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;

        ServerOnBaseDespawned?.Invoke(this);
    }

    #endregion

    #region Client



    #endregion
}
