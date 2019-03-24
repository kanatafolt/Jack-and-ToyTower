////
//SafetyNet.cs
//セクションのグラウンドに敷き、落下したプレイヤーをグラウンドの上まで送り返すスクリプト
////

#pragma warning disable 0649    //変数が初期化されていないという警告を無視する

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

    [SerializeField] TriggerContactingOrNot[] maskTrigger = new TriggerContactingOrNot[1];          //セーフティネットを無効化する範囲を指定するトリガーコライダー

    [System.Serializable] [SerializeField] struct GoToMask
    {
        public TriggerContactingOrNot mask;
        public Transform goToPoint;
    }

    [SerializeField] GoToMask[] goToMaskTrigger = new GoToMask[1];                                  //落下時にプレイヤーの帰還位置を指定するトリガーコライダー

    private bool sending = false;
    private float elapsedTime = 0.0f;
    private float goToDistance = 7.5f;              //送り返す地点のタワーとの距離(グラウンドの上)
    private float groundHeight = 0.5f;              //セーフティーネットからグラウンドまでの高さ差分
    private float jumpHeight = 0.8f;                //跳ね返しの最大高さ
    private float sendFinishTime = 0.6f;            //送り届ける時間
    private Vector3 initialPosition, toPosition;

    private AudioManager audioManager;
    private AudioSource audioSource;

    private void Start()
    {
        player = GameObject.Find("Jack").transform;
        playerRb = player.GetComponent<Rigidbody>();
        playerController = player.GetComponent<PlayerCharacterController>();
        colSwitcher = player.GetComponent<PlayerCollisionSwitcher>();
        playerTracer = GameObject.Find("PlayerLookAtTracer").transform;
        audioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        bool inMask = false;
        for (int i = 0; i < maskTrigger.Length; i++) if (maskTrigger[i] != null) if (maskTrigger[i].contacting) inMask = true;

        if (other.tag == "player" && !sending && !inMask)
        {
            //セーフティネットに引っかかった場合：プレイヤーを送り返す
            sending = true;
            playerController.enableInput = false;
            playerRb.useGravity = false;
            colSwitcher.SetCollisionOff();
            playerRb.velocity = Vector3.zero;

            initialPosition = playerRb.position;
            toPosition = initialPosition + playerTracer.TransformDirection(Vector3.back * (Vector3.Distance(playerRb.position, playerTracer.position) - goToDistance));
            toPosition = new Vector3(toPosition.x, transform.position.y + groundHeight, toPosition.z);

            //プレイヤーがGoToマスクに被さっている場合は、対応するGoToポイントへ飛ぶ
            for (int i = 0; i < goToMaskTrigger.Length; i++)
                if (goToMaskTrigger[i].mask != null && goToMaskTrigger[i].goToPoint != null) if (goToMaskTrigger[i].mask.contacting) toPosition = goToMaskTrigger[i].goToPoint.position;

            //ネットに跳ね返される音
            AudioManager.SEData seData = audioManager.safetyNetSE;
            audioSource.volume = seData.volume;
            audioSource.pitch = seData.pitch;
            if (seData.clip != null) audioSource.PlayOneShot(seData.clip);
        }
    }

    private void Update()
    {
        //プレイヤーを送り返す処理
        if (sending)
        {
            elapsedTime += Time.deltaTime;

            float timeRate = (elapsedTime < sendFinishTime) ? elapsedTime / sendFinishTime : 1.0f;

            float jumpDiff = jumpHeight * Mathf.Sin(timeRate * Mathf.PI);
            playerRb.position = initialPosition + (toPosition - initialPosition) * timeRate + Vector3.up * jumpDiff;

            if (elapsedTime >= sendFinishTime)
            {
                //送り届け終了
                sending = false;
                colSwitcher.SetCollisionOn();
                playerRb.useGravity = true;
                playerController.enableInput = true;
                elapsedTime = 0.0f;
            }
        }
    }
}
