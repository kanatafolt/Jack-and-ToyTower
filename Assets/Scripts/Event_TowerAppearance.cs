////
//Event_TowerAppearance.cs
//タワー出現時のプレイヤーの挙動を制御するイベントシーンスクリプト
////

#pragma warning disable 0649    //変数が初期化されていないという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_TowerAppearance : MonoBehaviour
{
    [SerializeField] SwitchManager switchObj;
    [SerializeField] float finishTime = 3.6f;
    private GameObject player;
    private Transform cameraRig;
    private Transform tutorialObj;
    private Transform playerDestination;
    private PlayerCollisionSwitcher colSwitcher;
    private Vector3 tutorialObjPos;
    private float elapsedTime, elapsedTime_FromStart;

    private void Reset()
    {
        switchObj = transform.Find("Switch").gameObject.GetComponent<SwitchManager>();
    }

    private void Start()
    {
        player = GameObject.Find("Jack");
        cameraRig = GameObject.Find("CameraRig").transform;
        tutorialObj = GameObject.Find("Tutorial_Push").transform;
        playerDestination = transform.Find("PlayerSpot");
        colSwitcher = player.GetComponent<PlayerCollisionSwitcher>();
    }

    private void Update()
    {
        if (!switchObj.isOn)
        {
            //タワー起動スイッチがONになるまでの間：PUSH!オブジェクトをふわふわさせる処理
            if (elapsedTime_FromStart == 0.0f) tutorialObjPos = tutorialObj.position;
            elapsedTime_FromStart += Time.deltaTime;
            tutorialObj.position = tutorialObjPos + Vector3.up * Mathf.Sin(elapsedTime_FromStart * 2.0f) * 0.05f;
        }

        if (switchObj.isOn && elapsedTime < finishTime)
        {
            //タワー起動スイッチがONになったら：プレイヤーの操作を禁止して所定の位置に移動させつつ、カメラ向きをリセットするイベント演出を行う
            if (elapsedTime == 0.0f)
            {
                //起動時、プレイヤーをジャンプさせる
                player.GetComponent<PlayerCharacterController>().enableInput = false;
                player.GetComponent<Rigidbody>().velocity = Vector3.up * 6.0f;
            }

            elapsedTime += Time.deltaTime;

            //プレイヤーのx, z座標を所定の位置に近づけ、カメラを回転させる
            player.transform.position += new Vector3(playerDestination.position.x - player.transform.position.x, 0.0f, playerDestination.position.z - player.transform.position.z) * Time.deltaTime * 2.0f;
            if (cameraRig.forward.x < 0.0f) cameraRig.rotation = Quaternion.AngleAxis(Vector3.Angle(cameraRig.forward, transform.forward) * Time.deltaTime, Vector3.up) * cameraRig.rotation;
            if (cameraRig.forward.x > 0.0f) cameraRig.rotation = Quaternion.AngleAxis(-Vector3.Angle(cameraRig.forward, transform.forward) * Time.deltaTime, Vector3.up) * cameraRig.rotation;

            if (elapsedTime >= finishTime)
            {
                //イベント演出終了
                elapsedTime = finishTime;
                GameObject.Find("ToyTower").transform.position = new Vector3(0.0f, 6.0f, 0.0f);
                colSwitcher.SetCollisionOn();
                player.GetComponent<PlayerCharacterController>().enableInput = true;
                GameObject.Find("GameManager").GetComponent<MainGameManager>().towerAppearanced = true;
            }
        }
    }
}
