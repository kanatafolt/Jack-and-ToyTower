﻿////
//PlayerCharacterController.cs
//プレイヤーキャラクターの移動やジャンプ入力を扱うスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCharacterController : MonoBehaviour
{
    const float MOVE_LENGTH = 3.0f;             //移動量倍率
    const float FORWARD_MOVE_DECREASE = 0.7f;   //タワー奥行き方向への移動量減衰倍率
    const float MAX_JUMP_CHARGE = 0.5f;         //ジャンプの最大溜め時間
    const float MIN_JUMP_HEIGHT = 3.75f;        //最低ジャンプ力倍率
    const float MAX_JUMP_HEIGHT = 6.0f;         //最大ジャンプ力倍率
    const float MIN_SPRING_SCALE = 0.1f;        //ばねの最小縮み長さ
    const float MOVE_FREGQUENCY = 0.2f;         //移動発生周期
    const float COVER_CLOSE_TIMING = 0.7f;      //jumpChargeが何割を超えたらカバーを閉め始めるか(0～1)
    const float FIXED_TOWER_DISTANCE = 7.0f;    //タワーに対する固定距離

    [SerializeField] enum MoveOption { cameraAngle,  polarCoordinates, oculusGoPolarCoordinates, oculusGoControllerAngleAndPolarCoordinates }   //各オプションの詳細説明は後述
    [SerializeField] MoveOption moveOption = MoveOption.polarCoordinates;

    [SerializeField] GameObject cameraRig, lookAtTracer, springObj, coverObj;
    [SerializeField] PlayerJumpTrigger upperTrigger, lowerTrigger;
    private Rigidbody rb;

    private Vector3 moveDir;                        //進行方向を表す単位ベクトル
    private bool enableJump = true;
    private float jumpCharge, moveCharge;
    private float initialSpringScale;               //ジャンプキーを押下した瞬間のバネの長さを一時保存する
    private bool stopCoverAngle = false;
    private float coverCloseRate;
    private float onePrevHeight, twoPrevHeight;
    private float distanceToTower;
    private bool allowForwardMove = true;           //前後移動入力を禁止する
    private bool forceStopX_Z_Velocity = false;     //物理演算による意図しない滑りを防止する(trueになったとき、一度だけvelTempのx,zを0にする)

    //デバッグ変数
    [SerializeField] Renderer ren;
    [SerializeField] Material contactingMat, flyingMat;

    private void Reset()
    {
        cameraRig = GameObject.Find("CameraRig");
        lookAtTracer = GameObject.Find("PlayerLookAtTracer");
        springObj = GameObject.Find("SpringRig");
        coverObj = GameObject.Find("CoverRig");
        upperTrigger = GameObject.Find("UpperJumpTrigger").GetComponent<PlayerJumpTrigger>();
        lowerTrigger = GameObject.Find("LowerJumpTrigger").GetComponent<PlayerJumpTrigger>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        moveDir = transform.forward;
        onePrevHeight = twoPrevHeight = transform.position.y;
        distanceToTower = Vector3.Distance(transform.position, lookAtTracer.transform.position);
    }

    private void Update()
    {
        //移動周期を計算
        if (moveCharge < MOVE_FREGQUENCY) moveCharge += Time.deltaTime;

        //ジャンプ禁止・許可処理
        if (upperTrigger.contacting && lowerTrigger.contacting) enableJump = true;          //ジャンプトリガーが二つとも接触していればジャンプを許可する
        if (!upperTrigger.contacting && !lowerTrigger.contacting) enableJump = false;       //ジャンプトリガーが二つとも接触していなければジャンプを禁止する
        if (transform.position.y == twoPrevHeight) enableJump = true;                       //2フレーム前からy座標が変化していないならジャンプを許可する
        twoPrevHeight = onePrevHeight;
        onePrevHeight = transform.position.y;

        //ren.material = (enableJump) ? contactingMat : flyingMat;        //デバッグ用

        //移動処理
        Vector3 velTemp = rb.velocity;
        float moveDirForwardAngle = 0.0f;       //moveOptionごとに、移動の奥行方向としたい方向への角度をラジアンで代入する

        switch (moveOption)
        {
            //カメラに対して右をx、奥行きをzとする直交座標移動
            case MoveOption.cameraAngle:
                moveDirForwardAngle = cameraRig.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
                break;

            //タワー接線方向をx、タワー中心方向をzとする極座標移動
            case MoveOption.polarCoordinates:
                moveDirForwardAngle = (lookAtTracer.transform.rotation.eulerAngles.y + 180.0f) * Mathf.Deg2Rad;
                break;

            //Oculus Goコントローラーの向きにかかわらず、タッチパッド右をタワー接線方向x、タッチパッド上をタワー中心方向zとする極座標移動(簡易対応想定、未実装)
            case MoveOption.oculusGoPolarCoordinates:
                //未実装
                moveDirForwardAngle = (lookAtTracer.transform.rotation.eulerAngles.y + 180.0f) * Mathf.Deg2Rad;
                break;

            //Oculus Goコントローラーの向きを反映し、カメラからタワーを見てタッチパッドの右側をタワー接線方向x、タワー側をタワー中心方向zとする極座標移動(最終版想定、未実装)
            case MoveOption.oculusGoControllerAngleAndPolarCoordinates:
                //未実装
                moveDirForwardAngle = (lookAtTracer.transform.rotation.eulerAngles.y + 180.0f) * Mathf.Deg2Rad;
                break;
        }

        if (Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
        {
            //移動キーを押している間：周期的に小ジャンプ移動
            if (moveCharge >= MOVE_FREGQUENCY)
            {
                moveCharge -= MOVE_FREGQUENCY;

                //一回の小ジャンプ移動が発生
                Vector3 moveDirTemp = Vector3.zero;
                if (Input.GetButton("Vertical")) moveDirTemp += new Vector3(Mathf.Sin(moveDirForwardAngle), 0.0f, Mathf.Cos(moveDirForwardAngle)) * Input.GetAxisRaw("Vertical");
                if (Input.GetButton("Horizontal")) moveDirTemp += new Vector3(Mathf.Cos(moveDirForwardAngle), 0.0f, -Mathf.Sin(moveDirForwardAngle)) * Input.GetAxisRaw("Horizontal");
                moveDir = moveDirTemp.normalized;
                velTemp = new Vector3(moveDir.x * MOVE_LENGTH, rb.velocity.y, moveDir.z * MOVE_LENGTH);                             //移動量倍率をかける
                velTemp = Quaternion.AngleAxis(lookAtTracer.transform.rotation.eulerAngles.y + 180.0f, Vector3.down) * velTemp;     //速度ベクトルをタワーフォワード座標系へ変換
                if (allowForwardMove) velTemp = new Vector3(velTemp.x, velTemp.y, velTemp.z * FORWARD_MOVE_DECREASE);               //タワー中心方向(z)に速度減衰をかける
                if (!allowForwardMove) velTemp = new Vector3(velTemp.x, velTemp.y, 0.0f);                                           //allowForwardMoveがfalseならタワー中心方向(z)を0に
                velTemp = Quaternion.AngleAxis(lookAtTracer.transform.rotation.eulerAngles.y + 180.0f, Vector3.up) * velTemp;       //速度ベクトルを逆変換
                if (enableJump) velTemp += transform.TransformDirection(Vector3.up * 1.0f);                                         //ジャンプ可能なら小ジャンプを行う
                springObj.GetComponent<SpringSimulation>().SetImpulse(-0.01f, 0.1f);
                coverObj.GetComponent<SpringSimulation>().SetImpulse(-1.5f, 0.1f);
            }
        }
        
        if (Input.GetButtonUp("Vertical"))
        {
            //上下キーを離したとき：タワー中心方向(z)の速度を0にする
            velTemp = Quaternion.AngleAxis(lookAtTracer.transform.rotation.eulerAngles.y + 180.0f, Vector3.down) * velTemp;     //速度ベクトルをタワーフォワード座標系へ変換
            velTemp = new Vector3(velTemp.x, velTemp.y, velTemp.z * 0.0f);                                                      //タワー中心方向(z)の速度をゼロにする
            velTemp = Quaternion.AngleAxis(lookAtTracer.transform.rotation.eulerAngles.y + 180.0f, Vector3.up) * velTemp;       //速度ベクトルを逆変換

            if (allowForwardMove) distanceToTower = Vector3.Distance(transform.position, lookAtTracer.transform.position);      //前後移動を止めたタイミングでタワーへの距離を保存する
        }

        if (Input.GetButtonUp("Horizontal"))
        {
            //左右キーを離したとき：タワー接線方向(x)の速度を0にする
            velTemp = Quaternion.AngleAxis(lookAtTracer.transform.rotation.eulerAngles.y + 180.0f, Vector3.down) * velTemp;     //速度ベクトルをタワーフォワード座標系へ変換
            velTemp = new Vector3(velTemp.x * 0.0f, velTemp.y, velTemp.z);                                                      //タワー接線方向(x)の速度をゼロにする
            velTemp = Quaternion.AngleAxis(lookAtTracer.transform.rotation.eulerAngles.y + 180.0f, Vector3.up) * velTemp;       //速度ベクトルを逆変換
        }

        if ((Input.GetButtonUp("Vertical") && !Input.GetButton("Horizontal")) || (Input.GetButtonUp("Horizontal") && !Input.GetButton("Vertical")) || (Input.GetButtonUp("Vertical") && Input.GetButtonUp("Horizontal")))
        {
            //移動キーを完全に離したとき：キャラクター向きの自動回転を中断する
            velTemp = new Vector3(0.0f, velTemp.y, 0.0f);
            moveDir = transform.forward;
        }

        if (Input.GetButtonDown("Horizontal"))
        {
            if (allowForwardMove) distanceToTower = Vector3.Distance(transform.position, lookAtTracer.transform.position);      //左右移動を始めたタイミングでタワーへの距離を保存する
        }

        if (Input.GetButton("Horizontal") && !Input.GetButton("Vertical"))
        {
            //左右キーを押していて、かつ上下キーを離している間：タワーとの距離を一定に保つ
            float distanceDiff = -distanceToTower + Vector3.Distance(transform.position, lookAtTracer.transform.position);

            velTemp = Quaternion.AngleAxis(lookAtTracer.transform.rotation.eulerAngles.y + 180.0f, Vector3.down) * velTemp;     //速度ベクトルをタワーフォワード座標系へ変換
            velTemp = new Vector3(velTemp.x, velTemp.y, velTemp.z + distanceDiff * 0.8f);                                       //タワー中心方向(z)へ補正をかける
            velTemp = Quaternion.AngleAxis(lookAtTracer.transform.rotation.eulerAngles.y + 180.0f, Vector3.up) * velTemp;       //速度ベクトルを逆変換
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

                //カバーを閉める
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
            //ジャンプキーを離したとき：jumpCharge溜め段階に応じてジャンプする
            springObj.GetComponent<SpringSimulation>().enableSpring = true;
            coverObj.GetComponent<SpringSimulation>().enableSpring = true;
            coverObj.GetComponent<SpringSimulation>().SetImpulse(-15.0f * jumpCharge / MAX_JUMP_CHARGE, 0.1f);

            if (enableJump)
            {
                //velTemp = new Vector3(velTemp.x, jumpCharge / MAX_JUMP_CHARGE * MAX_JUMP_HEIGHT, velTemp.z);                                          //案1：溜めに比例してジャンプ力が上がる
                //velTemp = new Vector3(velTemp.x, jumpCharge >= MAX_JUMP_CHARGE ? MAX_JUMP_HEIGHT : MIN_JUMP_HEIGHT, velTemp.z);                       //案2：最小or最大の2段階変化(三項演算子)
                velTemp = new Vector3(velTemp.x, MIN_JUMP_HEIGHT + jumpCharge / MAX_JUMP_CHARGE * (MAX_JUMP_HEIGHT - MIN_JUMP_HEIGHT), velTemp.z);      //案3：最小～最大まで溜めに比例する

            }

            jumpCharge = 0.0f;
            stopCoverAngle = false;
            enableJump = false;
        }

        //フロアオブジェクトに触れたときに移動入力を行っていなかった場合、一度だけ強制的にx,z方向への移動を0にする(物理演算による滑り防止)
        if (forceStopX_Z_Velocity)
        {
            velTemp = new Vector3(0.0f, velTemp.y, 0.0f);
            forceStopX_Z_Velocity = false;
        }

        //速度を確定
        rb.velocity = velTemp;
    }

    private void FixedUpdate()
    {
        //キャラクターが進行方向をゆっくりと向く
        float rotDiff = Vector3.Angle(transform.forward, moveDir);

        if (rotDiff >= 0.1f)
        {
            if (rotDiff >= 5.0f) rotDiff = 5.0f;
            else rotDiff *= 0.1f;
            if (transform.InverseTransformDirection(moveDir).x < 0) rotDiff *= -1;
            rb.MoveRotation(Quaternion.AngleAxis(rotDiff, Vector3.up) * transform.rotation);
        }
        else
        {
            if(moveDir != Vector3.zero) transform.forward = moveDir;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //フロアオブジェクトに触れた瞬間、移動入力を行っていなかった場合：滑り防止フラグをtrueにする
        if (collision.gameObject.tag == "floor" && !Input.GetButton("Vertical") && !Input.GetButton("Horizontal"))
        {
            forceStopX_Z_Velocity = true;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    //スタートトリガーに触れたとき：前後移動を禁止し、タワーとの距離を固定する(凍結中)
    //    if (other.tag == "fixedForwardMoveTrigger")
    //    {
    //        allowForwardMove = false;
    //        distanceToTower = FIXED_TOWER_DISTANCE;
    //    }

    //    //ボトムトリガーに触れたとき：前後移動を許可する(凍結中)
    //    if (other.tag == "allowForwardMoveTrigger")
    //    {
    //        allowForwardMove = true;
    //    }
    //}
}
