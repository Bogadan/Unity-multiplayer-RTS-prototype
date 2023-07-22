using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using System;
using UnityEngine.InputSystem;

public class UnitBuilder : UnitMovement
{
   // [SerializeField] private Targeter targeter = null;
    [SerializeField] private float buildingSpeed = 1f;
    [SerializeField] private float miningSpeed = 1f;
    [SerializeField] private float rotationSpeed = 2000f;
  //  [SerializeField] private NavMeshAgent agent = null;

    //for knowing when we last gathered resources or built something...
    private float lastBuildingTime;
    private float lastMiningTime;

    //the animator component for switching animations
   // [SerializeField] private Animator animator;

    //for syncing the animation across the network; You cannot change the triggers directly in code when using mirror!
   // [SerializeField] NetworkAnimator networkAnimator;

    public Targetable target;

    public Targetable buildingTarget;

    //for the building and gather range
    [SerializeField] private float buildingRange = 2f;
    [SerializeField] private float miningRange = 1f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public Targeter GetTargeter()
    {
        return targeter;
    }

    public NavMeshAgent GetNavMeshAgent()
    {
        return agent;
    }


    [ServerCallback]
    private void Update()
    {
        
        if (!isServer)
            return;

        target = targeter.GetTarget();

        if (target == null)
        {
            return;
        }

        if(target.GetComponent<Building>() != null)
        {
            buildingTarget = target;
        }
        if(buildingTarget && ( Vector3.Distance(buildingTarget.transform.position, transform.position) < buildingRange) )
        {
            Quaternion targetRotation =
            Quaternion.LookRotation(buildingTarget.transform.position - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

            if (Time.time > (1 / buildingSpeed) + lastBuildingTime)
                {
                    RpcPlayConstructAnimation();
                    target.GetComponent<Building>().ConstructBuilding();
                    lastBuildingTime = Time.time;

                    if (target.GetComponent<Building>().getConstructionProgress() >= 100)
                    {
                        target.GetComponent<Building>().OnBuildingFinished();
                        target.GetComponent<Building>().isBuildingFinished = true;
                        RpcStopConstructAnimation();
                    }
                }
        }
        
    }
    [ClientRpc]
    private void RpcPlayConstructAnimation()
    {
        networkAnimator.animator.SetLayerWeight(1, 1f);
    }
    [ClientRpc]
    private void RpcStopConstructAnimation()
    {
        //networkAnimator.animator.SetBool("isConstructing", false);
        networkAnimator.animator.SetLayerWeight(1, 0f);
    }

}
