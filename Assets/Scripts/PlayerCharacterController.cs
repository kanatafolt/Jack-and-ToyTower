//
//プレイヤーキャラクターの移動やジャンプ入力を扱うスクリプト
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCharacterController : MonoBehaviour
{
    const float MAX_JUMP_POWER = 0.6f;
    const float MIN_SPRING_SCALE = 0.1f;

    [SerializeField] GameObject springObj;
    private Rigidbody rb;

    private float jumpPower = 0.0f;
    private float initialSpringScale = 1.0f;
    [SerializeField] float moveFrequency = 0.1f;
    private float moveCycle = 0.0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
        {
            moveCycle += Time.deltaTime;
            if(moveCycle >= moveFrequency)
            {

                moveCycle -= moveFrequency;
            }
        }

        if (Input.GetButtonUp("Vertical") || Input.GetButtonUp("Horizontal"))
        {
            moveCycle = 0.0f;
        }

        

        if (Input.GetAxis("Vertical") > 0)
        {
            //前へ移動
        }

        if (Input.GetAxis("Vertical") < 0)
        {
            //後ろへ移動
        }

        if (Input.GetAxis("Horizontal") > 0)
        {
            //右へ移動
        }

        if (Input.GetAxis("Horizontal") < 0)
        {
            //左へ移動
        }

        if (Input.GetButtonDown("Jump"))
        {
            springObj.GetComponent<SpringSimulation>().enableSpring = false;
            initialSpringScale = springObj.transform.localScale.y;
        }

        if (Input.GetButton("Jump"))
        {
            //体を縮める
            jumpPower += Time.deltaTime;
            if (jumpPower > MAX_JUMP_POWER) jumpPower = MAX_JUMP_POWER;

            float shrinkLen = initialSpringScale - MIN_SPRING_SCALE;
            shrinkLen *= jumpPower / MAX_JUMP_POWER;
            springObj.transform.localScale = new Vector3(1.0f, initialSpringScale - shrinkLen, 1.0f);

        }

        if (Input.GetButtonUp("Jump"))
        {
            //ジャンプ
            springObj.GetComponent<SpringSimulation>().enableSpring = true;
            rb.velocity += transform.TransformDirection(Vector3.up * (jumpPower * 8.0f));
            jumpPower = 0.0f;
        }
    }
}
