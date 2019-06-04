using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameMenu : MonoBehaviour
{
    [Header("UI elements")]
    public Button hostGame;
    public Button joinGame;
    public InputField serverName;

    [Header("Game Elements")]
    public GameData gameData;

    void Start(){
        hostGame.onClick.AddListener(StartHost);
        joinGame.onClick.AddListener(StartJoin);
    }

    void StartHost(){
        if(serverName.text != ""){
            StartCoroutine(CreateHost());
        }
        
    }

    void StartJoin(){
        if(serverName.text != ""){
            StartCoroutine(CreateClient());
        }
    }

    IEnumerator CreateHost(){
        WWWForm form = new WWWForm();
        form.AddField("name", serverName.text);
        form.AddField("ip", IPManager.GetLocalIPAddress());
        UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/CreateServer.php", form);
        yield return www.SendWebRequest();
        Debug.Log(www.downloadHandler.text);
        if(www.downloadHandler.text == "0"){
            gameData.StartServer();
        }
    }

    IEnumerator CreateClient(){
        WWWForm form = new WWWForm();
        form.AddField("name", serverName.text);
        UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/JoinServer.php", form);
        yield return www.SendWebRequest();
        string[] ipAdressNumbers = www.downloadHandler.text.Split('.');
        if(ipAdressNumbers.Length == 4){
            Debug.Log(www.downloadHandler.text);
            gameData.StartClient(www.downloadHandler.text);
        } else {
            Debug.LogError("Error: ip is not correct - your given ip adress either contains not enough numbers, or too many");
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
