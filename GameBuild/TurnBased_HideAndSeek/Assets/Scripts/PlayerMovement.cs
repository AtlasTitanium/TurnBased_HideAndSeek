using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    void Update(){
        if(Input.GetKeyDown(KeyCode.W)){
            Vector3 newPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
        }
    }

}
