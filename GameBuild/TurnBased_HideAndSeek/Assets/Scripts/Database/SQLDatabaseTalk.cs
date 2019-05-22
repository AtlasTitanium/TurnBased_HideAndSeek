using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SQLDatabaseTalk : MonoBehaviour
{ 
    public Text name;
    public Text pass;
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
        loginName = name.text;
        loginPassword = pass.text;

        WWWForm form = new WWWForm();
        form.AddField("LoginName", loginName);
        form.AddField("LoginPassword", loginPassword);

        if(register){
            using(WWW www = new WWW("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/Register.php", form)){
                yield return www;

                if(www.text.Contains("0")){
                    Debug.Log("Connected Succesfully");
                } else{

                    Debug.LogError("Failed to connect, Error #" + www.text);
                }
            }
        } else {
            using(WWW www = new WWW("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/Login.php", form)){
                yield return www;

                if(www.text.Contains("0")){
                    Debug.Log("Connected Succesfully");
                } else{

                    Debug.LogError("Failed to connect, Error #" + www.text);
                }
            }
        }
    }
}
