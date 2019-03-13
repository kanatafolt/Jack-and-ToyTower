////
//PlayerShadowRaycast.cs
//レイキャストを用い、プレイヤーの影を描写するプロジェクターオブジェクトを常に地面から一定の高さに追随させるスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShadowRaycast : MonoBehaviour
{
    private Ray ray;
    private RaycastHit hit;
    [SerializeField] LayerMask mask = -1;
    [SerializeField] float maxDistance = 5.0f;
    private Transform shadowProjector;
    private Transform player;

    private void Start()
    {
        shadowProjector = GameObject.Find("PlayerShadowProjector").transform;
        player = GameObject.Find("Jack").transform;
    }

    private void Update()
    {
        transform.position = player.position + Vector3.up * 0.01f;

        //レイを作成
        ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));

        //レイキャストを行う
        if (Physics.Raycast(ray, out hit, maxDistance, mask))
        {
            //デバッグ用：レイを可視化する
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.cyan);

            shadowProjector.position = hit.point + Vector3.up * 1.0f;
        }
        else
        {
            shadowProjector.position = transform.position + Vector3.down * maxDistance + Vector3.up * 1.0f;
        }
    }
}
