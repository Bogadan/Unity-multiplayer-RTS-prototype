using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using System;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    //Distrugerea unitatilor si a cladirilor
    [SerializeField] private Health health = null;
    //Referinta la soldatul/unitatea pe care cladirea poate antrena!
    [SerializeField] private Unit unitPrefab = null; 
    [SerializeField] private Transform unitSpawnPoint = null;

    //implementam crearea de luptatori prin mecanismul de coada pentru a preveni productia continua de unitati
    //si vom implementa o bara de progres!
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitProgressImage = null;
    //numarul maxim de luptatori ce pot fi produsi la un moment dat
    [SerializeField] private int maxUnitQueue = 5; 
    [SerializeField] private float spawnMoveRange = 7;
    //durata de recrutare pentru un luptator
    [SerializeField] private float unitSpawnDuration = 5f; 

    //sincronizam pe server numarul de unitati ce sunt in proces de recrutare
    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    //cat de rapid se incarca bara de progres in productia luptatorilor
    private float progressImageVelocity;

    private void Update()
    {
        if(isServer)
        {
            //serverul se ocupa de producerea de noi luptatori
            ProduceUnits();
        }

        if(isClient)
        {
            //clientul se ocupa de updatarea elementelor de UI
            UpdateTimerDisplay();
        }
    }



    #region Server

    [Server]
    private void ProduceUnits()
    {
        if(queuedUnits == 0) { return; }

        unitTimer += Time.deltaTime; //incrementarea contorizatorului

        if(unitTimer < unitSpawnDuration) { return; }

        GameObject unitInstance = 
        Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);

        //offsetul pentru locatia de instantiere a unitatii

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnPoint.position.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        queuedUnits--;
        unitTimer = 0f;
    }

    //delegatul pentru distrugerea si dealocarea cladirii pe server cand este distrusa
    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject); 
        //dealocarea cladirii de pe server cand este distrusa...
    }

    [Command]
    private void CmdSpawnUnit()
    {
        /*
         * atunci cand dorim sa instantiem un obiect pe server, 
         * trebuie mai intai sa cream o copie 
         * a acelui obiect pe o versiune locala a serverului (si a host-ului principal,
         * daca este o conexiune de tip server-client-client), 
         * iar apoi prin comanda spawn 
         * sa propagam cate o copie a obiectului respectiv la fiecare client in parte
         */
        if(queuedUnits == maxUnitQueue) { return; }

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if(player.GetResources() < unitPrefab.GetResourceCost()) { return; }

        queuedUnits++;

        player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());
    }
    #endregion

    #region Client
    //event din noul input sistem Unity
    public void OnPointerClick(PointerEventData eventData) 
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }

        if(!hasAuthority) { return; }

        CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitsText.text = newUnits.ToString();
    }

    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration; 
        //calculam procentual cat la suta din
        //imaginea de generare a unei unitati trebuie sa fie desenata;
        //imaginea este desenata radial!

        if(newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
        unitProgressImage.fillAmount = 
        Mathf.SmoothDamp(unitProgressImage.fillAmount, 
        newProgress, ref progressImageVelocity, 0.1f); 
            //schimbam gradual vechea valoare de umplere a culorii cu noua valoare
            //pentru a nu aparea discrepante in afisare
        }
    }

    #endregion
}