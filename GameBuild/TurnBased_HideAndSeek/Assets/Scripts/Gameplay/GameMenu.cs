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
    public Button logOut;
    public InputField serverName;
    

    private string DatabaseID;
    void Start(){
        DatabaseID = CurrentDatabaseID.Instance.id;
        hostGame.onClick.AddListener(StartHost);
        joinGame.onClick.AddListener(StartJoin);
        logOut.onClick.AddListener(LogOutUser);
    }

    void StartHost(){
        if(serverName.text != ""){
            StartCoroutine(CreateLocalHost());
        }
        
    }

    void StartJoin(){
        if(serverName.text != ""){
            StartCoroutine(CreateLocalClient());
        }
    }

    void LogOutUser(){
        GameData.Instance.LogOut();
    }

    #region Server connect
    //Create server on school server
    IEnumerator CreateHost(){
        WWWForm form = new WWWForm();
        form.AddField("name", serverName.text);
        form.AddField("ip", IPManager.GetLocalIPAddress());
        UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/CreateServer.php", form);
        yield return www.SendWebRequest();
        Debug.Log(www.downloadHandler.text);
        if(www.downloadHandler.text == "0"){
            GameData.Instance.StartServer();
        }
        
        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            Debug.Log("School servers out");
            
        }
    }

    //Create server locally with xampp
    IEnumerator CreateLocalHost(){
        WWWForm form = new WWWForm();
        form.AddField("name", serverName.text);
        form.AddField("ip", IPManager.GetLocalIPAddress());
        UnityWebRequest www = UnityWebRequest.Post("http://" + DatabaseID + "/PHPstuff/CreateServer.php", form);
        yield return www.SendWebRequest();
        if(www.downloadHandler.text == "0"){
            GameData.Instance.StartServer();
        }
        
        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            StartCoroutine(CreateHost());
        }
    }

    #endregion

    #region Client connect
    //join client on school server
    IEnumerator CreateClient(){
        WWWForm form = new WWWForm();
        form.AddField("name", serverName.text);
        UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/JoinServer.php", form);
        yield return www.SendWebRequest();
        string[] ipAdressNumbers = www.downloadHandler.text.Split('.');
        if(ipAdressNumbers.Length == 4){
            GameData.Instance.StartClient(www.downloadHandler.text);
        } else {
            Debug.LogError("Error: ip is not correct - your given ip adress either contains not enough numbers, or too many");
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            Debug.Log("School servers out");
            
        }
    }

    //join client locally with xampp
    IEnumerator CreateLocalClient(){
        WWWForm form = new WWWForm();
        form.AddField("name", serverName.text);
        UnityWebRequest www = UnityWebRequest.Post("http://" + DatabaseID + "/PHPstuff/JoinServer.php", form);
        yield return www.SendWebRequest();
        string[] ipAdressNumbers = www.downloadHandler.text.Split('.');
        if(ipAdressNumbers.Length == 4){
            GameData.Instance.StartClient(www.downloadHandler.text);
        } else {
            Debug.LogError("Error: ip is not correct - your given ip adress either contains not enough numbers, or too many");
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            StartCoroutine(CreateClient());
        }
    }

    #endregion
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
