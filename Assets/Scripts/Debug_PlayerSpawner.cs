////
//Debug_PlayerSpawner.cs
//プレイヤーの初期位置を指定するデバッグ用スクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_PlayerSpawner : MonoBehaviour
{
    private Transform player;

    private void Start()
    {
        player = GameObject.Find("Jack").transform;
        player.position = transform.position;
    }
}
