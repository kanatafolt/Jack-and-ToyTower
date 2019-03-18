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
    [SerializeField] GameObject[] transparentPlayerObjects;
    private GameObject player;
    private Transform cameraRig;
    private Transform tutorialObj;
    private Transform playerDestination;
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
    }

    private void Update()
    {
        if (elapsedTime_FromStart == 0.0f) tutorialObjPos = tutorialObj.position;

        elapsedTime_FromStart += Time.deltaTime;

        if (!switchObj.isOn) tutorialObj.position = tutorialObjPos + Vector3.up * Mathf.Sin(elapsedTime_FromStart * 2.0f) * 0.05f;

        if (switchObj.isOn && elapsedTime < finishTime)
        {
            if (elapsedTime == 0.0f)
            {
                //起動時、プレイヤーをジャンプさせる
                player.GetComponent<PlayerCharacterController>().enableInput = false;
                player.GetComponent<Rigidbody>().velocity = Vector3.up * 6.0f;

                for (int i = 0; i < transparentPlayerObjects.Length; i++) transparentPlayerObjects[i].layer = LayerMask.NameToLayer("TransparentPlayer");
            }

            elapsedTime += Time.deltaTime;

            player.transform.position += new Vector3(playerDestination.position.x - player.transform.position.x, 0.0f, playerDestination.position.z - player.transform.position.z) * Time.deltaTime * 2.0f;
            if (cameraRig.forward.x < 0.0f) cameraRig.rotation = Quaternion.AngleAxis(Vector3.Angle(cameraRig.forward, transform.forward) * Time.deltaTime, Vector3.up) * cameraRig.rotation;
            if (cameraRig.forward.x > 0.0f) cameraRig.rotation = Quaternion.AngleAxis(-Vector3.Angle(cameraRig.forward, transform.forward) * Time.deltaTime, Vector3.up) * cameraRig.rotation;

            if (elapsedTime >= finishTime)
            {
                elapsedTime = finishTime;

                for (int i = 0; i < transparentPlayerObjects.Length; i++) transparentPlayerObjects[i].layer = LayerMask.NameToLayer("Player");

                player.GetComponent<PlayerCharacterController>().enableInput = true;
            }
        }
    }
}
