using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int ableToMove = 0;
    public bool abilityActive = true;

    public int seekerAbilityCooldown = 5;
    public int hiderAbilityCooldown = 10;

    public LayerMask wallLayer;
    public LayerMask hiderObjectLayer;
    private GameObject hiderObject;
    private float originalObjHeight;
    private float originalHeight;

    private Client client;
    private bool moved;

    void Start(){
        client = GetComponent<Client>();
    }

    void Update(){
        //move player
        if(ableToMove > 0){
            Vector3 checkSize = new Vector3(0.5f,1f,0.5f);
            if(Input.GetKeyDown(KeyCode.W)){
                if(!Physics.CheckBox(transform.position+transform.forward, checkSize, Quaternion.identity, wallLayer)){
                    transform.Translate(Vector3.forward);
                    moved = true;
                    ableToMove--;
                }
            }

            if(Input.GetKeyDown(KeyCode.A)){
                if(!Physics.CheckBox(transform.position-transform.right, checkSize, Quaternion.identity, wallLayer)){
                    transform.Translate(-Vector3.right);
                    moved = true;
                    ableToMove--;
                }
            }

            if(Input.GetKeyDown(KeyCode.S)){
                if(!Physics.CheckBox(transform.position-transform.forward, checkSize, Quaternion.identity, wallLayer)){
                    transform.Translate(-Vector3.forward);
                    moved = true;
                    ableToMove--;
                }
            }

            if(Input.GetKeyDown(KeyCode.D)){
                if(!Physics.CheckBox(transform.position+transform.right, checkSize, Quaternion.identity, wallLayer)){
                    transform.Translate(Vector3.right);
                    moved = true;
                    ableToMove--;
                }
            }

            //Rotate player
            if(Input.GetKeyDown(KeyCode.Q)){
                transform.eulerAngles = new Vector3(0,transform.eulerAngles.y - 90,0);
                moved = true;
            }

            if(Input.GetKeyDown(KeyCode.E)){
                transform.eulerAngles = new Vector3(0,transform.eulerAngles.y + 90,0);
                moved = true;
            }
            
            //Use bound ability
            if(Input.GetKeyDown(KeyCode.F) && abilityActive){
                Ability();
                ableToMove--;
            }

            //skip turn
            if(Input.GetKeyDown(KeyCode.Space)){
                SkipTurn();
                ableToMove--;
            }
        }


        if(moved){
            Move();
            moved = false;
        }
    }

    private void Move(){
        client.MovePlayer(new Vector2(transform.position.x, transform.position.z), Mathf.RoundToInt(transform.eulerAngles.y));
        if(ableToMove <= 0){
            ableToMove = 0;
            client.EndTurn();
        }
    }

    private void Ability(){
        client.Ability();
        if(ableToMove <= 0){
            ableToMove = 0;
            client.EndTurn();
        }
    }

    private void SkipTurn(){
        client.EndTurn();
    }

    public void SetHider(GameObject obj){
        originalHeight = transform.position.y;
        originalObjHeight = obj.transform.position.y;
        hiderObject = obj;
        transform.position = new Vector3(transform.position.x, hiderObject.transform.position.y, transform.position.z);
        hiderObject.transform.position = transform.position;
        hiderObject.transform.parent = transform;
        hiderObject.layer = 0;
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void RemoveHider(){
        hiderObject.transform.parent = null;
        hiderObject.transform.position = new Vector3(transform.position.x, originalObjHeight, transform.position.z);
        hiderObject.layer = 9;
        hiderObject = null;
        transform.position = new Vector3(transform.position.x, originalHeight, transform.position.z);
        GetComponent<MeshRenderer>().enabled = true;
    }
}
