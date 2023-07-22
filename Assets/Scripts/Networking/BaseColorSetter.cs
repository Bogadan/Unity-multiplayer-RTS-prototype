using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BaseColorSetter : TeamColorSetter
{
    #region Client

    override protected void HandleTeamColorUpdated(Color oldColor, Color newColor)
    {
        foreach (Renderer renderer in colorRenderers)
        {
renderer.GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", newColor);
        }
    }

    #endregion
}
