////
//ObjectToAndFrom.cs
//一定時間周期でステージオブジェクトを往復運動させるスクリプト
//左右方向には回転、上下・前後方向には並進を行う
//なお、左右回転と前後並進を併用はできない(上下は可能)
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ObjectToAndFrom : MonoBehaviour
{
    private bool isOn = true;           //動くかどうか
    private bool isStarted = false;     //起動後、初回処理を行ったかどうか
    private bool isReturn = false;      //復路かどうか
    private bool isInterval = false;    //インターバルかどうか

    [SerializeField] float leftAndRightRotateAngle = 0.0f;
    [SerializeField] float upAndDownMoveDistance = 0.0f;
    [SerializeField] float forwardAndBackMoveDistance = 0.0f;
    [SerializeField] float moveTime = 1.0f;
    [SerializeField] float intervalTime = 1.5f;

    private float elapsedTime;
    private Vector3 initialPosition, toPosition;
    private Quaternion initialRotation;

    private AudioManager audioManager;
    private AudioSource audioSource;

    private void Start()
    {
        audioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isOn)
        {
            if (!isStarted)
            {
                //起動後、初回のみ行う処理
                initialPosition = transform.position;
                initialRotation = transform.rotation;
                toPosition = initialPosition + transform.TransformDirection(Vector3.up * upAndDownMoveDistance + Vector3.forward * forwardAndBackMoveDistance);
                isStarted = true;
            }

            if (!isInterval)
            {
                //移動中の処理
                if (!isReturn) elapsedTime += Time.deltaTime;
                if (isReturn) elapsedTime -= Time.deltaTime;

                float timeRate = elapsedTime / moveTime;
                if (timeRate <= 0.0f) timeRate = 0.0f;
                if (timeRate >= 1.0f) timeRate = 1.0f;

                //移動・回転処理
                transform.position = initialPosition + (toPosition - initialPosition) * timeRate;
                transform.rotation = Quaternion.AngleAxis(leftAndRightRotateAngle * timeRate, Vector3.up) * initialRotation;

                if (elapsedTime <= 0.0f || elapsedTime >= moveTime)
                {
                    //往路・復路で目標位置を超えたら
                    isReturn = (isReturn) ? false : true;
                    isInterval = true;
                    elapsedTime = 0.0f;

                    //移動完了音を鳴らす
                    AudioManager.SEData seData = audioManager.sequenceFinishSE;
                    audioSource.volume = seData.volume;
                    audioSource.pitch = seData.pitch;
                    if (seData.clip != null) audioSource.PlayOneShot(seData.clip);
                }
            }
            else
            {
                //インターバル中の処理
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= intervalTime)
                {
                    isInterval = false;
                    elapsedTime = (isReturn) ? moveTime : 0.0f;
                }
            }
        }
    }
}
