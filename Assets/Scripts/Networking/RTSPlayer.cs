using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class RTSPlayer : NetworkBehaviour
{   
    //we will use this script to maintain reference server side regarding selected units, units list etc.

    //unit list reference for the current client
    private List<Unit> myUnits = new List<Unit>();
    //building list reference for the current client
    private List<Building> myBuildings = new List<Building>();
    //for returning the units that are linked to the client

    //spawning buildings; we create a list of buildings that links the ID with the building itself
    [SerializeField] private Building[] buildings = new Building[0];

    //Resource generation; We will store the local resoureces for producing new units and buildings!
    // we want the server to manage the resources, so it is going to be a server variable that is synched back to the client!
    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;

    //sincronizarea numelui jucatorului cu meniul de afisare din Lobby
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;

    public static event Action ClientOnInfoUpdated;

    //Building Limits
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private float buildingRangeLimit = 30f;

    public event Action<int> ClientOnResourcesUpdated;

    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    //Team colors
    private Color teamColor = new Color();

    //implementing a minimap movement
    [SerializeField] private Transform cameraTransform = null;

    public string GetDisplayName()
    {
        return displayName;
    }

    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }

    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    private void ClientHandleResourcesUpdated(int oldResources, int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }

    public Color GetTeamColor()
    {
        return teamColor;
    }

    public int GetResources()
    {
        return resources;
    }

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }    

public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
{   if (Physics.CheckBox(point + buildingCollider.center, buildingCollider.size / 2, 
        Quaternion.identity, buildingBlockLayer))
    {
        return false; //verificam daca ne intersectam cu alte obstacole din scena
    }
    //nu putem plasa o noua cladire decat in vecinatatea celor existente
    //mecanism sa nu poti antrena soldati chiar in baza inamica!
    foreach (Building building in myBuildings)
    {
        if ((point - building.transform.position).sqrMagnitude 
           <= buildingRangeLimit * buildingRangeLimit)
        {
            return true;
        }
    }
    return false;
}


    //by using + sign, we are subcribing a gameobject to an event, specifically the Unit script which calls the RTSPlayer script when spawned
    #region Server
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

        //for constructing the buildings...
        Building.ServerOnBuildingConstructed += ServerHandleBuildingConstructed;
    }
    [ClientRpc]
    private void ServerHandleBuildingConstructed(Building building)
    {
        building.GetBuildingSkeleton().SetActive(false);
        building.GetBuildingPreview().SetActive(true);
        building.GetComponent<Health>().enabled = true;

        //we enable the all the UI elements when the building is finished
        foreach (var uiElement in building.GetComponentsInChildren<Canvas>())
        {
            uiElement.enabled = true;
        }

        //we enable unit spawning after the building is fully finished if the building is a unit spawner; else we enable resource generator!
        if (building.tag.Equals("Resource"))
        {
            building.GetComponent<ResourceGenerator>().enabled = true;
            
        }        
        else
        {
            building.GetComponent<UnitSpawner>().enabled = true;
        }
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;

        Building.ServerOnBuildingConstructed -= ServerHandleBuildingConstructed;
    }

    [Command]
    public void CmdStartGame()
    {
        if(!isPartyOwner) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }

    //instantierea constructiilor
    [Command]
    public void CmdTryPlaceBuilding(int buildingID, Vector3 spawnPosition)
    {
        Building buildingToPlace = null;

        foreach(Building building in buildings)
        {
            if(building.GetId() == buildingID)
            {
                buildingToPlace = building;
                break;
            }
        }
        //daca raycasting-ul nu a reusit sau nu sa gasit id-ul cladirii
        if(buildingToPlace == null) { return; }

        //verificam server-side daca clientul are destule resurse pentru constructie

        if(resources < buildingToPlace.GetPrice()) { return; }

        //componenta de coliziune a cladirii

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

    

        if(!CanPlaceBuilding(buildingCollider, spawnPosition)) { return; }

        //cladirea va fi un obiect in retea, deci va trebui sa il asociem cu jucatorul;
        //mai intai il vom instantia local,
        //pe masina clientului, dupa pe server si realizam
        //asocierea intre client si obiect

        //inaltimea constructiei nu se modifica
        spawnPosition.y = buildingToPlace.transform.position.y;
        //instantiere locala
        GameObject buildingInstance =  
Instantiate(buildingToPlace.gameObject, spawnPosition, buildingToPlace.transform.rotation);
        //instantierea pe server a cladirii
        NetworkServer.Spawn(buildingInstance, connectionToClient);
        
        SetResources(resources - buildingToPlace.GetPrice());
    }

    //for handling server-side unit spawning
    private void ServerHandleUnitSpawned(Unit unit)
    {
        //check if the client who connects to the unit is the same as the client who owns the unit!
        if(unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        //check if the client who connects to the unit is the same as the client who owns the unit!
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Remove(unit);
    }
    //for handling server-side building spawning and reference
    private void ServerHandleBuildingSpawned(Building building)
    {
        //check if the client who connects to the unit is the same as the client who owns the unit!
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Add(building);

        //we initially disable the UnitSpawner component until the building is fully constructed...except for the base which is already existing
        if(building.GetId() != 0)
        {
            if(building.GetComponent<UnitSpawner>())
            {
                building.GetComponent<UnitSpawner>().enabled = false;
            }
            building.GetComponent<Health>().enabled = false;
            //we disable the all the UI elements until the building is finished
            foreach(var uiElement in building.GetComponentsInChildren<Canvas>())
            {
                uiElement.enabled = false;
            }
        }
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        //check if the client who connects to the unit is the same as the client who owns the unit!
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Remove(building);
    }

    [Server]
    public void SetDisplayName(string _displayName)
    {
        displayName = _displayName;
    }

    [Server]
    public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    //setting the team color inside the server
    [Server]
    public void SetTeamColor(Color newTeamColor)
    {
        teamColor = newTeamColor;
    }

    //setting resources for the player
    [Server]
    public void SetResources(int newResources)
    {
        resources = newResources;
    }

    
    //swapping the building prefab from in-construction to the finished one
    [Server]
    public void OnBuildingFinished(GameObject buildingSkeleton)
    {
        buildingSkeleton.SetActive(false);
        
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        //check if this machine is acting like a server
        if(NetworkServer.active) { return; }

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStartClient()
    {
        if (NetworkServer.active) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
    }

    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();

        if (!isClientOnly) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

        if(!hasAuthority) { return; }

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if(!hasAuthority) { return; }

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    #endregion
}
