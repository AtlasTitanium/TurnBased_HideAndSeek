using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class BeginCheckForSession : MonoBehaviour
{
    public string gameSelectScene;
    void Start(){
        StartCoroutine(LocalCheckForConnection());
    }

    IEnumerator ServerCheckForConnection(){
        UnityWebRequest www = UnityWebRequest.Get("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/GetUsername.php");
        yield return www.SendWebRequest();
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("no username");
            this.enabled = false;
        } else {
            SceneManager.LoadScene(gameSelectScene);
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            Debug.Log("School servers out");
        }
    }

    IEnumerator LocalCheckForConnection(){
        UnityWebRequest www = UnityWebRequest.Get("http://127.0.0.1/PHPstuff/GetUsername.php");
        yield return www.SendWebRequest();
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("no username");
            this.enabled = false;
        } else {
            SceneManager.LoadScene(gameSelectScene);
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            StartCoroutine(ServerCheckForConnection());
        }
    }
}
