using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class EventManager
{
    public static UnityAction OnGameWin;
    public static UnityAction OnGameLose;
    public static UnityAction OnGameStart;
    public static UnityAction OnBallExplode;
}