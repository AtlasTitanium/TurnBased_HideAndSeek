using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Networking.Transport;

public class Client : MonoBehaviour
{
    public uint playerID;
    public UdpNetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    
    void Start () { 
        m_Driver = new UdpNetworkDriver(new INetworkParameter[0]);
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.Parse("127.0.0.1", 9000);
        m_Connection = m_Driver.Connect(endpoint);
    }
    public void OnDestroy() { 
        m_Driver.Dispose();
    }

    void Update() { 
        m_Driver.ScheduleUpdate().Complete();
        WorkClient();
        SendNumber(0);
    }

    private void WorkClient(){
        if (!m_Connection.IsCreated){
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty) {
            switch (cmd){
                case NetworkEvent.Type.Connect:
                    ConnectClient(stream);
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

    public void SendNumber(int num){
        using (var writer = new DataStreamWriter(8, Allocator.Temp)) {
            writer.Write((uint)ServerEvent.NUMBER_SEND);
            writer.Write(num);
            m_Connection.Send(m_Driver, writer);
        }
    }

    private void HandleData(DataStreamReader stream){
        //read incoming data
        var readerCtx = default(DataStreamReader.Context);
        ClientEvent eventName = (ClientEvent)stream.ReadUInt(ref readerCtx);
        ClientEventManager.ClientEvents[eventName](this, stream, ref readerCtx, m_Connection);
    }

    private void ConnectClient(DataStreamReader stream){
        playerID = (uint)Random.Range(0,100);
        Debug.Log("sending " + playerID + ". check if id exists (This is only a test, for the real deal, the game has to gain a player login, which is connected to a player id, which is then connected to player data like location and stuff");
        using (var writer = new DataStreamWriter(10, Allocator.Temp))
        {
            writer.Write((uint)ServerEvent.INITIALIZE_PLAYER);
            writer.Write(playerID);
            m_Connection.Send(m_Driver, writer);
        }
    }

    private void DisconnectClient(){
        Debug.Log("Disconnect from server");
        m_Connection = default(NetworkConnection); //make sure connection is true
    }

    public void GetNumber(uint number){
        Debug.Log("got update number from Server: " + number);
    }
}
