using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Unity.Networking.Transport; 
using Unity.Collections;

using UdpCNetworkDriver = Unity.Networking.Transport.UdpNetworkDriver;

public class Server : MonoBehaviour
{
    public UdpCNetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    [HideInInspector]
    public string ipString;
    [HideInInspector]
    public GameObject[] spawnLocations;
    [HideInInspector]
    public int finalScore;

    private float StartSeconds;
    private bool gameStarted = false;


    void Start(){
        m_Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        if (m_Driver.Bind( NetworkEndPoint.Parse(ipString, 9000 ) ) != 0){
            Debug.Log("Failed to bind to port ...");
        }else{
            m_Driver.Listen();
        }
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        StartSeconds = 20;
    }

    public void OnDestroy(){
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    void FixedUpdate(){
        m_Driver.ScheduleUpdate().Complete();
        Debug.Log("Connections left: " + m_Connections.Length);
        Debug.Log("Game started = " + gameStarted);

        if(StartSeconds < 0 && !gameStarted){
            if(m_Connections.Length <= 1){
                //start the game, so remove the server from the database so no one can access it anymore
                Debug.Log("not enough connections");
                GameData.Instance.Disconnect();
            } else {
                Debug.Log("game started");
                StartGame();
                StartSeconds = (60 * GameData.Instance.gametimeInMinutes);
                gameStarted = true;
            }
        }

        if(StartSeconds < 0 && gameStarted){
            Debug.Log("Start Seonds: " + StartSeconds);
            GameEnd();
        }
        if(m_Connections.Length <= 1 && gameStarted){
            Debug.Log("game end");
            GameEnd();
        }

        UpdateConnections();
        WorkServer();
        PingClients();
        StartSeconds -= Time.deltaTime;        
    }

    public void StartGame(){
        //Allow the first hider the first turn
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write((uint)ClientEvent.ALLOW_TURN);
            m_Connections[1].Send(m_Driver, writer);
        }
    }

