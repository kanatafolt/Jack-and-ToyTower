//
//スケール変化でばねの挙動を表現するスクリプト
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringSimulation : MonoBehaviour
{
    enum ScaleAxis { x, y, z }
    [SerializeField] ScaleAxis scaleAxis = ScaleAxis.y;

    private Vector3 origin;
    private float acceleration = 0.0f;
    private float velocity = 0.0f;

    public bool enableSpring = true;
    [SerializeField] float accRate = 0.2f;
    [SerializeField] float velReduceRate = 0.9f;

    void Start()
    {
        origin = transform.localScale;
    }

    void FixedUpdate()
    {
        if (enableSpring)
        {
            float diff = 0.0f;
            Vector3 expansion = Vector3.zero;

            if (scaleAxis == ScaleAxis.x) diff = origin.x - transform.localScale.x;
            if (scaleAxis == ScaleAxis.y) diff = origin.y - transform.localScale.y;
            if (scaleAxis == ScaleAxis.z) diff = origin.z - transform.localScale.z;

            acceleration = diff * accRate;
            velocity += acceleration;
            velocity *= velReduceRate;

            if (scaleAxis == ScaleAxis.x) expansion = Vector3.right * velocity;
            if (scaleAxis == ScaleAxis.y) expansion = Vector3.up * velocity;
            if (scaleAxis == ScaleAxis.z) expansion = Vector3.forward * velocity;

            transform.localScale += expansion;
        }
    }
}
