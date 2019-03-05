////
//CenterOfMass.cs
//オブジェクトの回転中心をcenter座標へ変更するスクリプト
//回転中心を表す空のゲームオブジェクトを親にして、このスクリプトをつけてもよい(centerは0,0,0にする)
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CenterOfMass : MonoBehaviour
{
    [SerializeField] private Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = center;
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + transform.rotation * center);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * center, 0.05f);
    }
}
