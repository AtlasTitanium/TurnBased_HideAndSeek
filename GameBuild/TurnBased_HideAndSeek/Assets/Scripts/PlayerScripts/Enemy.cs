using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int id;

    public LayerMask hiderObjectLayer;
    private GameObject hiderObject;
    private float originalObjHeight;
    private float originalHeight;

    public void SetHider(){
        Collider[] hitColliders = Physics.OverlapBox(transform.position+transform.forward, new Vector3(0.8f,2f,0.8f), Quaternion.identity, hiderObjectLayer);
        if(hitColliders.Length >= 1){
            hiderObject = hitColliders[0].gameObject;
        } else {
            return;
        }

        originalHeight = transform.position.y;
        originalObjHeight = hiderObject.transform.position.y;
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
