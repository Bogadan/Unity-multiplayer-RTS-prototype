using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    //contorizator sa stim cat timp a trecut de la ultimul atac
    private float lastFireTime;
    [SerializeField] private int damageToDeal = 20;

    //componenta de animatie
    [SerializeField] private Animator animator;

    public Targetable target;

    //pentru sincronizarea animatiilor in retea/oferit de Mirror
    [SerializeField] NetworkAnimator networkAnimator;

    [ServerCallback]
    private void Update()
    {
        target = targeter.GetTarget();

        if(target == null) { return; }

        if(!CanFireAtTarget()) { return; }
        //rotatia unitatii in directia tintei
        Quaternion targetRotation =
            Quaternion.LookRotation(target.transform.position - transform.position);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        //verificam daca putem ataca din nou
        if((Time.time) > (1 / fireRate) + lastFireTime)
        {
            //declansam animatia de atac
            RpcPlayAttackAnimation();
            //resetarea contorizatorului
            lastFireTime = Time.time;
        }
    }
    //va fi apelat printr-un trigger programat in animatia de atac a unitatii,
    //nu direct in cod
    private void Attack()
    {
        if (target.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damageToDeal);
        }
    }
    
    //initiaza animatiile de atac pe toti clientii
    [ClientRpc]
    private void RpcPlayAttackAnimation()
    {
        networkAnimator.animator.SetBool("Moving", false);
        int chooseATK = UnityEngine.Random.Range(1, 3);
        string atkName = "Attack" + chooseATK;
        networkAnimator.SetTrigger(atkName);
    }

    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position 
            - transform.position).sqrMagnitude <= fireRange * fireRange;
    }
}
