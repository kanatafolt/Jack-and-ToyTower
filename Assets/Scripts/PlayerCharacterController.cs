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
    const float COVER_CLOSE_TIMING = 0.7f;  //jumpChargeが何割を超えたらカバーを閉め始めるか(0～1)

    [SerializeField] GameObject springObj, coverObj;
    private Rigidbody rb;

    private float initialSpringScale;       //ジャンプキーを押下した瞬間のバネの長さを一時保存する
    private bool stopCoverAngle = false;
    private float jumpCharge, moveCharge;
    private Vector3 moveDir;
    private float coverCloseRate;
    private float onePrevHeight, twoPrevHeight;

    private bool enableJump = true;

    private void Reset()
    {
        springObj = GameObject.Find("SpringRig");
        coverObj = GameObject.Find("CoverRig");
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        moveDir = transform.forward;
        onePrevHeight = twoPrevHeight = transform.position.y;
    }

    void Update()
    {
        //移動周期を計算
        if (moveCharge < MOVE_FREGQUENCY) moveCharge += Time.deltaTime;

        if (transform.position.y == twoPrevHeight) enableJump = true;
        twoPrevHeight = onePrevHeight;
        onePrevHeight = transform.position.y;

        if (Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
        {
            //移動キーを押している間：周期的に小ジャンプ移動
            if(moveCharge >= MOVE_FREGQUENCY)
            {
                moveCharge -= MOVE_FREGQUENCY;

                //一回の小ジャンプ移動が発生
                moveDir = (Vector3.forward * Input.GetAxis("Vertical") + Vector3.right * Input.GetAxis("Horizontal")).normalized;
                rb.velocity = new Vector3 (moveDir.x * 2.0f, rb.velocity.y, moveDir.z * 2.0f);
                if (enableJump) rb.velocity += transform.TransformDirection(Vector3.up * 1.0f);
                springObj.GetComponent<SpringSimulation>().SetImpulse(-0.01f, 0.1f);
                coverObj.GetComponent<SpringSimulation>().SetImpulse(-1.5f, 0.1f);
            }
        }

        if (Input.GetButtonUp("Vertical"))
        {
            //上下移動キーを離したとき：前後方向への移動を止める
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0.0f);
        }

        if (Input.GetButtonUp("Horizontal"))
        {
            //左右移動キーを離したとき：左右方向への移動を止める
            rb.velocity = new Vector3(0.0f, rb.velocity.y, rb.velocity.z);
        }

        if ((Input.GetButtonUp("Vertical") && !Input.GetButton("Horizontal")) || (Input.GetButtonUp("Horizontal") && !Input.GetButton("Vertical")) || (Input.GetButtonUp("Vertical") && Input.GetButtonUp("Horizontal")))
        {
            //移動キーを完全に離したとき：キャラクター向きの自動回転を中断する
            moveDir = transform.forward;
        }

        if (Input.GetButtonDown("Jump"))
        {
            //ジャンプキーを押したとき：ばねとカバーの挙動を一時的に止める
            springObj.GetComponent<SpringSimulation>().enableSpring = false;
            initialSpringScale = springObj.transform.localScale.y;
        }

        if (Input.GetButton("Jump"))
        {
            //ジャンプキーを押している間：押す時間に応じて体を縮める(jumpChargeが溜まる)
            jumpCharge += Time.deltaTime;
            if (jumpCharge > MAX_JUMP_CHARGE) jumpCharge = MAX_JUMP_CHARGE;

            float shrinkLen = initialSpringScale - MIN_SPRING_SCALE;
            shrinkLen *= jumpCharge / MAX_JUMP_CHARGE;
            springObj.transform.localScale = new Vector3(1.0f, initialSpringScale - shrinkLen, 1.0f);

            if (jumpCharge / MAX_JUMP_CHARGE >= COVER_CLOSE_TIMING)
            {
                //カバーを閉め始める
                if (!stopCoverAngle)
                {
                    stopCoverAngle = true;
                    coverObj.GetComponent<SpringSimulation>().enableSpring = false;
                    coverCloseRate = Vector3.Angle(coverObj.transform.forward, transform.forward) / (MAX_JUMP_CHARGE * (1.0f - COVER_CLOSE_TIMING));
                }

                if (Vector3.Angle(coverObj.transform.forward, transform.forward) > coverCloseRate * Time.deltaTime)
                {
                    coverObj.transform.Rotate(Vector3.right * coverCloseRate * Time.deltaTime);
                }
                else
                {
                    coverObj.transform.forward = transform.TransformDirection(Vector3.forward);
                }
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            //ジャンプキーを離したとき：jumpChargeの値に応じてジャンプする
            springObj.GetComponent<SpringSimulation>().enableSpring = true;
            coverObj.GetComponent<SpringSimulation>().enableSpring = true;
            coverObj.GetComponent<SpringSimulation>().SetImpulse(-15.0f, 0.1f);
            if (enableJump) rb.velocity = new Vector3 (rb.velocity.x, 0.0f, rb.velocity.z) + transform.TransformDirection(Vector3.up * (jumpCharge * 8.0f));
            jumpCharge = 0.0f;
            stopCoverAngle = false;
            enableJump = false;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "floor") enableJump = true;
    }
}
