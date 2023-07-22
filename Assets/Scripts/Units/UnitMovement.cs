using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using UnityEngine.InputSystem;
using System;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] protected NavMeshAgent agent = null;
    [SerializeField] protected Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;
    //logica de baza pentru deplasare si pozitionarea caracterului!

    //componenta de animatie pe partea de client si pe partea de server
    [SerializeField] protected Animator animator;
    [SerializeField] protected NetworkAnimator networkAnimator;

    #region Server

    [ServerCallback]
    private void Update()
    {   //logica de urmarire a unui luptator
        //daca tinta nu se afla in distanta de atac al unitatii,
        //setam destinatia agentului de pathfiding la pozitia tintei

        Targetable target = targeter.GetTarget();

        Debug.Log(target);

        if(targeter.GetTarget() != null)
        {
            if( (target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
                
            }
            else if(agent.hasPath)
            {
                agent.ResetPath();
                
            }
        }

        //avem un offset de oprire a caracterului in pathfinding
        if(!agent.hasPath) { RpcStopMoveAnimation(); return; }
        if(agent.remainingDistance > agent.stoppingDistance) 
        {            
            return; 
        }
        agent.ResetPath();
    }
    //comanda de deplasare a caracterului pe partea de client
    [Command]
    public virtual void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }
    //comanda de deplasare a caracterului server-side
    [Server]
    public virtual void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) 
        { return; }

        agent.SetDestination(hit.position);

        RpcPlayMoveAnimation();
    }
    //serverul propaga apelul de functie pe toate masinile locale ale clientilor
    [ClientRpc]
    public void RpcPlayMoveAnimation()
    {
        networkAnimator.animator.SetBool("Moving", true);
    }

    public void RpcStopMoveAnimation()
    {
        networkAnimator.animator.SetBool("Moving", false);
    }
    #endregion
}