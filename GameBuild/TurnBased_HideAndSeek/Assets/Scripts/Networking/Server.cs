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

    void Start(){
        m_Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        if (m_Driver.Bind( NetworkEndPoint.Parse("0.0.0.0", 9000 ) ) != 0){
            Debug.Log("Failed to bind to port ...");
        }else{
            m_Driver.Listen();
        }
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    public void OnDestroy(){
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    void Update(){
        m_Driver.ScheduleUpdate().Complete();
        UpdateConnections();
        WorkServer();
        SendUpdate();
    }

    private void WorkServer(){
        Debug.Log("Lenght of connections: " + m_Connections.Length);

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

        Debug.Log("Server now has " + m_Connections.Length + " players");
    }

    private void HandleData(DataStreamReader stream, int connectionIndex){
        //read incoming data
        var readerCtx = default(DataStreamReader.Context);
        ServerEvent eventName = (ServerEvent)stream.ReadUInt(ref readerCtx);
        ServerEventManager.ServerEvents[eventName](this, stream, ref readerCtx, m_Connections[connectionIndex]);
    }

    private void SendUpdate(){
        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;

            uint updateInt = 0;
            using (var writer = new DataStreamWriter(10, Allocator.Temp)) {
                writer.Write((uint)ClientEvent.NUMBER_SEND);
                writer.Write((uint)updateInt);
                m_Connections[i].Send(m_Driver, writer);
            }
        }
    }

    private void PlayerDisconnect(int playerIndex){
        Debug.Log("Client disconnected from server");
        m_Connections[playerIndex] = default(NetworkConnection);
    }

    public void GetNumber(uint givenNumber){
        Debug.Log("got update number from Client: " + givenNumber);
    }

    public void PlayerInit(uint playerID){
        Debug.Log("player joined, id: " + playerID);
    }
}
