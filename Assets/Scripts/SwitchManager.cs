////
//SwitchManager.cs
//触れると起動するスイッチオブジェクトのON/OFFステートを管理、変化時の描画をするスクリプト
//拡張案：制限時間を設定する変数を作り、時間制限付きスイッチにも対応する(未実装)
//カラーコードメモ：(ライトブルー)55879A→82C6E0、(グリーン)、4A8C4F→8BF392、(ピンク)AB5FAB→F69BF8
////

#pragma warning disable 0649    //参照先がnullのままであるという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{
    const float COLOR_CHANGE_TIME = 0.1f;

    public bool isOn = false;
    [SerializeField] Color fromColor;
    [SerializeField] Color toColor;
    [SerializeField] Vector3 moveDiff;
    [SerializeField] Light spotLight;
    private float timeElapsed;
    private bool switched = false;          //スイッチオブジェクトの変化が完了したかどうか(isOnとは異なる)
    private Vector3 defaultPos;

    private void Start()
    {
        defaultPos = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "player")
        {
            //スイッチにプレイヤーが触れたとき：スイッチをONにする
            isOn = true;
        }
    }

    private void Update()
    {
        if (isOn && !switched)
        {
            //スイッチ起動後、スイッチの変化処理を行う(スイッチ変化後は無視)
            timeElapsed += Time.deltaTime;

            float changeRate = timeElapsed / COLOR_CHANGE_TIME;

            if (changeRate > 1.0f)
            {
                changeRate = 1.0f;
                switched = true;
            }

            GetComponent<Renderer>().material.color = fromColor + new Color(toColor.r - fromColor.r, toColor.g - fromColor.g, toColor.b - fromColor.b) * changeRate;
            transform.position = defaultPos + transform.TransformDirection(moveDiff * changeRate);
            spotLight.intensity = changeRate;
        }
    }
}
