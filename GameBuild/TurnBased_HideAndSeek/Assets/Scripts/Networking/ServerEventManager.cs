using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class ServerEventManager
{
    public delegate void Function(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source);

    public static readonly Dictionary<ServerEvent, Function> ServerEvents = new Dictionary<ServerEvent, Function>(){
        {ServerEvent.NUMBER_SEND, GetNumber},
        {ServerEvent.INITIALIZE_PLAYER, PlayerInit}
    };

    public static void GetNumber(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Server server = caller as Server;
        uint givenNumber = stream.ReadUInt(ref context);
        server.GetNumber(givenNumber);
    }

    public static void PlayerInit(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Server server = caller as Server;
        uint playerID = stream.ReadUInt(ref context);
        server.PlayerInit(playerID);
    }
}
