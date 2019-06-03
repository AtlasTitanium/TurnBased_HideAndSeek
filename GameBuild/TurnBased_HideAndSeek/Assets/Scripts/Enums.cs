using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ServerEvent{
    PING,
    INITIALIZE_PLAYER,
    MOVE_CLIENT,
    SKIP_TURN,
    CHANGE_PLAYER
}

public enum ClientEvent{
    PING,
    SET_CLIENT,
    CREATE_ENEMY,
    MOVE_ENEMY,
    ALLOW_TURN,
    CHANGE_ENEMIES
}
