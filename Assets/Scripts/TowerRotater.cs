////
//TowerRotater.cs
//プレイヤーキャラクターの移動に合わせてタワーを上下に移動、左右に回転させるスクリプト
//視点移動ができないOculus Go向けの仕組み
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TowerRotater : MonoBehaviour
{
    const float LEFT_BORDER_ANGLE = 8.0f;
    const float RIGHT_BORDER_ANGLE = -8.0f;
    const float UPPER_BORDER_HEIGHT = 2.0f;
    const float LOWER_BORDER_HEIGHT = 1.0f;
    const float TOWER_ROTATE_RATE = 1.0f;
    const float TOWER_ELEVATE_RATE = 1.0f;
    const float TOWER_BOTTOM_HEIGHT = 0.5f;
    const float TOWER_TOP_HEIGHT = 20.0f;

    [SerializeField] Transform player, tracer;
    private Rigidbody rb;

    private Vector3 leftBorderForward, rightBorderForward;
    private float leftAngleDiff, rightAngleDiff, upperHeightDiff, lowerHeightDiff;

    private void Reset()
    {
        player = GameObject.Find("Jack").GetComponent<Transform>();
        tracer = GameObject.Find("PlayerLookAtTracer").GetComponent<Transform>();
    }

    private void Start()
    {
        leftBorderForward = Quaternion.AngleAxis(LEFT_BORDER_ANGLE, Vector3.up) * Vector3.back;
        rightBorderForward = Quaternion.AngleAxis(RIGHT_BORDER_ANGLE, Vector3.up) * Vector3.back;

        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //playerがborderからどれくらい離れているかを計算
        leftAngleDiff = Vector3.Angle(leftBorderForward, tracer.forward);
        if (leftBorderForward.x - tracer.forward.x > 0.0f) leftAngleDiff *= -1.0f;
        rightAngleDiff = Vector3.Angle(rightBorderForward, tracer.forward);
        if (rightBorderForward.x - tracer.forward.x >= 0.0f) rightAngleDiff *= -1.0f;
        upperHeightDiff = UPPER_BORDER_HEIGHT - tracer.position.y;
        lowerHeightDiff = LOWER_BORDER_HEIGHT - tracer.position.y;

        //LeftBorderを越えている(負のとき)：TowerRigを逆転させる(反時計回り)
        if (leftAngleDiff < 0.0f)
        {
            rb.MoveRotation(Quaternion.AngleAxis(leftAngleDiff * TOWER_ROTATE_RATE, Vector3.up) * transform.rotation);
        }

        //RightBorderを越えている(正のとき)：TowerRigを正転させる(時計回り)
        if (rightAngleDiff > 0.0f)
        {
            rb.MoveRotation(Quaternion.AngleAxis(rightAngleDiff * TOWER_ROTATE_RATE, Vector3.up) * transform.rotation);
        }

        //UpperBorderを越えている(負のとき)：TowerRigを下降させる
        if (upperHeightDiff < 0.0f)
        {
            if ((Vector3.up * upperHeightDiff * TOWER_ELEVATE_RATE + transform.position).y >= -TOWER_TOP_HEIGHT)
            {
                rb.MovePosition(Vector3.up * upperHeightDiff * TOWER_ELEVATE_RATE + transform.position);
            }
            else
            {
                rb.MovePosition(new Vector3(transform.position.x, -TOWER_TOP_HEIGHT, transform.position.z));
            }
        }

        //LowerBorderを越えている(正のとき)：TowerRigを上昇させる
        if (lowerHeightDiff > 0.0f)
        {
            if ((Vector3.up * lowerHeightDiff * TOWER_ELEVATE_RATE + transform.position).y <= TOWER_BOTTOM_HEIGHT)
            {
                rb.MovePosition(Vector3.up * lowerHeightDiff * TOWER_ELEVATE_RATE + transform.position);
            }
            else
            {
                rb.MovePosition(new Vector3(transform.position.x, TOWER_BOTTOM_HEIGHT, transform.position.z));
            }
        }

        //Debug.Log("player:" + tracer.forward + ", " + tracer.position.y + " leftAngle:" + leftAngleDiff + " rightAngle:" + rightAngleDiff + " upperHeight:" + upperHeightDiff + " lowerHeight:" + lowerHeightDiff);
    }

    //private void PlayerFetch()
    //{
    //    //TowerRig系にプレイヤーキャラクターを取り込む
    //    player.SetParent(transform);
    //}

    //private void PlayerDeport()
    //{
    //    //TowerRig系からWorld系へプレイヤーキャラクターを出す
    //    player.SetParent(null);
    //}
}
