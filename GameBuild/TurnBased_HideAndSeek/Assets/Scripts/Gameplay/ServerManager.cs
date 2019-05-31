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

        if(gameData.isServer){
            gameData.ipString = IPManager.GetLocalIPAddress();
            Debug.Log(gameData.ipString);
            
            Instantiate(server,transform.position,Quaternion.identity).GetComponent<Server>().ipString = gameData.ipString;
            ipText.text = "IP: " + gameData.ipString;
        }
        if(gameData.isClient){
            Instantiate(client,transform.position,Quaternion.identity).GetComponent<Client>().ipAdress = gameData.ipString;
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
