using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //Publics
    [Header("AbilitySpecifics")]
    public int stepsPerTurn;
    public int seekerAbilityCooldown = 5;
    public int hiderAbilityCooldown = 10;

    [Header("CheckLayers")]
    public LayerMask wallLayer;
    public LayerMask enemyLayer;
    public LayerMask hiderObjectLayer;

    [Header("UI control")]
    public Text title;
    public Text username;
    public Text countDownTime;
    public Text time;
    public Text stepsLeft;
    public RectTransform abilitySlider;
    public Button disconnect;


    
    //public but unchangable variables
    [HideInInspector]
    public bool abilityActive = true;
    [HideInInspector]
    public int ableToMove = 0;
    [HideInInspector]
    public int realTime;

    //private variables
    private GameObject hiderObject;
    private Client client;
    private float actualTime;
    private float originalObjHeight;
    private float originalHeight;
    private bool uiInit = true;
    private bool moved;


    void Start(){
        client = GetComponent<Client>();

        if(client.seeker){
            title.text = "Seeker";
        } else {
            title.text = "Hider";
        }
        username.text = GameData.Instance.GetUsername();
        disconnect.onClick.AddListener(Disconnect);
    }

    public void Disconnect(){
        GameData.Instance.Disconnect();
    }

    void Update(){
        //control ui
        if(username.text == ""){
            username.text = GameData.Instance.GetUsername() + " id= " + client.playerID;
        }

        if(client.startTime > 0 && uiInit){
            actualTime = client.startTime;
            uiInit = false;
        }

        if(countDownTime.isActiveAndEnabled && !uiInit){
            if(actualTime > 0){
                time.gameObject.SetActive(false);
                actualTime -= Time.deltaTime;
                realTime = Mathf.RoundToInt(actualTime);
                countDownTime.text = realTime.ToString();
            } else {
                countDownTime.gameObject.SetActive(false);
                time.gameObject.SetActive(true);
                actualTime = (60 * GameData.Instance.gametimeInMinutes);
            }
        } else if(time.isActiveAndEnabled  && !uiInit) {
            if(actualTime > 0){
                actualTime -= Time.deltaTime;
                realTime = Mathf.RoundToInt(actualTime);
                time.text = Conversions.toTime(realTime);
            }
        }

        stepsLeft.text = "Steps left: " + ableToMove;
      
        abilitySlider.anchorMax = new Vector2(1, (float)client.cooldownTimer / (float)seekerAbilityCooldown);
        abilitySlider.offsetMax = new Vector2 (0, 0);
        

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
                if(Physics.CheckBox(transform.position+transform.forward, checkSize, Quaternion.identity, enemyLayer)){
                    Collider[] c = Physics.OverlapBox(transform.position+transform.forward, checkSize, Quaternion.identity, enemyLayer);
                    if(c.Length >= 1){
                        client.CheckEnemy(c[0].gameObject.GetComponent<Enemy>().id);
                        return;
                    }
                } 
                SkipTurn();
                ableToMove = 0;
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
