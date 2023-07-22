using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class JoinLobbyMenu : MonoBehaviour
{   //pentru conexiunea jucatorului cu lobby-ul prin meniul principal de joc implementat
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMP_InputField addressInput = null;
    [SerializeField] private Button joinButton = null;

    [SerializeField] private Button startGameButton = null;

    private void OnEnable()
    {   //facem subscribe la evenimentele create pentru conectarea clientilor
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected -= HandleClientDisconnected;
    }

    public void Join()
    {
        string address = addressInput.text;

        NetworkManager.singleton.networkAddress = address;
        Debug.Log(address);
        NetworkManager.singleton.StartClient();

        joinButton.interactable = false;

        startGameButton.gameObject.SetActive(false);
    }

    public void HandleClientConnected()
    {
        joinButton.interactable = true;

        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }
    public void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
