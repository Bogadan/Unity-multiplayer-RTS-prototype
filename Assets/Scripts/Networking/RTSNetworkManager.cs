using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitSpawnerPrefab = null;
    
    [SerializeField] GameOverHandler gameOverHandlerPrefab = null;
    //suprascrierea event-urile de conectare in retea pentru un jucator
    //in scopul realizarii unui meniu de Lobby
    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;
    //pentru configurarea jucatorilor din Lobby (camera de asteptare a meciului)...
    private bool isGameInProgress = false;
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    #region Server
    //pentru conectarea la Lobby
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (!isGameInProgress) { return; }
            //base.OnServerConnect((NetworkConnectionToClient)conn);
            conn.Disconnect();
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        

        Players.Remove(player);

        


    // Dealocam obiectul jucator, in cazul in care acesta paraseste lobby-ul dar doreste sa se reconecteze!
   // if (player != null)
    //        NetworkServer.Destroy(player.gameObject);

        base.OnServerDisconnect(conn);
    }
    public override void OnStopServer()
    {
        Players.Clear();

       // isGameInProgress = false; //MOD AICI
    }

    public void StartGame()
    {
        //if (Players.Count < 1) return;

      //  isGameInProgress = true; //MOD AICI

        ServerChangeScene("Scene_Map");
    }

    

    //Lista de culori disponibila pentru un jucator
    public static List<Color> availableTeamColors = 
    new List<Color> { Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta };
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        //base.OnServerAddPlayer(conn);
        // setarea culorii jucatorului in logica serverului
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>(); 
        Players.Add(player);
        //generarea numelor de jucatori pentru afisare in lobby
        player.SetDisplayName($"Jucatorul {Players.Count}");

        Color pickedColor = 
            availableTeamColors[UnityEngine.Random.Range(0, availableTeamColors.Count)];
        player.SetTeamColor(pickedColor);
        availableTeamColors.Remove(pickedColor);
        //instantierea bazei principale
        GameObject unitSpawner = 
Instantiate(unitSpawnerPrefab, conn.identity.transform.position, conn.identity.transform.rotation);
        NetworkServer.Spawn(unitSpawner, conn);
        player.SetPartyOwner(Players.Count == 1); //lobby
    }
    //deoarece vorbim despre un obiect static care isi mentine
    //durata de viata intre scenele incarcate,
    //cream conditia de victorie doar in harta principala, nu si in meniul jocului!
    public override void OnServerSceneChanged(string sceneName)
    {

        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {

            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach(RTSPlayer player in Players)
            {
                if (player.connectionToClient != null && player.connectionToClient.isReady)
                {
            GameObject baseInstance = 
            Instantiate(unitSpawnerPrefab, GetStartPosition().position, Quaternion.identity);
            NetworkServer.Spawn(baseInstance, player.connectionToClient);
                }
            }
        }
    }

    #endregion

    #region Client

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }

    #endregion

}
