using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.EventSystems;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{   
    //referinta la locatia ortografica a hartii
    [SerializeField] private RectTransform minimapTransform = null;
    [SerializeField] private float mapScale = 20f;
    [SerializeField] private float offset = -6;

    //referinta la camera jucatorului
    private Transform playerCameraTransform;

    private void Update()
    {
        if(playerCameraTransform != null) { return; }

        if(NetworkClient.connection.identity == null) { return; }

        playerCameraTransform = 
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();
    }

    private void MoveCamera()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle
        (minimapTransform, mousePos, null, out Vector2 localPoint)) { return; }
        //vom imparti transformata minimapului,
        //din moment ce valorile preluate sunt la dimensiune absoluta raportata
        //la dimensiunea curenta a ecranului
        //si avem nevoie de valori relative pentru portarea pe ecrane
        //cu rezolutii multiple
        Vector2 lerp = new Vector2(
            (localPoint.x - minimapTransform.rect.x) / minimapTransform.rect.width,
            (localPoint.y - minimapTransform.rect.y) / minimapTransform.rect.height);

        Vector3 newCameraPos = 
        new Vector3(Mathf.Lerp(-mapScale, mapScale, lerp.x), 
        playerCameraTransform.position.y, Mathf.Lerp(-mapScale, mapScale, lerp.y));

        playerCameraTransform.position = newCameraPos + new Vector3(0f, 0f, offset);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }
}
