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
        {ClientEvent.CREATE_ENEMY, CreateEnemy},
        {ClientEvent.ALLOW_TURN, NextTurn},
        {ClientEvent.CHANGE_ENEMIES, ChangeEnemy},
        {ClientEvent.DISCONNECT, Disconnect},
        {ClientEvent.DISCONNECT_ENEMY, DisconnectEnemy},
        {ClientEvent.CHECKED, Check},
        {ClientEvent.END_GAME, GameEnd}
    };

    public static void Ping(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;
        client.RecievePing();
    }

    public static void SetClient(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;

        //Get Vector2
        int positionX_StringLength = stream.ReadInt(ref context);
        byte[] positionX = stream.ReadBytesAsArray(ref context, positionX_StringLength);

        int positionZ_StringLength = stream.ReadInt(ref context);
        byte[] positionZ = stream.ReadBytesAsArray(ref context, positionZ_StringLength);

        Vector2 pos = Conversions.BytesToVector2(positionX, positionZ);

        //Get rotation
        int rot = stream.ReadInt(ref context);

        //get startTime
        float startTime = stream.ReadFloat(ref context);

        //get player id
        uint playerID = stream.ReadUInt(ref context);

        //set init variables
        client.playerID = playerID;
        client.transform.position = new Vector3(pos.x, client.transform.position.y, pos.y);
        client.transform.rotation = Quaternion.Euler(0,rot,0);
        client.startTime = startTime;
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

    public static void NextTurn(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;
        client.NextTurn();
    }

    public static void ChangeEnemy(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;

        //Get enemy id
        int enemyID = stream.ReadInt(ref context);

        //get active
        int i = stream.ReadInt(ref context);
        bool active = false;
        if(i >= 1){
            active = true;
        }

        client.ChangeEnemy(active, enemyID);
    }

    public static void Disconnect(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;
        client.Disconnect();
    }

    public static void DisconnectEnemy(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;

        int id = stream.ReadInt(ref context);
        
        client.DisconnectEnemy(id);
    }

    public static void Check(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;

        //Get id
        int id = stream.ReadInt(ref context);

        //get active
        int i = stream.ReadInt(ref context);
        bool isSeeker = false;
        if(i >= 1){
            isSeeker = true;
        }

        client.CheckedByPlayer(isSeeker, id);
    }

    public static void GameEnd(object caller, DataStreamReader stream, ref DataStreamReader.Context context, NetworkConnection source) {
        Client client = caller as Client;
        client.GameEnd();
    }
}
