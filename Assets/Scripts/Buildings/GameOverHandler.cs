using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameOverHandler : NetworkBehaviour
{   //Logica doar pe partea de server; Obiectul nu apartine de niciun client
    //aceasta este logica asociata cu managerul de retea care va determina
    //cate baze au mai ramas in joc
    private List<UnitBase> bases = new List<UnitBase>();

    //Construim un UI care declanseaza "Game over" cand ramane o singura baza

    public static event Action<string> ClientOnGameOver;
    public static event Action ServerOnGameOver;

    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    public void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        if (bases.Count != 1) { return; }

        
        var playerColor = 
        bases[0].connectionToClient.identity.GetComponent<RTSPlayer>().GetTeamColor();

        var colorName = "";

        //Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta

        if (playerColor == Color.blue)
        {
            colorName = "albastru";
        }
        else
        if (playerColor == Color.red)
        {
            colorName = "rosu";
        }
        else
        if (playerColor == Color.green)
        {
            colorName = "verde";
        }
        else
        if (playerColor == Color.yellow)
        {
            colorName = "galben";
        }
        else
        if (playerColor == Color.cyan)
        {
            colorName = "cian";
        }
        else
        if (playerColor == Color.magenta)
        {
            colorName = "magenta";
        }

        RpcGameOver($"Jucatorul {colorName}"); //the playerID
    }

    #endregion

    //Trebuie sa ii spunem serverului fiecarui client ca jocul s-a terminat;
    //facem asta printr-un ClientRpc
    #region Client

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        //raise client-side event
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
