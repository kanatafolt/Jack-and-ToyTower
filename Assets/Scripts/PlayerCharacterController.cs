//
//プレイヤーキャラクターの移動やジャンプ入力を扱うスクリプト
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCharacterController : MonoBehaviour
{
    const float MAX_JUMP_CHARGE = 0.6f;     //ジャンプの最大溜め時間及びジャンプ力係数
    const float MIN_SPRING_SCALE = 0.1f;    //ばねの最小縮み長さ
    const float MOVE_FREGQUENCY = 0.2f;     //移動発生周期

    [SerializeField] GameObject springObj, coverObj;
    private Rigidbody rb;

    private float initialSpringScale;       //ジャンプキーを押下した瞬間のバネの長さを一時保存する
    private float jumpCharge, moveCharge;
    private Vector3 moveDir;

    private void Reset()
    {
        springObj = GameObject.Find("SpringRig");
        coverObj = GameObject.Find("CoverRig");
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        moveDir = transform.forward;
    }

    void Update()
    {
        //移動周期を計算
        if (moveCharge < MOVE_FREGQUENCY) moveCharge += Time.deltaTime;
        
        if (Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
        {
            //移動キーを押している間：周期的に小ジャンプ移動
            if(moveCharge >= MOVE_FREGQUENCY)
            {
                moveCharge -= MOVE_FREGQUENCY;

                //一回の小ジャンプ移動が発生
                moveDir = (Vector3.forward * Input.GetAxis("Vertical") + Vector3.right * Input.GetAxis("Horizontal")).normalized;
                rb.velocity += moveDir * 2.0f;
                rb.velocity += transform.TransformDirection(Vector3.up * 1.0f);
                //springObj.transform.localScale = new Vector3(1.0f, springObj.transform.localScale.y - 0.1f, 1.0f);
                //coverObj.transform.Rotate(Vector3.right * 15.0f);
                springObj.GetComponent<SpringSimulation>().SetImpulse(-0.01f, 0.1f);
                coverObj.GetComponent<SpringSimulation>().SetImpulse(-1.0f, 0.1f);
            }
        }

        if ((Input.GetButtonUp("Vertical") && !Input.GetButton("Horizontal")) || (Input.GetButtonUp("Horizontal") && !Input.GetButton("Vertical")) || (Input.GetButtonUp("Vertical") && Input.GetButtonUp("Horizontal")))
        {
            //移動キーを完全に離したとき：キャラクター向きの自動回転を中断する
            moveDir = transform.forward;
        }

        if (Input.GetButtonDown("Jump"))
        {
            //ジャンプキーを押したとき：ばねの挙動を一時的に止める
            springObj.GetComponent<SpringSimulation>().enableSpring = false;
            initialSpringScale = springObj.transform.localScale.y;
        }

        if (Input.GetButton("Jump"))
        {
            //ジャンプキーを押している間：押す長さに応じて体を縮める(jumpChargeが溜まる)
            jumpCharge += Time.deltaTime;
            if (jumpCharge > MAX_JUMP_CHARGE) jumpCharge = MAX_JUMP_CHARGE;

            float shrinkLen = initialSpringScale - MIN_SPRING_SCALE;
            shrinkLen *= jumpCharge / MAX_JUMP_CHARGE;
            springObj.transform.localScale = new Vector3(1.0f, initialSpringScale - shrinkLen, 1.0f);
        }

        if (Input.GetButtonUp("Jump"))
        {
            //ジャンプキーを離したとき：jumpChargeの値に応じてジャンプする
            springObj.GetComponent<SpringSimulation>().enableSpring = true;
            rb.velocity += transform.TransformDirection(Vector3.up * (jumpCharge * 8.0f));
            jumpCharge = 0.0f;
        }
    }

    private void FixedUpdate()
    {
        //キャラクターが進行方向をゆっくりと向く
        if (Vector3.Angle(transform.forward, moveDir) >= 0.1f)
        {
            float rotDiff = Vector3.Angle(transform.forward, moveDir);
            if (rotDiff >= 5.0f) rotDiff = 5.0f;
            else rotDiff *= 0.1f;
            if (transform.InverseTransformDirection(moveDir).x < 0) rotDiff *= -1;
            transform.Rotate(Vector3.up * rotDiff);
        } else
        {
            if(moveDir != Vector3.zero) transform.forward = moveDir;
        }
    }
}
