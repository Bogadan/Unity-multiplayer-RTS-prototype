using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Health : NetworkBehaviour
{
    //Tutorial 14 - dealing damage; we want only the server to be
    //able to change the health variable, so it will be synchronized across the network!

    [SerializeField] private int maxHealth = 100;
    //we use hooks to synchronise variables across the server
    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;

    public event Action ServerOnDie;
    //tutorial 16 - healtg
    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if(currentHealth == 0) { return; }
        //prevent the health value from going negative value
        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if(currentHealth != 0) { return; }

        GetComponent<NetworkAnimator>().SetTrigger("Die");
        StartCoroutine(DespawnUnit());
        Debug.Log("We freaking died!");
        //we will raise an event to notify the server that a certain gameObject died;
        //the logic will be separated, because a building might die in a different way than a soldier

    }
    [Server]
    public IEnumerator DespawnUnit()
    {
        yield return new WaitForSeconds(5f);
        ServerOnDie?.Invoke();
    }

    #endregion

    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        //we raise this event for all clients
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }

    #endregion
}
