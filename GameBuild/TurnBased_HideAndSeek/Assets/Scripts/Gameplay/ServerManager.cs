using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerManager : MonoBehaviour
{
    //Singleton
    private static ServerManager _instance;

    public static ServerManager Instance { get { return _instance; } }

    //Client and Server Prefabs
    public GameObject client;
    public GameObject server;

    //Others
    public GameObject[] spawnLocations;
    private GameData gameData;
    [HideInInspector]
    public Server currentServer;
    [HideInInspector]
    public Client currentClient;

    void Awake()
    {
        //activate singleton
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    void Start(){
        gameData = GameData.Instance;
        if(gameData == null){
            Debug.LogWarning("Server Manager cannot find GameData script. \nStart game from MainMenu Scene.");
            return;
        }

      

        //Instantiate server
        if(gameData.isServer){
            gameData.ipString = IPManager.GetLocalIPAddress();
            
            currentServer = Instantiate(server,transform.position,Quaternion.identity).GetComponent<Server>();
            currentServer.ipString = gameData.ipString;
            currentServer.spawnLocations = spawnLocations;
        }

        //Instantiate Client
        if(gameData.isClient){
            currentClient = Instantiate(client,transform.position,Quaternion.identity).GetComponent<Client>();
            currentClient.ipAdress = gameData.ipString;

            //Set player as seeker or hider;
            if(gameData.isServer){
                currentClient.seeker = true;
            } else {
                currentClient.seeker = false;
            }

            currentClient.GetComponent<Player>().ableToMove = 0;
        }
        
    }
}
