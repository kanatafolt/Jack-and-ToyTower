////
//ObjectMovement.cs
//オブジェクトの自律的な移動・回転を制御するスクリプト
//Translation：一定速度で並進する
//Rotation：一定速度で回転する
//Circular Orbit：円軌道を描いて移動する(未実装)
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectMovement : MonoBehaviour
{
    private Rigidbody rb;

    public float lifeTime = 0.0f;           //lifeTime = 0.0fで無限
    private float elapsedTime = 0.0f;

    public Vector3 translationVelocity;

    public Vector3 rotationPivot;
    public float rotationVelocity;

    //public Vector3 circularOrbitPivot;
    //public float circularOrbitRadius, circularOrbitVelocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void Update()
    {
        //生存時間処理
        if (lifeTime > 0.0f)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= lifeTime) Destroy(gameObject);
        }

        //並進処理
        rb.MovePosition(transform.position + translationVelocity * Time.deltaTime);

        //回転処理
        rb.MoveRotation(Quaternion.AngleAxis(rotationVelocity * Time.deltaTime, rotationPivot) * transform.rotation);
    }
}
