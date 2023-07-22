using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{   //obiect universal atasat scenei pentru controlul unitatilor!
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if(!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }
        //verificam daca tinta este un obiect de-al nostru; in caz afirmativ ne deplasam in acea directie
        //daca este un inamic, il referentiem ca tinta curenta, iar daca este element de teren, ne deplasam spre el!
        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {         
            if(target.hasAuthority)
            {
                TryMove(hit.point);

                foreach (Unit unit in unitSelectionHandler.SelectedUnits)
                {
                    if(unit.gameObject.GetComponent<UnitBuilder>())
                    {
                        TryTarget(target);
                    }
                }

                return;
            }
            TryTarget(target);
            return;
        }
        TryMove(hit.point);
    }

    private void TryTarget(Targetable target)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter().CmdSetTarget(target.gameObject);
        }
    }

    private void TryMove(Vector3 point)
    {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().CmdMove(point);
        }
    }
}
