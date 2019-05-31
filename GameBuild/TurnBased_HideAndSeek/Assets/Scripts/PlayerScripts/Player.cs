using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Client client;
    private bool moved;

    void Start(){
        client = GetComponent<Client>();
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.W)){
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
            moved = true;
        }

        if(Input.GetKeyDown(KeyCode.A)){
            transform.position = new Vector3(transform.position.x  - 1, transform.position.y, transform.position.z);
            moved = true;
        }

        if(Input.GetKeyDown(KeyCode.S)){
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
            moved = true;
        }

        if(Input.GetKeyDown(KeyCode.D)){
            transform.position = new Vector3(transform.position.x  + 1, transform.position.y, transform.position.z);
            moved = true;
        }

        if(moved){
            Move();
            moved = false;
        }
    }

    private void Move(){
        client.MovePlayer(new Vector2(transform.position.x, transform.position.z), 0);
    }

}
