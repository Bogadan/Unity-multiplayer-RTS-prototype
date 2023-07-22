using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText = null;

    private RTSPlayer player;

    private void Start()
    {   //temporary....
        StartCoroutine(instantiatePlayerDelay());
    }

    private IEnumerator instantiatePlayerDelay()
    {   //temporary...
        if (player == null)
        {
            yield return new WaitForSeconds(2f);
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            Debug.Log("The server instantiated the following player: " + player);
        }
    }

    private void Update()
    {
        if (player != null)
        {
            ClientHandleResourcesUpdated(player.GetResources());

            player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
        }
    }

    private void OnDestroy()
    {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int resources)
    {
        resourcesText.text = $"{resources}";
    }
}
