using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Targeter : NetworkBehaviour
{
    [SerializeField] private Targetable target;

    //Tutorial 12 unit chasing
    public Targetable GetTarget()
    {
        return target;
    }

    #region Server

    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        if(!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }

        target = newTarget;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    #endregion

    #region Client
    #endregion
}
