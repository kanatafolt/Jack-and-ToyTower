////
//PlayerFloorTracer.cs
//プレイヤーが接触したフロアオブジェクトの子になり、フロアの動きに追従して位置・回転移動量をプレイヤーに伝える役割を果たすスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFloorTracer : MonoBehaviour
{
    private GameObject player;
    private Vector3 prevPosition, prevRotation;

    private void Start()
    {
        player = GameObject.Find("Jack");
        transform.position = player.transform.position;
        prevPosition = transform.position;
    }

    private void Update()
    {
        player.transform.position += transform.position - prevPosition;
        prevPosition = transform.position;
    }
}
