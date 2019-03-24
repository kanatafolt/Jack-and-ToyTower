////
//CameraRigRotater.cs
//プレイヤーキャラクターの移動に合わせて背景とカメラを上下に移動、左右に回転させるスクリプト
//視点移動ができないOculus Go向けに、背景込みで視点を動かすことで「タワーが回転している」ように見せる
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRigRotater : MonoBehaviour
{
    const float TOWER_ROTATE_RATE = 0.2f;       //タワーの回転速度(1で一瞬)
    const float TOWER_ELEVATE_RATE = 0.2f;      //タワーの上昇下降速度(1で一瞬)
    const float CAMERA_BOTTOM_HEIGHT = 0.0f;    //カメラの最下点
    const float CAMERA_TOP_HEIGHT = 30.0f;      //カメラの最上点

    [SerializeField] Transform player, tracer;
    [SerializeField] Transform leftBorder, rightBorder, upperBorder, lowerBorder;
    [SerializeField] InBorderArea leftBorderArea, rightBorderArea;
    private float leftAngleDiff, rightAngleDiff, upperHeightDiff, lowerHeightDiff;

    [HideInInspector] public bool cameraHeightFixed = false;

    private void Reset()
    {
        player = GameObject.Find("Jack").GetComponent<Transform>();
        tracer = GameObject.Find("PlayerLookAtTracer").GetComponent<Transform>();
        leftBorder = GameObject.Find("LeftBorder").GetComponent<Transform>();
        rightBorder = GameObject.Find("RightBorder").GetComponent<Transform>();
        upperBorder = GameObject.Find("UpperBorder").GetComponent<Transform>();
        lowerBorder = GameObject.Find("LowerBorder").GetComponent<Transform>();
        leftBorderArea = GameObject.Find("LeftBorderArea").GetComponent<InBorderArea>();
        rightBorderArea = GameObject.Find("RightBorderArea").GetComponent<InBorderArea>();
    }

    private void Update()
    {
        //playerがborderからどれくらい離れているかを計算
        leftAngleDiff = Vector3.Angle(leftBorder.forward, tracer.forward);
        rightAngleDiff = Vector3.Angle(rightBorder.forward, tracer.forward);
        if (leftBorderArea.inArea) leftAngleDiff *= -1.0f;
        if (rightBorderArea.inArea) rightAngleDiff *= -1.0f;
        upperHeightDiff = tracer.position.y - upperBorder.position.y;
        lowerHeightDiff = tracer.position.y - lowerBorder.position.y;

        //LeftBorderを越えている(負のとき)：CameraRigを正転させる(時計回り)
        if (-leftAngleDiff > 0.0f)
        {
            transform.Rotate(Vector3.up * -leftAngleDiff * TOWER_ROTATE_RATE);
        }

        //RightBorderを越えている(正のとき)：CameraRigを逆転させる(反時計回り)
        if (rightAngleDiff < 0.0f)
        {
            transform.Rotate(Vector3.up * rightAngleDiff * TOWER_ROTATE_RATE);
        }

        if (!cameraHeightFixed)
        {
            //UpperBorderを越えている(負のとき)：CameraRigを上昇させる
            if (upperHeightDiff > 0.0f)
            {
                if ((Vector3.up * upperHeightDiff * TOWER_ELEVATE_RATE + transform.position).y < CAMERA_TOP_HEIGHT)
                {
                    transform.position += Vector3.up * upperHeightDiff * TOWER_ELEVATE_RATE;
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, CAMERA_TOP_HEIGHT, transform.position.z);
                }
            }

            //LowerBorderを越えている(正のとき)：CameraRigを下降させる
            if (lowerHeightDiff < 0.0f)
            {
                if ((Vector3.up * lowerHeightDiff * TOWER_ELEVATE_RATE + transform.position).y > CAMERA_BOTTOM_HEIGHT)
                {
                    transform.position += Vector3.up * lowerHeightDiff * TOWER_ELEVATE_RATE;
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, CAMERA_BOTTOM_HEIGHT, transform.position.z);
                }
            }
        }

        //Debug.Log("player:" + tracer.forward + ", " + tracer.position.y + " leftAngle:" + leftAngleDiff + " rightAngle:" + rightAngleDiff + " upperHeight:" + upperHeightDiff + " lowerHeight:" + lowerHeightDiff);
    }
}
