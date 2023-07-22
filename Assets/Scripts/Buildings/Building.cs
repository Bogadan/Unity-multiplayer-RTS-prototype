using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Building : NetworkBehaviour
{
    [SerializeField] private Sprite icon = null;
    [SerializeField] private int id = -1;
    [SerializeField] private int price = 100;

    //autorizarea pe partea de server pentru cladire; pentru instantiere si dealocare
    public static event Action<Building> ServerOnBuildingSpawned;
    public static event Action<Building> ServerOnBuildingDespawned;
    //autorizarea pe partea de client pentru cladire
    public static event Action<Building> AuthorityOnBuildingSpawned;
    public static event Action<Building> AuthorityOnBuildingDespawned;

    //obiectul ce va retine previzualizarea cladirii
    [SerializeField] private GameObject buildingPreview = null;

    //variabile pentru starea nefinalizata a cladirii...
    [SerializeField] private GameObject buildingSkeleton = null;
    [SerializeField] private int constructionProgress = 0;
    public static event Action<Building> ServerOnBuildingConstructed;
    public bool isBuildingFinished { get; set; }

    public GameObject GetBuildingSkeleton()
    {
        return buildingSkeleton;
    }

    public int getConstructionProgress()
    {
        return constructionProgress;
    }

    public GameObject GetBuildingPreview()
    {
        return buildingPreview;
    }

    //metode de tip getter pentru a acesa elementele de UI
    public Sprite GetIcon()
    {
        return icon;
    }
    public int GetId()
    {
        return id;
    }
    public int GetPrice()
    {
        return price;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawned?.Invoke(this);
    }

    //incrementarea progresului de construire
    public void ConstructBuilding()
    {
        constructionProgress += 10;
    }

    public void OnBuildingFinished()
    {   
        ServerOnBuildingConstructed?.Invoke(this);

        if (GetComponent<BuildingSound>())
        {
            GetComponent<BuildingSound>().PlayBuildingSound();
        }
    }


    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if(!isClientOnly) { return; }
        if(!hasAuthority) { return; }

        AuthorityOnBuildingSpawned?.Invoke(this);

    }

    public override void OnStopClient()
    {
        if(!hasAuthority) { return; }

        AuthorityOnBuildingDespawned?.Invoke(this);
    }
    #endregion
}
