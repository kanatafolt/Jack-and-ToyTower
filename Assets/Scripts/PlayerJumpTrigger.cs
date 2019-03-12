////
//PlayerJumpTrigger.cs
//プレイヤーキャラクターのトリガーにアタッチし、トリガーの接触状態を個別に管理するスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpTrigger : MonoBehaviour
{
    [HideInInspector] public bool contacting;

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "floor")
        {
            contacting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "floor")
        {
            contacting = false;
        }
    }
}
