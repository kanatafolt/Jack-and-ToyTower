////
//PlayerLookAtTracer.cs
//常にタワーの芯(0, y, 0)から「プレイヤーと高さを合わせ」「プレイヤーをLookAtする」transform監視スクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAtTracer : MonoBehaviour
{
    [SerializeField]private Transform player, tower;

    private void Reset()
    {
        player = GameObject.Find("Jack").GetComponent<Transform>();
        tower = GameObject.Find("ToyTower").GetComponent<Transform>();
    }

    private void Update()
    {
        transform.position = new Vector3(tower.position.x, player.position.y, tower.position.z);
        transform.LookAt(player);
    }
}
