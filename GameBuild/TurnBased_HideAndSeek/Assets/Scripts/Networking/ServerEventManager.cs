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
        {ServerEvent.PING, Ping},
        {ServerEvent.INITIALIZE_PLAYER, PlayerInit},
        {ServerEvent.MOVE_CLIENT, MoveClient},
        {ServerEvent.SKIP_TURN, NextClient},
        {ServerEvent.CHANGE_PLAYER, ChangePlayer}
    };

    public static void Ping(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Server server = caller as Server;
        server.RecievePing();
    }

    public static void PlayerInit(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Server server = caller as Server;

        server.PlayerInit(source);
    }

    public static void MoveClient(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Server server = caller as Server;

        //Get Vector2
        int positionX_StringLength = stream.ReadInt(ref context);
        byte[] positionX = stream.ReadBytesAsArray(ref context, positionX_StringLength);

        int positionZ_StringLength = stream.ReadInt(ref context);
        byte[] positionZ = stream.ReadBytesAsArray(ref context, positionZ_StringLength);

        Vector2 position = Conversions.BytesToVector2(positionX, positionZ);

        //Get rotation
        int rot = stream.ReadInt(ref context);

        //Get player id
        int ID = stream.ReadInt(ref context);

        server.MovePlayer(position, rot, ID, source);
    }

    public static void NextClient(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Server server = caller as Server;
        server.NextClient(source);
    }

    public static void ChangePlayer(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Server server = caller as Server;

        //Set bool on or off
        int i = stream.ReadInt(ref context);
        bool active = false;
        if(i >= 1){
            active = true;
        }

        int id = stream.ReadInt(ref context);

        server.ChangePlayer(active, id, source);
    }
}
