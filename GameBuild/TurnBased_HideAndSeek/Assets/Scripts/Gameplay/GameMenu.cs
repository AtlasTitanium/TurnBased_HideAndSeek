using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    [Header("UI elements")]
    public Button hostGame;
    public Button joinGame;
    public InputField ipAdress;

    [Header("game Elements")]
    public GameData gameData;

    void Start(){
        hostGame.onClick.AddListener(StartHost);
        joinGame.onClick.AddListener(StartJoin);
    }

    void StartHost(){
        gameData.StartServer();
    }

    void StartJoin(){
        if(ipAdress.text != ""){
            string[] ipAdressNumbers = ipAdress.text.Split('.');
            if(ipAdressNumbers.Length == 4){
                gameData.StartClient(ipAdress.text);
            } else {
                Debug.LogError("Error: ip is not correct - your given ip adress either contains not enough numbers, or too many");
            }
        }
    }
}
