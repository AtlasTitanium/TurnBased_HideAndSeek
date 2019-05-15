using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SQLDatabaseTalk : MonoBehaviour
{   
    public Text sqlTextField;
    public string testPuppetID;

    private void Start(){
        StartCoroutine(Connect());
    }
    IEnumerator Connect(){
        WWWForm form = new WWWForm();
        form.AddField("CharacterID", testPuppetID);

        using(WWW www = new WWW("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/Connection.php", form)){
            yield return www;

            if(www.text.Contains("0")){
                Debug.Log("Connected Succesfully");
                sqlTextField.text = www.text;
            } else{
                Debug.LogError("Failed to connect, Error #" + www.text);
            }
        }
    }
}
