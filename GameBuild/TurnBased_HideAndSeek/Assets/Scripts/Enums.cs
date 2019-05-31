using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ServerEvent{
    PING,
    NUMBER_SEND,
    INITIALIZE_PLAYER,
    MOVE
}

public enum ClientEvent{
    PING,
    SET_CLIENT,
    CREATE_ENEMY,
    MOVE_ENEMY,
    MOVE
}

public enum ClientMovement{
    MOVE_FORWARD,
    MOVE_LEFT,
    MOVE_RIGHT,
    MOVE_BACKWARD,
    ROTATE_LEFT,
    ROTATE_RIGHT
}
