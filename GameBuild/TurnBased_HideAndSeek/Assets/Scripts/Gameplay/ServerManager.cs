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

    //UI control
    public Text ipText;

    //Others
    public GameObject[] spawnLocations;
    private GameData gameData;

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
            Debug.Log(gameData.ipString);
            
            Server s = Instantiate(server,transform.position,Quaternion.identity).GetComponent<Server>();
            s.ipString = gameData.ipString;
            s.spawnLocations = spawnLocations;
            ipText.text = "IP: " + gameData.ipString;
        }

        //Instantiate Client
        if(gameData.isClient){
            GameObject c = Instantiate(client,transform.position,Quaternion.identity);
            c.GetComponent<Client>().ipAdress = gameData.ipString;

            //Set player as seeker or hider;
            if(gameData.isServer){
                c.GetComponent<Client>().seeker = true;
                c.GetComponent<Player>().ableToMove = 0;
            } else {
                c.GetComponent<Client>().seeker = false;
                c.GetComponent<Player>().ableToMove = 20;
            }
        }
        
    }
}

public static class IPManager
{
    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
