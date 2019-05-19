using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private int i = 0;
    private Client client;

    void Start(){
        client = GetComponent<Client>();
    }
    void Update(){
        if(Input.GetKey(KeyCode.W)){
            client.SendNumber(1);
            i++;
        }
    }

}
