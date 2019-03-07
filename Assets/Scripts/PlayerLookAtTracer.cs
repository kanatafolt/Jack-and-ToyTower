////
//PlayerLookAtTracer.cs
//常にタワーの芯(0, y, 0)から「プレイヤーと高さを合わせ」「プレイヤーをLookAtする」監視スクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAtTracer : MonoBehaviour
{
    [SerializeField]private Transform player, towerRig;

    private void Reset()
    {
        player = GameObject.Find("Jack").GetComponent<Transform>();
        towerRig = GameObject.Find("TowerRig").GetComponent<Transform>();
    }

    private void Update()
    {
        transform.position = new Vector3(towerRig.position.x, player.position.y, towerRig.position.z);
        transform.LookAt(player);
    }
}
