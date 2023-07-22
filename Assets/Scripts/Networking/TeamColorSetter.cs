using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TeamColorSetter : NetworkBehaviour
{   
    [SerializeField] protected Renderer[] colorRenderers = new Renderer[0];

    [SyncVar(hook = nameof(HandleTeamColorUpdated))]
    protected Color teamColor = new Color();

    #region Server

    public override void OnStartServer()
    {
        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        teamColor = player.GetTeamColor();
    }

    #endregion
    #region Client

    protected virtual void HandleTeamColorUpdated(Color oldColor, Color newColor)
    {
        foreach(Renderer renderer in colorRenderers)
        {
        renderer.GetComponentInChildren<Renderer>().material.SetColor("_Color", newColor);
        }
    }

    #endregion
}
