using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuilderCommandGiver : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask = new LayerMask();
    [SerializeField] private BuilderSelectionHandler builderSelectionHandler;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) {  return; }
        //we check: if we are targeting a friendly unit, we move towards it;
        //if it is an enemy, get target it, and if it's just the ground, we move towards it and claer our current target!

        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            if (target.hasAuthority)
            {
                builderSelectionHandler.selectedBuilder.GetTargeter().CmdSetTarget(target.gameObject);
                return;
            }
        }
    }

}
