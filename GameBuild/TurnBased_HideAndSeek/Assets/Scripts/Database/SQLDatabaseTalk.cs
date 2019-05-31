using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SQLDatabaseTalk : MonoBehaviour
{ 
    public InputField username;
    public InputField pass;
    public Button button;
    private string loginName;
    private string loginPassword;
    public bool register = false;

    private void Start(){
        button.onClick.AddListener(Connector);
    }

    void Connector(){
        StartCoroutine(Connect());
    }
    IEnumerator Connect(){
        loginName = username.text;
        loginPassword = pass.text;

        WWWForm form = new WWWForm();
        form.AddField("LoginName", loginName);
        form.AddField("LoginPassword", loginPassword);

        if(register){
            using(UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/Register.php", form)){
                yield return www.SendWebRequest();

                if(www.downloadHandler.text.Contains("0")){
                    Debug.Log("Connected Succesfully");
                } else{
                    Debug.LogError("Failed to connect, Error #" + www.downloadHandler.text);
                }
            }
        } else {
            using(UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/Login.php", form)){
                yield return www.SendWebRequest();

                if(www.downloadHandler.text.Contains("0")){
                    Debug.Log("Connected Succesfully");
                } else{
                    Debug.LogError("Failed to connect, Error #" + www.downloadHandler.text);
                }
            }
        }
    }
}
