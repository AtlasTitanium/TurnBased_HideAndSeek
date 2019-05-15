using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Networking.Transport;

public class Client : MonoBehaviour
{
    public Text clientText;
    public UdpNetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool Done;
    
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

        if (!m_Connection.IsCreated){
            if (!Done){
                //Debug.Log("Something went wrong during connect");
                clientText.text += "\ndone is false";
            }
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != 
            NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                //Debug.Log("We are now connected to the server");
                clientText.text += "\nWe are now connected to the server";
                
                var value = Random.Range(0,10);
                //Debug.Log("sending " + value);
                clientText.text += "\nsending " + value;
                using (var writer = new DataStreamWriter(4, Allocator.Temp))
                {
                    writer.Write(value);
                    m_Connection.Send(m_Driver, writer);
                }
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var readerCtx = default(DataStreamReader.Context);
                uint value = stream.ReadUInt(ref readerCtx);
                //Debug.Log("Got the value = " + value + " back from the server");
                clientText.text += "\nGot the value = " + value + " back from the server";
                Done = true;
                //m_Connection.Disconnect(m_Driver);
                m_Connection = default(NetworkConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                //Debug.Log("Client got disconnected from server");
                clientText.text += "\nClient got disconnected from server";
                m_Connection = default(NetworkConnection);
            }
        }
    }
}
