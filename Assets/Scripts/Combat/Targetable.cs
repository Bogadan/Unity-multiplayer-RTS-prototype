using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : NetworkBehaviour
{
    //a tag to identify if an object is targetable by an enemy

    [SerializeField] private Transform aimAtPoint = null;

    public Transform GetAimAtPoint()
    {
        return aimAtPoint;
    }
}
