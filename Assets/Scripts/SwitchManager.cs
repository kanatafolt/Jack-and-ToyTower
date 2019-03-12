////
//SwitchManager.cs
//触れると起動するスイッチオブジェクトのON/OFFステートを管理、変化時の描画をするスクリプト
//カラーコードメモ：(ライトブルー)55879A→82C6E0、(グリーン)、4A8C4F→8BF392、(ピンク)AB5FAB→F69BF8
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{
    const float CHANGE_TIME = 0.1f;                 //スイッチの変化にかかる時間

    [HideInInspector] public bool isOn = false;
    [SerializeField] float timeLimit = 0.0f;        //スイッチがOFFになるまでの制限時間　0ならOFFにならない
    [SerializeField] Color toColor = Color.white;
    [SerializeField] Vector3 moveDiff = Vector3.zero;
    [SerializeField] Light spotLight;
    private float timeElapsed;
    private bool switched = false;                  //スイッチオブジェクトの変化が完了したかどうか(isOnとは異なる)
    private Color currentColor, initialColor;
    private Vector3 currentPos, initialPos, toPos;
    private float currentIntensity, initialIntensity, toIntensity;

    private void Reset()
    {
        spotLight = transform.parent.Find("Spot Light").gameObject.GetComponent<Light>();
    }

    private void Start()
    {
        initialColor = currentColor = GetComponent<Renderer>().material.color;
        initialPos = currentPos = transform.position;
        toPos = initialPos + transform.TransformDirection(moveDiff);
        initialIntensity = currentIntensity = spotLight.intensity;
        toIntensity = 1.0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "player" && !isOn && timeElapsed / CHANGE_TIME <= 0.8f)
        {
            //スイッチにプレイヤーが触れたとき：スイッチをONにする
            isOn = true;
            switched = false;
            currentColor = GetComponent<Renderer>().material.color;
            currentPos = transform.position;
            currentIntensity = spotLight.intensity;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.tag == "player" && !isOn && timeElapsed / CHANGE_TIME <= 0.5f)
        {
            //スイッチにプレイヤーが触れたとき：スイッチをONにする
            isOn = true;
            switched = false;
            currentColor = GetComponent<Renderer>().material.color;
            currentPos = transform.position;
            currentIntensity = spotLight.intensity;
        }
    }

    private void Update()
    {

        if (isOn)
        {
            if (timeLimit == 0.0f || timeElapsed < timeLimit)
            {
                //スイッチがONのとき、制限時間が設定されていないか、あるいは制限時間を超えていないとき：スイッチの変化処理を行う
                timeElapsed += Time.deltaTime;

                if (!switched)
                {
                    float changeRate = timeElapsed / CHANGE_TIME;

                    if (changeRate > 1.0f)
                    {
                        changeRate = 1.0f;
                        switched = true;
                    }

                    GetComponent<Renderer>().material.color = new Color(currentColor.r + (toColor.r - currentColor.r) * changeRate, 
                        currentColor.g + (toColor.g - currentColor.g) * changeRate, currentColor.b + (toColor.b - currentColor.b) * changeRate);
                    transform.position = currentPos + (toPos - currentPos) * changeRate;
                    spotLight.intensity = currentIntensity + (toIntensity - currentIntensity) * changeRate;
                }

            }
            else if (timeLimit > 0.0f && timeElapsed >= timeLimit)
            {
                //スイッチがONのとき、制限時間が設定されており、制限時間を超えていたら：スイッチをOFFにする
                isOn = false;
                currentColor = GetComponent<Renderer>().material.color;
                currentPos = transform.position;
                currentIntensity = spotLight.intensity;
                timeElapsed = CHANGE_TIME;
                switched = true;
            }
        }

        if (!isOn && timeLimit > 0.0f && switched)
        {
            //スイッチがOFFのとき、制限時間が設定されており、スイッチの逆変化が完了していないとき：スイッチの逆変化処理を行う
            timeElapsed -= Time.deltaTime;
            if (timeElapsed <= 0.0f)
            {
                timeElapsed = 0.0f;
                switched = false;
            }

            float changeRate = timeElapsed / CHANGE_TIME;

            GetComponent<Renderer>().material.color = new Color(initialColor.r + (currentColor.r - initialColor.r) * changeRate,
                initialColor.g + (currentColor.g - initialColor.g) * changeRate, initialColor.b + (currentColor.b - initialColor.b) * changeRate);
            transform.position = initialPos + (currentPos - initialPos) * changeRate;
            spotLight.intensity = initialIntensity + (currentIntensity - initialIntensity) * changeRate;
        }
    }
}
