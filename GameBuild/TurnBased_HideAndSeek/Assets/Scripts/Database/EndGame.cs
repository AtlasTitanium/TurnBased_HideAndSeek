using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    public Text endState;
    public Text score;
    public Button end;
    public string mainMenuSceneName;

    void Start(){
        end.onClick.AddListener(ReturntoMenu);

        if(GameData.Instance.isSeeker){
            if(GameData.Instance.finalScore <= 0){
                Debug.Log("player lost");
                endState.text = "Lost";
            } else {
                Debug.Log("player won");
                endState.text = "Won";
            }

            score.text = "Hiders found: " + GameData.Instance.finalScore;

            StartCoroutine(LocalSeekerScore());
        } else {
            if(GameData.Instance.finalScore >= 300){
                Debug.Log("player Won");
                endState.text = "Won";
            } else {
                Debug.Log("player lost");
                endState.text = "Lost";
            }

            score.text = "Time survived: " + Conversions.toTime(GameData.Instance.finalScore);

            StartCoroutine(LocalHiderScore());
        }
    }

    public void ReturntoMenu(){
        SceneManager.LoadScene(mainMenuSceneName);
    }

    //Set seeker score
    IEnumerator ServerSeekerScore(){
        WWWForm form = new WWWForm();
        form.AddField("score", GameData.Instance.finalScore);
        form.AddField("isSeeker", 1);

        UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/SetScore.php", form);
        yield return www.SendWebRequest();
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("Can't set score");
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            Debug.Log("School servers out");
        }
    }

    IEnumerator LocalSeekerScore(){
        WWWForm form = new WWWForm();
        form.AddField("score", GameData.Instance.finalScore);
        form.AddField("isSeeker", 1);

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1/PHPstuff/SetScore.php", form);
        yield return www.SendWebRequest();
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("Can't set score");
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            StartCoroutine(ServerSeekerScore());
        }
    }

    //Set hider score
    IEnumerator ServerHiderScore(){
        WWWForm form = new WWWForm();
        form.AddField("score", GameData.Instance.finalScore);
        form.AddField("isSeeker", 0);

        UnityWebRequest www = UnityWebRequest.Post("https://studenthome.hku.nl/~pepijn.kok/PHPstuff/SetScore.php",form);
        yield return www.SendWebRequest();
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("Can't set score");;
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            Debug.Log("School servers out");
        }
    }

    IEnumerator LocalHiderScore(){
        WWWForm form = new WWWForm();
        form.AddField("score", GameData.Instance.finalScore);
        form.AddField("isSeeker", 0);

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1/PHPstuff/SetScore.php",form);
        yield return www.SendWebRequest();
        if(www.downloadHandler.text.Contains("ERROR")){
            Debug.Log("Can't set score");
        }

        //check for connect error
        if(www.downloadHandler.text.Contains("Failed")){
            StartCoroutine(ServerHiderScore());
        }
    }
}
