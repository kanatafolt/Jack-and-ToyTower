////
//ObjectToAndFrom.cs
//一定時間周期でステージオブジェクトを往復運動させるスクリプト
//左右方向には回転、上下・前後方向には並進を行う。ロール軸(forward軸)での傾き回転も行える
//片道無限運動も可能
//
//注意事項：
//左右回転と前後並進は併用できないため、その場合は親の回転リグオブジェクトに左右回転を、子のステージオブジェクトに前後並進を付与する必要がある
//
//メモ：rigidbodyのinterpolation(描画補間)設定を使うかどうか？　すべてのオブジェクトに適用すると重いとのこと　現在は未設定
//　　　同様にcollision detection(衝突判定補間)モードも負荷を考えて未設定　壁抜けなどが問題になったら改めて考える
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectToAndFrom : MonoBehaviour
{
    private bool isOn = false;                              //falseのときオブジェクトの運動は停止する
    private bool isReturn = false;                          //falseなら往路、trueなら復路
    private bool isInterval = false;                        //インターバル(往路・復路間の待機時間)中かどうか
    [HideInInspector] public bool pausing = false;          //trueのときオブジェクトの運動を一時停止させる

    [SerializeField] bool isInfinity = false;                       //片道無限運動設定：trueのとき、isReturnに関わらず常に往路方向へ進む
    [SerializeField] float leftAndRightRotateAngle = 0.0f;          //左右方向への回転量(world.up軸回転)
    [SerializeField] float upAndDownMoveDistance = 0.0f;            //上下方向への移動量(local.up方向並進)
    [SerializeField] float forwardAndBackMoveDistance = 0.0f;       //前後方向への移動量(local.forward方向並進)
    [SerializeField] float tiltRotateAngle = 0.0f;                  //ロール軸での回転量(local.forward軸回転)
    [SerializeField] float moveTime = 1.0f;                         //片道の移動にかかる時間
    [SerializeField] float intervalTime = 1.5f;                     //折り返すまでの待機時間
    [SerializeField] float delayTime = 0.0f;                        //初回起動までの遅延時間

    private Rigidbody rb;
    private float elapsedTime;

    private MainGameManager gameManager;
    private AudioManager audioManager;
    private AudioSource audioSource;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<MainGameManager>();
        audioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        audioSource = gameObject.AddComponent<AudioSource>();

        if (delayTime > 0.0f)
        {
            //ディレイタイムが設定されている場合、初回のみ待機時間が発生する
            isInterval = true;
            elapsedTime = intervalTime - delayTime;
        }
    }

    private void FixedUpdate()
    {
        if (!isOn)
        {
            //共通起動条件：タワーの出現が完了する
            if (gameManager.towerAppearanced)
            {
                isOn = true;

                //初回起動処理：対象オブジェクトにRigidbodyを付与または取得する
                rb = (GetComponent<Rigidbody>() != null) ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }
        }

        if (isOn && !pausing)
        {
            if (isInterval)
            {
                //インターバル中の処理
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= intervalTime)
                {
                    isInterval = false;
                    elapsedTime = (isReturn) ? moveTime : 0.0f;
                }
            }

            if (!isInterval)
            {
                //移動中の処理
                float deltaMoveRate = 0.0f;
                if (!isReturn)
                {
                    elapsedTime += Time.deltaTime;
                    deltaMoveRate = (elapsedTime <= moveTime) ? Time.deltaTime : Time.deltaTime - (elapsedTime - moveTime);
                }
                else
                {
                    elapsedTime -= Time.deltaTime;
                    deltaMoveRate = (elapsedTime >= 0.0f) ? -Time.deltaTime : -Time.deltaTime - elapsedTime;
                }
                deltaMoveRate = deltaMoveRate / moveTime;

                //移動・回転を行う
                rb.MovePosition(transform.TransformDirection(Vector3.up * upAndDownMoveDistance + Vector3.forward * forwardAndBackMoveDistance) * deltaMoveRate + rb.position);
                rb.MoveRotation(Quaternion.AngleAxis(tiltRotateAngle * deltaMoveRate, transform.forward) * Quaternion.AngleAxis(leftAndRightRotateAngle * deltaMoveRate, Vector3.up) * rb.rotation);

                if (elapsedTime <= 0.0f || elapsedTime >= moveTime)
                {
                    //往路・復路で目標位置を超えたら
                    isReturn = (isReturn || isInfinity) ? false : true;
                    isInterval = true;
                    elapsedTime = 0.0f;

                    if (!isInfinity || intervalTime > 0.0f)
                    {
                        //往復運動、またはインターバル時間が設定されている場合：移動完了音を鳴らす
                        AudioManager.SEData seData = audioManager.objectToAndFromSE;
                        audioSource.volume = seData.volume;
                        audioSource.pitch = seData.pitch;
                        if (seData.clip != null) audioSource.PlayOneShot(seData.clip);
                    }
                }
            }
        }
    }
}
