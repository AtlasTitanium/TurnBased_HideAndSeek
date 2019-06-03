using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Networking.Transport;

public class Client : MonoBehaviour
{
    public int seekerAmountOfAbilitySteps;
    public GameObject enemyPrefab;
    public UdpNetworkDriver m_Driver;
    public NetworkConnection m_Connection;

    //initializable variables
    [HideInInspector]
    public uint playerID;
    [HideInInspector]
    public string ipAdress;
    [HideInInspector]
    public bool seeker = false;

    //Privates
    private Player playerController;
    private List<Enemy> enemies = new List<Enemy>();
    private bool connected;
    private int cooldownTimer;

    void Start () { 
        playerController = GetComponent<Player>();

        //Setup server connection
        m_Driver = new UdpNetworkDriver(new INetworkParameter[0]);
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.Parse(ipAdress, 9000);
        m_Connection = m_Driver.Connect(endpoint);

        StartCoroutine(WaitIfConnected(15));
    }

    public void OnDestroy() { 
        m_Driver.Dispose();
    }

    void FixedUpdate() { 
        m_Driver.ScheduleUpdate().Complete();
        if (!m_Connection.IsCreated){
            return;
        }
        WorkClient();
    }

    #region Client Functions
    //--------------Client Functions------------    
    //wait x seconds and disconnect if it couldn't find a host
    IEnumerator WaitIfConnected(int seconds){
        yield return new WaitForSeconds(seconds);
        if (!connected){
            GameData.Instance.Disconnect();
        }
    }

    //Get data from the server
    private void WorkClient(){
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty) {
            switch (cmd){
                case NetworkEvent.Type.Connect:
                    ConnectClient();
                break;
                
                case NetworkEvent.Type.Data:
                    HandleData(stream);
                break;

                case NetworkEvent.Type.Disconnect:
                    DisconnectClient();
                break;
            }
        }
    }

    //Connect the client 
    private void ConnectClient(){
        connected = true;

        byte[] positionXInBytes = Conversions.VectorAxisToBytes(transform.position.x);
        byte[] positionZInBytes = Conversions.VectorAxisToBytes(transform.position.z);

        using (var writer = new DataStreamWriter(64, Allocator.Temp))
        {
            writer.Write((uint)ServerEvent.INITIALIZE_PLAYER);
            m_Connection.Send(m_Driver, writer);
        }
    }

    //Handle incominhg data and check it's event
    private void HandleData(DataStreamReader stream){
        var readerCtx = default(DataStreamReader.Context);
        ClientEvent eventName = (ClientEvent)stream.ReadUInt(ref readerCtx);
        ClientEventManager.ClientEvents[eventName](this, stream, ref readerCtx, m_Connection);
    }

    //Disconnect client correctly
    private void DisconnectClient(){
        Debug.Log("Disconnect from server");
        m_Connection = default(NetworkConnection); //make sure connection is true
    }
    #endregion
    
    #region Player Functions

    #region ping
    public void PingServer(){
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write((uint)ServerEvent.PING);
            m_Connection.Send(m_Driver, writer);
        }
    }

    public void RecievePing(){
        PingServer();
    }
    #endregion

    #region Functions called from Server
    //Move the given enemy
    public void MoveEnemy(int enemyID, Vector2 _pos, int _rot){
        foreach(Enemy enemy in enemies){
            if(enemy.id == enemyID){
                enemy.transform.position = new Vector3(_pos.x, enemy.transform.position.y, _pos.y);
                enemy.transform.rotation = Quaternion.Euler(0,_rot,0);
            }
        }
        MovePlayer(transform.position, Mathf.RoundToInt(transform.rotation.y));
    }

    //create another player
    public void CreateEnemy(Vector2 _pos, int enemyID){
        Vector3 pos = new Vector3(_pos.x, this.transform.position.y, _pos.y);
        GameObject enemyObj = Instantiate(enemyPrefab, pos, Quaternion.identity);
        Enemy enemy = enemyObj.AddComponent<Enemy>();
        enemy.hiderObjectLayer = playerController.hiderObjectLayer;
        enemy.id = enemyID;
        enemies.Add(enemy);
        MovePlayer(transform.position, Mathf.RoundToInt(transform.rotation.y));
    }

    //it's this client's turn!
    public void NextTurn(){
        playerController.ableToMove = 1;

        if(!playerController.abilityActive){
            cooldownTimer++;
            if(seeker){
                if(playerController.seekerAbilityCooldown == cooldownTimer){
                    cooldownTimer = 0;
                    playerController.abilityActive = true;
                }
            } else {
                if(playerController.hiderAbilityCooldown == cooldownTimer){
                    //change the players in the game
                    using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                        writer.Write((uint)ServerEvent.CHANGE_PLAYER);
                        writer.Write((uint)0); //on or off;
                        writer.Write((uint)playerID);
                        m_Connection.Send(m_Driver, writer);
                    }
                    playerController.RemoveHider();
                    cooldownTimer = 0;
                    playerController.abilityActive = true;
                }
            }
        }
    }

    //change the enemy (hider ability)
    public void ChangeEnemy(bool active, int enemyID){
        foreach(Enemy enemy in enemies){
            if(enemy.id == enemyID){
                if(active){
                    enemy.SetHider();
                } else {
                    enemy.RemoveHider();
                }
            }
        }
    }

    #endregion
    
    #region Functions called from Player
    //Move the player around
    public void MovePlayer(Vector2 pos, int rot){
        byte[] positionXInBytes = Conversions.VectorAxisToBytes(pos.x);
        byte[] positionZInBytes = Conversions.VectorAxisToBytes(pos.y);

        using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
            writer.Write((uint)ServerEvent.MOVE_CLIENT);

            writer.Write(positionXInBytes.Length);                           //Position.X lenght of Array
            writer.Write(positionXInBytes, positionXInBytes.Length);         //PositionArray

            writer.Write(positionZInBytes.Length);                           //Position.Z lenght of Array
            writer.Write(positionZInBytes, positionXInBytes.Length);         //PositionArray

            writer.Write((uint)rot);                                         //Y rotation

            writer.Write((uint)playerID);

            m_Connection.Send(m_Driver, writer);
        }
    }

    //use the ability corresponding if the player is a hider or a seeker;
    public void Ability(){
        if(seeker){
            //able to move multiple times at once
            playerController.ableToMove += seekerAmountOfAbilitySteps;
            playerController.abilityActive = false;
        } else {
            Collider[] hitColliders = Physics.OverlapBox(transform.position+transform.forward, new Vector3(0.8f,2f,0.8f), Quaternion.identity, playerController.hiderObjectLayer);
            if(hitColliders.Length >= 1){
                Debug.Log("there's an object");
                //if found changeable object, change into the first found object;
                playerController.SetHider(hitColliders[0].gameObject);

                playerController.abilityActive = false;
                
                //change the players in the game
                using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                    writer.Write((uint)ServerEvent.CHANGE_PLAYER);
                    writer.Write((uint)1); //on or off;
                    writer.Write((uint)playerID);
                    m_Connection.Send(m_Driver, writer);
                }
            }
            playerController.ableToMove++;
        }

        
    }
    
    //End the player's turn
    public void EndTurn(){
        //end turn
        using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
            writer.Write((uint)ServerEvent.SKIP_TURN);
            m_Connection.Send(m_Driver, writer);
        }
    }

    #endregion

    #endregion
}
