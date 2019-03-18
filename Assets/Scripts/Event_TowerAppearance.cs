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
    private Transform playerDestination;
    private float elapsedTime;

    private void Reset()
    {
        switchObj = transform.Find("Switch").gameObject.GetComponent<SwitchManager>();
    }

    private void Start()
    {
        player = GameObject.Find("Jack");
        cameraRig = GameObject.Find("CameraRig").transform;
        playerDestination = transform.Find("Event_TowerAppearance_PlayerDestination");
    }

    private void Update()
    {
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

            transform.position += Vector3.down * Time.deltaTime;
            player.transform.position += new Vector3(playerDestination.position.x - player.transform.position.x, 0.0f, playerDestination.position.z - player.transform.position.z) * Time.deltaTime * 2.0f;
            cameraRig.rotation = Quaternion.AngleAxis(-Vector3.Angle(cameraRig.forward, transform.forward) * Time.deltaTime, Vector3.up) * cameraRig.rotation;

            if (elapsedTime >= finishTime)
            {
                elapsedTime = finishTime;

                for (int i = 0; i < transparentPlayerObjects.Length; i++) transparentPlayerObjects[i].layer = LayerMask.NameToLayer("Player");

                player.GetComponent<PlayerCharacterController>().enableInput = true;
            }
        }
    }
}
