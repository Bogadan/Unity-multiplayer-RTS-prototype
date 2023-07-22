using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
public class GameOverDisplay : MonoBehaviour
{
    //clasa interfetei de UI la care va avea acces toti jucatorii pentru a afisa mesajul de Game Over

    [SerializeField] private GameObject gameOverDisplayParent = null;
    [SerializeField] private TMP_Text winnerNameText = null;
    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }
    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    public void LeaveGame()
    {   //daca suntem client, iesim din joc, iar daca suntem si gazda jocului, oprim serverul.
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        winnerNameText.text = $"{winner} a castigat!";
        gameOverDisplayParent.SetActive(true);
    }
}
