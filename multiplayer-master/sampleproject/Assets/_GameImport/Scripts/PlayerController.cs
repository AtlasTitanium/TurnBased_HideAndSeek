using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public LayerMask enemyLayer;

    public bool inputChange = false;
    public CreateGrid currentGrid;
    private int forwardNumber = 1;
    private int rightNumber = 2;
    private int backwardNumber = 3;
    private int leftNumber = 4;

    private void Update(){
        //Walking around
        if(!inputChange){
            if(Input.GetKeyDown(KeyCode.D)){
                Move(2);
            }
            if(Input.GetKeyDown(KeyCode.A)){
                Move(4);
            }
            if(Input.GetKeyDown(KeyCode.W)){
                Move(1);
            }
            if(Input.GetKeyDown(KeyCode.S)){
                Move(3);
            }
            
            if(Input.GetKeyDown(KeyCode.E)){
                Debug.Log("rotate Right");
                transform.Rotate(0,90,0);
                forwardNumber--;
                backwardNumber--;
                leftNumber--;
                rightNumber--;
                
            }
            if(Input.GetKeyDown(KeyCode.Q)){
                Debug.Log("Rotate left");
                transform.Rotate(0,-90,0);
                forwardNumber++;
                backwardNumber++;
                leftNumber++;
                rightNumber++;
            }
        } else {
            if(Input.GetKeyDown(KeyCode.RightArrow)){
                Move(2);
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow)){
                Move(4);
            }
            if(Input.GetKeyDown(KeyCode.UpArrow)){
                Move(1);
            }
            if(Input.GetKeyDown(KeyCode.DownArrow)){
                Move(3);
            }
            
            if(Input.GetKeyDown(KeyCode.Period)){
                Debug.Log("rotate Right");
                transform.Rotate(0,90,0);
                forwardNumber--;
                backwardNumber--;
                leftNumber--;
                rightNumber--;
                
            }
            if(Input.GetKeyDown(KeyCode.Comma)){
                Debug.Log("Rotate left");
                transform.Rotate(0,-90,0);
                forwardNumber++;
                backwardNumber++;
                leftNumber++;
                rightNumber++;
            }
        }

        if(forwardNumber <= 0){
            forwardNumber = 4;
        }
        if(backwardNumber <= 0){
            backwardNumber = 4;
        }
        if(rightNumber <= 0){
            rightNumber = 4;
        }
        if(leftNumber <= 0){
            leftNumber = 4;
        }

        if(forwardNumber >= 5){
            forwardNumber = 1;
        }
        if(backwardNumber >= 5){
            backwardNumber = 1;
        }
        if(rightNumber >= 5){
            rightNumber = 1;
        }
        if(leftNumber >= 5){
            leftNumber = 1;
        }

        //Check for other player;
        if(Physics.CheckBox(transform.position, transform.localScale, Quaternion.identity, enemyLayer)){
            Debug.Log("WIN!");
            Destroy(this.gameObject);
        }
    }

    private void Move(int moveNumber){
        if(forwardNumber == moveNumber){
            Debug.Log("forward");
            currentGrid.CheckRelocation(RelocationDirection.Front, this.gameObject);
        }
        if(rightNumber == moveNumber){
            Debug.Log("Right");
            currentGrid.CheckRelocation(RelocationDirection.Right, this.gameObject);
        }
        if(backwardNumber == moveNumber){
            Debug.Log("backward");
            currentGrid.CheckRelocation(RelocationDirection.Back, this.gameObject);
        }
        if(leftNumber == moveNumber){
            Debug.Log("left");
            currentGrid.CheckRelocation(RelocationDirection.Left, this.gameObject);
        }
    }
}
