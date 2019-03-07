////
//InBorderArea.cs
//カメラを回転させるため、プレイヤーキャラクターがボーダーエリアに入っているかを保持するスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InBorderArea : MonoBehaviour
{
    [HideInInspector] public bool inArea = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "player")
        {
            inArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "player")
        {
            inArea = false;
        }
    }
}
