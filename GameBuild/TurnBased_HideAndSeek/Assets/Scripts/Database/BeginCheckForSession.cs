using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class BeginCheckForSession : MonoBehaviour
{
    public string gameSelectScene;
    private string DatabaseID;
    void Start(){
        DatabaseID = CurrentDatabaseID.Instance.id;
        StartCoroutine(LocalCheckForConnection());
    }

    IEnumerator ServerCheckForConnection(){
        UnityWebRequest www = UnityWebRequest.Get("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/GetUsername.php");
        yield return www.SendWebRequest();
        if(www.isNetworkError){
            Debug.Log("School servers out");
        } else 
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("no username");
            this.enabled = false;
        } else {
            SceneManager.LoadScene(gameSelectScene);
        }
    }

    IEnumerator LocalCheckForConnection(){
        UnityWebRequest www = UnityWebRequest.Get("http://" + DatabaseID + "/PHPstuff/GetUsername.php");
        yield return www.SendWebRequest();
        if(www.isNetworkError){
            StartCoroutine(ServerCheckForConnection());
        } else 
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("no username");
            this.enabled = false;
        } else {
            SceneManager.LoadScene(gameSelectScene);
        }
    }
}
