////
//SafetyNet.cs
//セクションのグラウンドに敷き、落下したプレイヤーをグラウンドの上まで送り返すスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafetyNet : MonoBehaviour
{
    private Transform player;
    private Rigidbody playerRb;
    private PlayerCharacterController playerController;
    private PlayerCollisionSwitcher colSwitcher;
    private Transform playerTracer;

    private bool sending = false;
    private float elapsedTime = 0.0f;
    private float fallenDistance = 9.0f;            //落下したと判定されるタワーとの距離(グラウンドより外側かどうかを判定)
    private float goToDistance = 7.5f;              //送り返す地点のタワーとの距離(グラウンドの上)
    private float jumpVelocity = 4.5f;              //跳ねる力の強さ
    private float sendFinishTime = 0.7f;            //送り届ける時間
    private Vector3 initialPosition, toPosition;

    private void Start()
    {
        player = GameObject.Find("Jack").transform;
        playerRb = player.GetComponent<Rigidbody>();
        playerController = player.GetComponent<PlayerCharacterController>();
        colSwitcher = player.GetComponent<PlayerCollisionSwitcher>();
        playerTracer = GameObject.Find("PlayerLookAtTracer").transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "player" && !sending && Vector3.Distance(playerRb.position, playerTracer.position) >= fallenDistance)
        {
            sending = true;
            playerController.enableInput = false;
            colSwitcher.SetCollisionOff();
            playerRb.velocity = Vector3.up * jumpVelocity;
            initialPosition = playerRb.position;
            toPosition = initialPosition + playerTracer.TransformDirection(Vector3.back * (Vector3.Distance(playerRb.position, playerTracer.position) - goToDistance));
        }
    }

    private void Update()
    {
        //プレイヤーを送り返す処理
        if (sending)
        {
            elapsedTime += Time.deltaTime;

            float timeRate = (elapsedTime < sendFinishTime) ? elapsedTime / sendFinishTime : 1.0f;

            playerRb.position = new Vector3(initialPosition.x + (toPosition.x - initialPosition.x) * timeRate, playerRb.position.y, initialPosition.z + (toPosition.z - initialPosition.z) * timeRate);

            if (elapsedTime >= sendFinishTime)
            {
                //送り届け終了
                sending = false;
                colSwitcher.SetCollisionOn();
                playerController.enableInput = true;
                elapsedTime = 0.0f;
            }
        }
    }
}
