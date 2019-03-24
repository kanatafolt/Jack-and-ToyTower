////
//ObjectScalePop.cs
//オブジェクトのスケールを変化させながら出現させるスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScalePop : MonoBehaviour
{
    [HideInInspector] public bool popStart = false;         //外部からの操作でポップを開始する
    [SerializeField] float popTime = 1.0f;                  //ポップにかかる時間
    [SerializeField] Vector3 goalScale = Vector3.one;       //目標のローカルスケール

    private float elapsedTime = 0.0f;
    private Vector3 initialScale;

    private void Update()
    {
        if (popStart)
        {
            if (elapsedTime == 0.0f)
            {
                //起動時、最初のローカルスケールを記録する
                initialScale = transform.localScale;
            }

            elapsedTime += Time.deltaTime;

            float scaleRate = (elapsedTime <= popTime) ? elapsedTime / popTime : 1.0f;

            //ローカルスケールを変更
            transform.localScale = initialScale + (goalScale - initialScale) * scaleRate;

            if (elapsedTime >= popTime)
            {
                //処理を終える
                popStart = false;
            }
        }
    }
}
