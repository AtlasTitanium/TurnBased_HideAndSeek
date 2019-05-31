using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameData : MonoBehaviour
{
    //Singleton
    private static GameData _instance;

    public static GameData Instance { get { return _instance; } }


    //Variables
    public string menuSceneName;
    public string gameSceneName;
    
    [HideInInspector]
    public string ipString;
    [HideInInspector]
    public bool isClient;
    [HideInInspector]
    public bool isServer;

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

    public void Disconnect(){
        isClient = false;
        isServer = false;
        SceneManager.LoadScene(menuSceneName);
    }
}
