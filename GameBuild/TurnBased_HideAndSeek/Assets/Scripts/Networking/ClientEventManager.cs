using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class ClientEventManager
{
    public delegate void Function(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source);

    public static readonly Dictionary<ClientEvent, Function> ClientEvents = new Dictionary<ClientEvent, Function>(){
        {ClientEvent.NUMBER_SEND, GetNumber}
    };

    public static void GetNumber(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;
        uint givenNumber = stream.ReadUInt(ref context);
        client.GetNumber(givenNumber);
    }
}
