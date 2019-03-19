////
//PlayerCollisionSwitcher.cs
//プレイヤーの当たり判定を透過・不透過に変換する呼びだし用スクリプト
////

#pragma warning disable 0649    //変数が初期化されていないという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionSwitcher : MonoBehaviour
{
    [SerializeField] GameObject[] colList;

    public void SetCollisionOn()
    {
        for (int i = 0; i < colList.Length; i++) colList[i].layer = LayerMask.NameToLayer("Player");
    }

    public void SetCollisionOff()
    {
        for (int i = 0; i < colList.Length; i++) colList[i].layer = LayerMask.NameToLayer("TransparentPlayer");
    }
}
