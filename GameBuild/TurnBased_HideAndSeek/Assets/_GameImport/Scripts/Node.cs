using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 location;
    public bool hasPlayer = false;
    public bool isObstructed = false;

    public Node(Vector3 loc){
        location = loc;
    }
}
