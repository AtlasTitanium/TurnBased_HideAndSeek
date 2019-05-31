using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Networking.Transport;

public class Client : MonoBehaviour
{
    public GameObject enemyPrefab;
    public UdpNetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public uint playerID;

    [HideInInspector]
    public string ipAdress;
    private List<Enemy> enemies = new List<Enemy>();
    private bool connected;
    
    void Start () { 
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

            writer.Write(positionXInBytes.Length);                           //Position.X lenght of Array
            writer.Write(positionXInBytes, positionXInBytes.Length);         //PositionArray

            writer.Write(positionZInBytes.Length);                           //Position.Z lenght of Array
            writer.Write(positionZInBytes, positionXInBytes.Length);         //PositionArray

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

    public void MovePlayer(Vector2 pos, int rot){
        byte[] positionXInBytes = Conversions.VectorAxisToBytes(pos.x);
        byte[] positionZInBytes = Conversions.VectorAxisToBytes(pos.y);

        using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
            writer.Write((uint)ServerEvent.MOVE);

            writer.Write(positionXInBytes.Length);                           //Position.X lenght of Array
            writer.Write(positionXInBytes, positionXInBytes.Length);         //PositionArray

            writer.Write(positionZInBytes.Length);                           //Position.Z lenght of Array
            writer.Write(positionZInBytes, positionXInBytes.Length);         //PositionArray

            writer.Write((uint)rot);                                         //Y rotation

            writer.Write((uint)playerID);

            m_Connection.Send(m_Driver, writer);
        }
    }

    public void MoveEnemy(int enemyID, Vector2 _pos, int _rot){
        foreach(Enemy enemy in enemies){
            if(enemy.id == enemyID){
                enemy.transform.position = new Vector3(_pos.x, this.transform.position.y, _pos.y);
                enemy.transform.rotation = Quaternion.Euler(0,_rot,0);
            }
        }
    }

    public void CreateEnemy(Vector2 position, int enemyID){
        Vector3 pos = new Vector3(position.x, this.transform.position.y, position.y);
        GameObject enemyObj = Instantiate(enemyPrefab, pos, Quaternion.identity);
        Enemy enemy = enemyObj.AddComponent<Enemy>();
        enemy.id = enemyID;
        enemies.Add(enemy);
    }

    #endregion
}
