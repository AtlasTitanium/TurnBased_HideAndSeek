﻿using System.Collections;
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

    void Start(){
        m_Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        if (m_Driver.Bind( NetworkEndPoint.Parse(ipString, 9000 ) ) != 0){
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

    void FixedUpdate(){
        m_Driver.ScheduleUpdate().Complete();
        UpdateConnections();
        WorkServer();
        PingClients();
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
        Debug.Log("got ping back");
    }
    
    #endregion

    public void PlayerInit(Vector2 pos, NetworkConnection client){
        Debug.Log("player joined, id: " + m_Connections.Length);
        Debug.Log("Server now has " + m_Connections.Length + " players");

        byte[] positionXInBytes = Conversions.VectorAxisToBytes(pos.x);
        byte[] positionZInBytes = Conversions.VectorAxisToBytes(pos.y);
        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;
            
            if(m_Connections[i] != client){
                //For every client that is not the initializing client, create an enemy on the same location with the right id;
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
                //For the initializing client, send only id
                using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                    writer.Write((uint)ClientEvent.SET_CLIENT);
                    writer.Write((uint)m_Connections.Length);
                    client.Send(m_Driver, writer);
                }

                //and create enemies of the other connections
                for (int f = 0; f < m_Connections.Length; f++){
                    if(m_Connections[f] != client){
                        using (var writer = new DataStreamWriter(64, Allocator.Temp)) {
                            writer.Write((uint)ClientEvent.CREATE_ENEMY);

                            writer.Write(positionXInBytes.Length);                           //Position.X lenght of Array
                            writer.Write(positionXInBytes, positionXInBytes.Length);         //PositionArray

                            writer.Write(positionZInBytes.Length);                           //Position.Z lenght of Array
                            writer.Write(positionZInBytes, positionXInBytes.Length);         //PositionArray

                            writer.Write((uint)f+1);
                            client.Send(m_Driver, writer);
                        }
                    }
                }
            }
        }

        PingClients();
    }

    public void MovePlayer(Vector2 pos, int rot, int clientID, NetworkConnection client){
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
    
    #endregion
}
