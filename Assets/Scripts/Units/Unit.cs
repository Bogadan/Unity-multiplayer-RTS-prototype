using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{   //logica pe partea de network legata de interactiunea unitatii
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;

    //referinta la tinta curenta a luptatorului
    [SerializeField] private Targeter targeter = null;

    //stocarea unitatii pentru RTSPlayer in lista de unitati existente;
    //utilizam eventuri C# sa stim cand spawnam si despawnam pe server o unitate
    //autorizarea pe partea de server a instantierii unitatii
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    //autorizarea pe partea de client a instantierii unitatii
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    [SerializeField] private Health health;

    //cat costa un luptator
    [SerializeField] private int resourceCost = 10;

    [SerializeField] private CharacterController characterController;

    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }
    
    public Targeter GetTargeter()
    {
        return targeter;
    }

    public int GetResourceCost()
    {
        return resourceCost;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);

        health.ServerOnDie += ServerHandleDie;
    }
    public override void OnStopServer()
    {
        ServerOnUnitDespawned?.Invoke(this);

        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    public void Select()
    {
        if(!hasAuthority) { return; }

        onSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        if(!hasAuthority) { return; }
        onDeselected?.Invoke();
    }

    
    public override void OnStartAuthority()
    {
        if(!isClientOnly) { return; }
        if(!hasAuthority) { return; }
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {        
        if (!hasAuthority) { return; }
        AuthorityOnUnitDespawned?.Invoke(this);
    }
    #endregion
}
