using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SQLDatabaseTalk : MonoBehaviour
{ 
    public GameObject main;
    public InputField username;
    public InputField pass;
    public Button button;
    private string loginName;
    private string loginPassword;
    public bool register = false;
    public string gameSelectSceneName;

    private void Start(){
        button.onClick.AddListener(Connector);
    }

    void Connector(){
        StartCoroutine(LocalConnect());
    }
    IEnumerator ServerConnect(){
        loginName = username.text;
        loginPassword = pass.text;

        WWWForm form = new WWWForm();
        form.AddField("LoginName", loginName);
        form.AddField("LoginPassword", loginPassword);

        if(register){
            using(UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/Register.php", form)){
                yield return www.SendWebRequest();

                if(www.downloadHandler.text.Contains("ERROR")){
                    StartCoroutine(ServerConnect());
                } else {
                    main.SetActive(true);
                    this.gameObject.SetActive(false);
                }
            }
        } else {
            using(UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/Login.php", form)){
                yield return www.SendWebRequest();

                if(www.downloadHandler.text.Contains("ERROR")){
                    StartCoroutine(ServerConnect());
                } else {
                    SceneManager.LoadScene(gameSelectSceneName);
                }
            }
        }
    }

    IEnumerator LocalConnect(){
        loginName = username.text;
        loginPassword = pass.text;

        WWWForm form = new WWWForm();
        form.AddField("LoginName", loginName);
        form.AddField("LoginPassword", loginPassword);

        if(register){
            using(UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1/PHPstuff/Register.php", form)){
                yield return www.SendWebRequest();

                if(www.downloadHandler.text.Contains("ERROR")){
                    StartCoroutine(ServerConnect());
                } else {
                    main.SetActive(true);
                    this.gameObject.SetActive(false);
                }
            }
        } else {
            using(UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1/PHPstuff/Login.php", form)){
                yield return www.SendWebRequest();

                if(www.downloadHandler.text.Contains("ERROR")){
                    StartCoroutine(ServerConnect());
                } else {
                    SceneManager.LoadScene(gameSelectSceneName);
                }
            }
        }
    }
}
