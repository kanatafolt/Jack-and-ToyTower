////
//Event_TowerTopCameraPan.cs
//タワーの頂上に達したとき、カメラをパンするスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_TowerTopCameraPan : MonoBehaviour
{
    [SerializeField] float panTime = 1.5f;
    [SerializeField] float panGoalHeight = 22.0f;
    private Transform cameraRigObj;

    private bool panStart = false;
    private bool panFinished = false;
    private float elapsedTime = 0.0f;
    private Vector3 initialPosition, toPosition;
    private float onTriggerCount = 0.0f;

    private void Start()
    {
        cameraRigObj = GameObject.Find("CameraRig").transform;
        toPosition = Vector3.up * panGoalHeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "player")
        {
            panStart = true;
            onTriggerCount += 1.0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "player")
        {
            onTriggerCount -= 1.0f;

            if (onTriggerCount <= 0.0f)
            {
                //全てのプレイヤーオブジェクトがトリガーの外に出たらカメラを元に戻す
                cameraRigObj.gameObject.GetComponent<CameraRigRotater>().cameraHeightFixed = false;
                panStart = false;
                panFinished = false;
                elapsedTime = 0.0f;
            }
        }
    }

    private void FixedUpdate()
    {
        if (panStart && !panFinished)
        {
            if (elapsedTime == 0.0f)
            {
                //カメラリグの高さ自動補正を制限し、現在座標を記録する
                cameraRigObj.gameObject.GetComponent<CameraRigRotater>().cameraHeightFixed = true;
                initialPosition = cameraRigObj.position;
            }

            elapsedTime += Time.deltaTime;

            float moveRate = (elapsedTime <= panTime) ? elapsedTime / panTime : 1.0f;

            //カメラを動かす
            cameraRigObj.position = initialPosition + (toPosition - initialPosition) * moveRate;

            if (elapsedTime >= panTime)
            {
                //処理を終了
                panFinished = true;
            }
        }
    }
}
