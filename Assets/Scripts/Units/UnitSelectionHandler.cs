using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    //We will build upon this in the future...This is the main game logic for multiple unit selection locally!
    [SerializeField] private RectTransform unitSelectionArea = null;

    [SerializeField] private LayerMask layerMask = new LayerMask();

    //selecting multiple units, tut 9, for storing the drag box
    private Vector2 startPosition;

    private static RTSPlayer player; //i made it static
    private Camera mainCamera;

    public List<Unit> SelectedUnits = new List<Unit>();


    private void Start()
    {
        mainCamera = Camera.main;
            StartCoroutine(instantiatePlayerDelay());
            Debug.Log("aaaa");

        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
    }

    private IEnumerator instantiatePlayerDelay()
    {
        if(player == null)
        {
            yield return new WaitForSeconds(2f);
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            Debug.Log("The server instantiated the following player: " + player);
        }
    }

    private void Update()
    {
        //temporary fix for the player instantiating before the network starts!
        /*if(player is null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }*/

        //for deselecting units
        
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }//for selecting
        else if(Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if(Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }

    }

    private void StartSelectionArea()
    {
        if(!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }

            SelectedUnits.Clear();
        }    

        unitSelectionArea.gameObject.SetActive(true);

        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    //the main method for checking validation on selecting a unit, adding it to the list of active units and activating the circle selection for each!
    private void ClearSelectionArea()
    {
        //tutorial 9
        //logic for only one unit
        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());


            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) { return; }

            if (!unit.hasAuthority) { return; }

            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }
            return;
        }

        //for selecting multiple units inside the drag box....
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (Unit unit in player.GetMyUnits())
        {
            //tut10 - for being able to select additional units besides those selected with the initial drag box...
            if(SelectedUnits.Contains(unit)) { return; }

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            if(screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
        unitSelectionArea.sizeDelta = new Vector2(0,0);
    }
    //tutorail 16 - when we destroy a unit, it still remains in the selectible units list;
    //this can generate errors as we try to select something that does not exist anymore, so we remove it
    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }
}
