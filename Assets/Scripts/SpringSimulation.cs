//
//スケール変化や位置・回転ででばねの挙動を表現するスクリプト
//SpringTransform.position(座標ばね)は未実装
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringSimulation : MonoBehaviour
{
    enum SpringAxis { x, y, z }                             //バネの挙動を取る軸
    [SerializeField] SpringAxis springAxis = SpringAxis.y;

    enum SpringTransform { position, rotation, scale }      //バネが影響するトランスフォーム要素
    [SerializeField] SpringTransform springTransform = SpringTransform.scale;

    [SerializeField] Transform coordinateParent;
    private Vector3 originPosition, originScale;
    private Vector3 originAngleX, originAngleY, originAngleZ;
    private float velocity = 0.0f;

    public bool enableSpring = true;
    [SerializeField] float accRate = 0.2f;
    [SerializeField] float velReduceRate = 0.9f;

    private float impulseTime, impulsePower;

    void Start()
    {
        if (!coordinateParent) coordinateParent = transform;

        originPosition = transform.localPosition;
        originScale = transform.localScale;
        originAngleX = coordinateParent.InverseTransformDirection(transform.right);
        originAngleY = coordinateParent.InverseTransformDirection(transform.up);
        originAngleZ = coordinateParent.InverseTransformDirection(transform.forward);
    }

    void FixedUpdate()
    {
        //インパルス処理
        if (impulseTime > 0.0f) impulseTime -= Time.deltaTime;
        else impulsePower = 0.0f;

        if (enableSpring)
        {
            //スケールばねの場合
            if (springTransform == SpringTransform.scale)
            {
                float diff = 0.0f;
                Vector3 expansion = Vector3.zero;

                if (springAxis == SpringAxis.x) diff = originScale.x - transform.localScale.x;
                if (springAxis == SpringAxis.y) diff = originScale.y - transform.localScale.y;
                if (springAxis == SpringAxis.z) diff = originScale.z - transform.localScale.z;

                float acc = diff * accRate;
                velocity += acc;
                velocity *= velReduceRate;

                velocity += impulsePower;

                if (springAxis == SpringAxis.x) expansion = Vector3.right * velocity;
                if (springAxis == SpringAxis.y) expansion = Vector3.up * velocity;
                if (springAxis == SpringAxis.z) expansion = Vector3.forward * velocity;

                transform.localScale += expansion;
            }

            //回転ばねの場合
            if (springTransform == SpringTransform.rotation)
            {
                float diff = 0.0f;
                Vector3 rot = Vector3.zero;

                if (springAxis == SpringAxis.x) diff = Vector3.Angle(coordinateParent.InverseTransformDirection(transform.up), originAngleY);
                if (springAxis == SpringAxis.y) diff = Vector3.Angle(coordinateParent.InverseTransformDirection(transform.forward), originAngleZ);
                if (springAxis == SpringAxis.z) diff = Vector3.Angle(coordinateParent.InverseTransformDirection(transform.right), originAngleX);

                float acc = diff * accRate;

                if (springAxis == SpringAxis.x && transform.InverseTransformDirection(coordinateParent.TransformDirection(originAngleY)).z < 0) acc *= -1;
                if (springAxis == SpringAxis.y && transform.InverseTransformDirection(coordinateParent.TransformDirection(originAngleZ)).x < 0) acc *= -1;
                if (springAxis == SpringAxis.z && transform.InverseTransformDirection(coordinateParent.TransformDirection(originAngleX)).y < 0) acc *= -1;

                velocity += acc;
                velocity *= velReduceRate;

                velocity += impulsePower;

                if (springAxis == SpringAxis.x) rot = Vector3.right * velocity;
                if (springAxis == SpringAxis.y) rot = Vector3.up * velocity;
                if (springAxis == SpringAxis.z) rot = Vector3.forward * velocity;

                transform.Rotate(rot);
            }
        }
    }

    //バネに撃力を与える。powはかかる力の大きさ、tはインパルス時間を設定
    public void SetImpulse (float pow, float t)
    {
        impulsePower = pow;
        impulseTime = t;
    }
}
