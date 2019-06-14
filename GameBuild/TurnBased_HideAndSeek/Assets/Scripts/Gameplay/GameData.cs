using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class GameData : MonoBehaviour
{
    //Singleton
    private static GameData _instance;

    public static GameData Instance { get { return _instance; } }


    //Variables
    public string loginScreenName;
    public string menuSceneName;
    public string gameSceneName;
    public string endgameSceneName;
    public int gametimeInMinutes;
    
    [HideInInspector]
    public string ipString;
    [HideInInspector]
    public bool isClient;
    [HideInInspector]
    public bool isServer;
    [HideInInspector]
    public string playerUsername = "";
    [HideInInspector]
    public int finalScore;
    [HideInInspector]
    public bool isSeeker;
    private string DatabaseID;
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }
    void Start(){
        DatabaseID = CurrentDatabaseID.Instance.id;
    }

    #region Work Server
    public void StartServer(){
        isServer = true;
        isClient = true;
        SceneManager.LoadScene(gameSceneName);
    }

    public void StartClient(string ip){
        ipString = ip;
        isClient = true;
        SceneManager.LoadScene(gameSceneName);
    }

    public void StartGame(){
        //deactivate server when game starts
        StartCoroutine(LocalRemoveServer());
            
    }

    #region Normal Disconnect
    public void Disconnect(){
        Debug.Log("Disconnect");
        if(isServer){
            ServerManager.Instance.currentServer.DisconnectAllPlayers();
            isServer = false;
            isClient = false;
        } else if(isClient){
            ServerManager.Instance.currentClient.DisconnectThisPlayer();
            isClient = false;
        }
    }
    public void DisconnectClient(){
        Debug.Log("DisconnectClient");
        isServer = false;
        isClient = false;
        SceneManager.LoadScene(menuSceneName);
    }
    #endregion

    #region Endgame Disconnect
    public void EndGame(int _finalScore, bool _isSeeker){
        Debug.Log("GameData End Game");
        isSeeker = _isSeeker;
        finalScore = _finalScore;

        isServer = false;
        isClient = false;
        
        SceneManager.LoadScene(endgameSceneName);
    }
    #endregion
    

    IEnumerator RemoveServer(){
        WWWForm form = new WWWForm();
        form.AddField("ip", IPManager.GetLocalIPAddress());
        UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/RemoveServer.php", form);
        yield return www.SendWebRequest();
        if(www.downloadHandler.text == "0"){
            Debug.Log("Server removed");
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            Debug.Log("School servers out");
        }
    }

    IEnumerator LocalRemoveServer(){
        WWWForm form = new WWWForm();
        form.AddField("ip", IPManager.GetLocalIPAddress());
        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1/PHPstuff/RemoveServer.php", form);
        yield return www.SendWebRequest();
        if(www.downloadHandler.text == "0"){
            Debug.Log("Server removed");
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            StartCoroutine(RemoveServer());
        }
    }
    
    #endregion

    #region Userdata
    

    //Logout player
    public void LogOut(){
        StartCoroutine(LocalLogout());
    }

    IEnumerator ServerLogout(){
        UnityWebRequest www = UnityWebRequest.Get("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/Logout.php");
        yield return www.SendWebRequest();
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("Can't logout");
        } else {
            SceneManager.LoadScene(loginScreenName);
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            Debug.Log("School servers out");
        }
    }

    IEnumerator LocalLogout(){
        UnityWebRequest www = UnityWebRequest.Get("http://" + DatabaseID + "/PHPstuff/Logout.php");
        yield return www.SendWebRequest();
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("Can't logout");
        } else {
            SceneManager.LoadScene(loginScreenName);
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            StartCoroutine(ServerLogout());
        }
    }


    //Get Username
    public string GetUsername(){
        StartCoroutine(GetUsernameLocally());
        return playerUsername;
    }

    IEnumerator GetUsernameServer(){
        UnityWebRequest www = UnityWebRequest.Get("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/GetUsername.php");
        yield return www.SendWebRequest();
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("Can't get username");
        } else {
            playerUsername = www.downloadHandler.text;
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            Debug.Log("School servers out");
        }
    }

    IEnumerator GetUsernameLocally(){
        UnityWebRequest www = UnityWebRequest.Get("http://" + DatabaseID + "/PHPstuff/GetUsername.php");
        yield return www.SendWebRequest();
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("Can't get username");
        } else {
            playerUsername = www.downloadHandler.text;
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            StartCoroutine(RemoveServer());
        }
    }


    #endregion
}


