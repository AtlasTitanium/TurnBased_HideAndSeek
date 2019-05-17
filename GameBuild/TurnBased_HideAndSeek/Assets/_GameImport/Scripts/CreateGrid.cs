using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RelocationDirection{Right, Left, Front, Back}
public class CreateGrid : MonoBehaviour
{
    //Publics
    public int gridSizeX, gridSizeY;
    public LayerMask playerLayer;
    public LayerMask obstructionLayer;

    //Privates
    public Node[,] grid;
    int nodeDiameter = 1;
    float nodeRadius = 0.5f;

    void Start(){
        MakeGrid();
    }

    private void MakeGrid(){
        //Create grid and start at the bottom left.
        grid = new Node[gridSizeX,gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridSizeX/2 - Vector3.forward * gridSizeY/2;
        
        //Make each node on the correct location, and check it's status.
        for(int x = 0; x < gridSizeX; x++){
            for(int y = 0; y < gridSizeY; y++){
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                grid[x,y] = new Node(worldPoint);
            }
        }
    }

    private void Update(){
        foreach(Node n in grid){
            Vector3 checkbox = new Vector3(nodeRadius-0.2f, nodeRadius-0.2f, nodeRadius-0.2f);
            bool hasPlayer = Physics.CheckBox(n.location, checkbox, Quaternion.identity ,playerLayer);
            bool hasObstruction = Physics.CheckBox(n.location, checkbox, Quaternion.identity ,obstructionLayer);
            n.hasPlayer = hasPlayer;
            n.isObstructed = hasObstruction;
        }
    }

    //Draw each node and a correct color for it's status.
    private void OnDrawGizmos(){
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSizeX, 1, gridSizeY));

        if(grid != null){
            foreach(Node n in grid){
                Gizmos.color = Color.white;

                if(n.hasPlayer){
                    Gizmos.color = Color.green;
                }
                if(n.isObstructed){
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawCube(n.location, new Vector3(0.9f, 1, 0.9f));
            }
        }
    }

    public void CheckRelocation(RelocationDirection direction, GameObject player){
        for(int x = 0; x < gridSizeX; x++){
            for(int y = 0; y < gridSizeY; y++){
                Collider[] incurrentLocation = Physics.OverlapBox(grid[x,y].location, new Vector3(nodeRadius-0.2f, nodeRadius-0.2f, nodeRadius-0.2f), Quaternion.identity, playerLayer);
                foreach(Collider c in incurrentLocation){
                    if(c == player.GetComponent<Collider>()){
                        switch(direction){
                            case RelocationDirection.Right:
                                if(x+1 >= gridSizeX){
                                    //Debug.Log("no way Hose");
                                } else {
                                    if(grid[x+1,y].isObstructed){
                                    // Debug.Log("there's a bitch here");
                                    } else {
                                        //Debug.Log("no bitch here");
                                        player.transform.position = grid[x+1,y].location;
                                    }
                                }
                            break;
                            case RelocationDirection.Left:
                                if(x-1 <= -1){
                                    //Debug.Log("no way Hose");
                                } else {
                                    if(grid[x-1,y].isObstructed){
                                        //Debug.Log("there's a bitch here");
                                    } else {
                                        //Debug.Log("no bitch here");
                                        player.transform.position = grid[x-1,y].location;
                                    }
                                }
                            break;
                            case RelocationDirection.Front:
                                if(y+1 >= gridSizeY){
                                    //Debug.Log("no way Hose");
                                } else {
                                    if(grid[x,y+1].isObstructed){
                                        //Debug.Log("there's a bitch here");
                                    } else {
                                        //Debug.Log("no bitch here");
                                        player.transform.position = grid[x,y+1].location;
                                    }
                                }
                            break;
                            case RelocationDirection.Back:
                                if(y-1 <= -1){
                                    //Debug.Log("no way Hose");
                                } else {
                                    if(grid[x,y-1].isObstructed){
                                        //Debug.Log("there's a bitch here");
                                    } else {
                                        //Debug.Log("no bitch here");
                                        player.transform.position = grid[x,y-1].location;
                                    }
                                }
                            break;
                        }
                    }
                }
            }
        }
    }
}
