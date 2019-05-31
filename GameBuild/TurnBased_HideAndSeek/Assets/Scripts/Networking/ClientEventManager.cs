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
        {ClientEvent.PING, Ping},
        {ClientEvent.SET_CLIENT, SetClient},
        {ClientEvent.MOVE_ENEMY, MoveEnemy},
        {ClientEvent.CREATE_ENEMY, CreateEnemy}
    };

    public static void Ping(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;
        client.RecievePing();
    }

    public static void SetClient(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;
        uint playerID = stream.ReadUInt(ref context);
        client.playerID = playerID;
    }

    public static void MoveEnemy(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;

        //Get Vector2
        int positionX_StringLength = stream.ReadInt(ref context);
        byte[] positionX = stream.ReadBytesAsArray(ref context, positionX_StringLength);

        int positionZ_StringLength = stream.ReadInt(ref context);
        byte[] positionZ = stream.ReadBytesAsArray(ref context, positionZ_StringLength);

        Vector2 pos = Conversions.BytesToVector2(positionX, positionZ);
        
        //Get rotation
        int rot = stream.ReadInt(ref context);

        //Get enemy id
        int enemyID = stream.ReadInt(ref context);

        client.MoveEnemy(enemyID, pos, rot);
    }

    public static void CreateEnemy(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;

        //Get Vector2
        int positionX_StringLength = stream.ReadInt(ref context);
        byte[] positionX = stream.ReadBytesAsArray(ref context, positionX_StringLength);

        int positionZ_StringLength = stream.ReadInt(ref context);
        byte[] positionZ = stream.ReadBytesAsArray(ref context, positionZ_StringLength);

        Vector2 pos = Conversions.BytesToVector2(positionX, positionZ);
    
        //Get enemy id
        int enemyID = stream.ReadInt(ref context);

        client.CreateEnemy(pos, enemyID);
    }
}
