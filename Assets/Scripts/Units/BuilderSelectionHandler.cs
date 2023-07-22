using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BuilderSelectionHandler : MonoBehaviour
{
    private Camera mainCamera;

    [SerializeField] private LayerMask layerMask = new LayerMask();
    
    public UnitBuilder selectedBuilder;


    private void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());


            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

            if (!hit.collider.TryGetComponent<UnitBuilder>(out UnitBuilder builder)) { return; }

            if (!builder.hasAuthority) { return; }

            selectedBuilder = builder;

            selectedBuilder.gameObject.GetComponent<Unit>().Select();

            Debug.Log(selectedBuilder);
        }
        else if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            selectedBuilder.gameObject.GetComponent<Unit>().Deselect();
        }

    }
}
