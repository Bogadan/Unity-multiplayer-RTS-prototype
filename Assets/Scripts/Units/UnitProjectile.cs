using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;
    //tutorial 14 - dealing damage
    [SerializeField] private int damageToDeal = 20;
    void Start()
    {
        rb.velocity = transform.forward * launchForce;
    }
    //to destroy a networked object, in this case the projectile that we spawn,
    //we need to do it from the server
    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    //we use a server callback to tell mirror not to call this on the clients!
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {   //if we hit our own unit, as to say, the projectile belongs to the same client that instantiated it!
            if(networkIdentity.connectionToClient == connectionToClient) { return; }
        }
        //if the object that this bullet hit is valid, then reduce the enemy health and destroy this bullet!
        if(other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damageToDeal);
        }
        DestroySelf();
    }

    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }

}
