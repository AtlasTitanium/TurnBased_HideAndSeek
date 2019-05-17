using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Unity.Networking.Transport; 
using Unity.Collections;

using UdpCNetworkDriver = Unity.Networking.Transport.UdpNetworkDriver;

public class Server : MonoBehaviour
{
    public Text serverText;
    public UdpCNetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    void Start(){
        m_Driver = new UdpCNetworkDriver(new INetworkParameter[0]);
        if (m_Driver.Bind( NetworkEndPoint.Parse("0.0.0.0", 9000 ) ) != 0){
            //Debug.Log("Failed to bind to port ...");
            serverText.text += "\nFailed to bind to port ...";
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
        WorkServer();
    }

    private void WorkServer(){
        Debug.Log("Lenght of connections: " + m_Connections.Length);

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated){
                m_Connections.RemoveAtSwapBack(i);
                --i;
                serverText.text += "\nServer now has " + m_Connections.Length + " players";
            }
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection)){
            m_Connections.Add(c);
            serverText.text += "\nAccepted a connection";
        }

        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++){
            if (!m_Connections[i].IsCreated)
                continue;

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty){
                if (cmd == NetworkEvent.Type.Data){
                    //Try and read the data from the package
                    var readerCtx = default(DataStreamReader.Context);
                    uint number = stream.ReadUInt(ref readerCtx);

                    serverText.text += "\nClient got connected: Server now has " + m_Connections.Length + " players";

                    //Now write the number back to all the clients
                    for(int f = 0; f < m_Connections.Length; f++){
                        using (var writer = new DataStreamWriter(4, Allocator.Temp)){
                            writer.Write(number);
                            m_Driver.Send(NetworkPipeline.Null, m_Connections[f], writer);
                        }
                    }
                } else if (cmd == NetworkEvent.Type.Disconnect){
                    serverText.text += "\nClient disconnected from server";
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }


}