    public void GameEnd(){
        Debug.Log("GameEnded");

        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;

            using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
                writer.Write((uint)ClientEvent.END_GAME);
                m_Connections[i].Send(m_Driver, writer);
            }
        }
    }

    #region Server Functions
    //---------Server Functions----------
    //Work incoming data
    private void WorkServer(){
        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty){
                switch (cmd){
                    case NetworkEvent.Type.Data:
                        HandleData(stream, i);
                    break;

                    case NetworkEvent.Type.Disconnect:
                        PlayerDisconnect(i);
                    break;
                }
            }
        }
    }
    
    //Check and update new connections
    private void UpdateConnections(){
        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated){
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection)){
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
        }
    }

    //Handle incoming data, read and send events
    private void HandleData(DataStreamReader stream, int connectionIndex){
        var readerCtx = default(DataStreamReader.Context);
        ServerEvent eventName = (ServerEvent)stream.ReadUInt(ref readerCtx);
        ServerEventManager.ServerEvents[eventName](this, stream, ref readerCtx, m_Connections[connectionIndex]);
    }

    //Handle player disconnections
    private void PlayerDisconnect(int playerIndex){
        Debug.Log("Client disconnected from server");
        m_Connections[playerIndex] = default(NetworkConnection);
        finalScore++;
    }

    #endregion
    
    #region Other Functions

    #region ping
    private void PingClients(){
        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;
            
            using (var writer = new DataStreamWriter(10, Allocator.Temp)) {
                writer.Write((uint)ClientEvent.PING);
                m_Connections[i].Send(m_Driver, writer);
            }
        }
    }
    public void RecievePing(){
        //Debug.Log("got ping back");
    }
    
    #endregion

    public void PlayerInit(NetworkConnection client){   //Instantiate client and other enemies for every client
        byte[] positionXInBytes = Conversions.VectorAxisToBytes(spawnLocations[m_Connections.Length-1].transform.position.x);
        byte[] positionZInBytes = Conversions.VectorAxisToBytes(spawnLocations[m_Connections.Length-1].transform.position.z);

        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;
            
            if(m_Connections[i] != client){
                //Create enemies for already existing clients
                using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                    writer.Write((uint)ClientEvent.CREATE_ENEMY);

                    writer.Write(positionXInBytes.Length);                           //Position.X lenght of Array
                    writer.Write(positionXInBytes, positionXInBytes.Length);         //PositionArray

                    writer.Write(positionZInBytes.Length);                           //Position.Z lenght of Array
                    writer.Write(positionZInBytes, positionXInBytes.Length);         //PositionArray

                    writer.Write((uint)m_Connections.Length);
                    m_Connections[i].Send(m_Driver, writer);
                }
            } else {
                //For the initializing client, send id and spawn location
                using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                    writer.Write((uint)ClientEvent.SET_CLIENT);
                    
                    writer.Write(positionXInBytes.Length);                           
                    writer.Write(positionXInBytes, positionXInBytes.Length);         

                    writer.Write(positionZInBytes.Length);                           
                    writer.Write(positionZInBytes, positionXInBytes.Length);         

                    writer.Write((uint)spawnLocations[m_Connections.Length-1].transform.eulerAngles.y); //Y rotation

                    writer.Write((float)StartSeconds); //float time

                    writer.Write((uint)m_Connections.Length);
                    client.Send(m_Driver, writer);
                }

                //And create enemies for initialized
                for (int f = 0; f < m_Connections.Length; f++){
                    byte[] positionXInBytes2 = Conversions.VectorAxisToBytes(spawnLocations[f].transform.position.x);
                    byte[] positionZInBytes2 = Conversions.VectorAxisToBytes(spawnLocations[f].transform.position.z);
                    if(m_Connections[f] != client){
                        using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                            writer.Write((uint)ClientEvent.CREATE_ENEMY);

                            writer.Write(positionXInBytes2.Length);                          
                            writer.Write(positionXInBytes2, positionXInBytes.Length);         

                            writer.Write(positionZInBytes2.Length);                         
                            writer.Write(positionZInBytes2, positionXInBytes.Length);         

                            writer.Write((uint)f+1);
                            client.Send(m_Driver, writer);
                        }
                    }
                }
            }
        }

        PingClients();
    }

    public void MovePlayer(Vector2 pos, int rot, int clientID, NetworkConnection client){   //Move the player around
        byte[] positionXInBytes = Conversions.VectorAxisToBytes(pos.x);
        byte[] positionZInBytes = Conversions.VectorAxisToBytes(pos.y);

        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;
            
            if(m_Connections[i] != client){
                using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                    writer.Write((uint)ClientEvent.MOVE_ENEMY);

                    writer.Write(positionXInBytes.Length);                           //Position.X lenght of Array
                    writer.Write(positionXInBytes, positionXInBytes.Length);         //PositionArray

                    writer.Write(positionZInBytes.Length);                           //Position.Z lenght of Array
                    writer.Write(positionZInBytes, positionXInBytes.Length);         //PositionArray

                    writer.Write((uint)rot);                                         //Y rotation

                    writer.Write((uint)clientID);
                    m_Connections[i].Send(m_Driver, writer);
                }
            }
        }
    }

    public void NextClient(NetworkConnection client){   //Allow the next player the turn
        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;
            
            if(m_Connections[i] == client){
                using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                    writer.Write((uint)ClientEvent.ALLOW_TURN);
                    if(i+1 < m_Connections.Length){
                        m_Connections[i+1].Send(m_Driver, writer);
                    } else {
                        m_Connections[0].Send(m_Driver, writer);
                    }
                }
            }
        }
    }
    
    public void ChangePlayer(bool active, int clientID, NetworkConnection client){  //Change the given client into a different object (hider ability)
        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;
            
            int f = 0;
            if(active){
                f = 1;
            }

            if(m_Connections[i] != client){
                using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                    writer.Write((uint)ClientEvent.CHANGE_ENEMIES);
                    writer.Write((uint)clientID);
                    writer.Write((uint)f);
                    m_Connections[i].Send(m_Driver, writer);
                }
            }
        }
    }
    
    public void CheckEnemy(int enemyID, bool isSeeker, NetworkConnection client){   //check the given enemy id and send if the checker is a seeker
        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;

            if(m_Connections[i] != client){
                using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                    writer.Write((uint)ClientEvent.CHECKED);
                    writer.Write((uint)enemyID);
                    if(isSeeker){
                        writer.Write((uint)1);
                    } else {
                        writer.Write((uint)0);
                    }
                    m_Connections[i].Send(m_Driver, writer);
                }
            }
        }
    }
    
    public void DisconnectAllPlayers(){  //Disconnect all players
        Debug.Log("Disconnect all players");

        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;

            using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                writer.Write((uint)ClientEvent.DISCONNECT);
                m_Connections[i].Send(m_Driver, writer);
            }
        }
    }
    
    public void DisconnectPlayer(int playerID, NetworkConnection client){   //disconnect given player
        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;

            using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                writer.Write((uint)ClientEvent.DISCONNECT_ENEMY);
                writer.Write((uint)playerID);
                m_Connections[i].Send(m_Driver, writer);
            }
        }
        Debug.Log("4. disconnect enemy");
    }
    
    #endregion
}
