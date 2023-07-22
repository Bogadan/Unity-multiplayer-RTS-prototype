using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    //logica se realizeaza pur pe partea de client;
    //nu este nevoie de o componenta de networking

    [SerializeField] private Building building = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text priceText = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();
    //referinta la camera principala va fi utilizata pentru raycasting
    private Camera mainCamera;
    private RTSPlayer player;
    //un obiect 3D ce va arata previzualizarea modelului 3D al cladirii
    private GameObject buildingPreviewInstance;
    private Renderer buildingRendererInstance;

    private BoxCollider buildingCollider;

    private void Start()
    {
        mainCamera = Camera.main;

        iconImage.sprite = building.GetIcon();
        priceText.text = building.GetPrice().ToString();

        StartCoroutine(instantiatePlayerDelay());

        buildingCollider = building.GetComponent<BoxCollider>();
    }
    
    private IEnumerator instantiatePlayerDelay()
    {
        if (player == null)
        {
            yield return new WaitForSeconds(2f);
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            Debug.Log("The server instantiated the following player: " + player);
        }
    }

    private void Update()
    {
        if(buildingPreviewInstance == null) { return; }

        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }

        if(player.GetResources() < building.GetPrice()) { return; }

        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = 
        buildingPreviewInstance.GetComponentInChildren<Renderer>();
        //dezactivam initial previzualizarea, pana cand raycast-ul devine valid
        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(buildingPreviewInstance == null) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            //validarea si asocierea cladirii cu jucatorul pe partea de server
            //printr-un Command
            //precizam serverului ca jucatorul doreste sa realizeaza instantierea

            player.CmdTryPlaceBuilding(building.GetId(), hit.point);

        }
        //dealocam previzualizarea cladirii dupa ce aceasta a fost amplasata
        Destroy(buildingPreviewInstance);
    }

    private void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        //verificam daca raycast-ul se izbeste de un punct valid,
        //pentru a putea randa in continuare previzualizarea cladirii sau nu...
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            return;
        }

        buildingPreviewInstance.transform.position = 
        new Vector3(hit.point.x,buildingPreviewInstance.transform.position.y, hit.point.z);

        //activam previzualizarea daca pozitia de construire este valida
        if (!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }
        //Ca la schimbarea culorilor echipelor, suprascriem canalul de culoare al
        //texturii cladirii cu verde daca locatia este valida; altfel rosu
        Color color = 
        player.CanPlaceBuilding(buildingCollider, hit.point) ? Color.green : Color.red;

        buildingRendererInstance.material.SetColor("_Color", color);
    }
}
