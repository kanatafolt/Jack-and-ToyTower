////
//TriggerContactingOrNot.cs
//トリガーに付与し、特定のtagオブジェクトと接触しているかどうかを保持するスクリプト
//使用例：
//・プレイヤーキャラクターのトリガーにアタッチし、ジャンプ可能かどうかを識別する
//・セーフティネットマスクにアタッチし、ネットの範囲内かどうかを判別する
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerContactingOrNot : MonoBehaviour
{
    [HideInInspector] public bool contacting = false;
    [SerializeField] string contactTagName = "";

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == contactTagName)
        {
            contacting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == contactTagName)
        {
            contacting = false;
        }
    }
}
