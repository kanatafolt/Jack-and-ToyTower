////
//GetScoreObject.cs
//得点オブジェクトのAbsorbTriggerに反応し、プレイヤーがオブジェクトを吸引、スコアを獲得する処理
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetScoreObject : MonoBehaviour
{
    const float ABSORB_TIME = 0.4f;
    const float SCALE_DOWN_TIME = 0.2f;

    private MainGameManager gameManager;
    private Transform playerCenter;
    [SerializeField] float score = 10.0f;
    private bool absorbing = false;
    private float elapsedTime = 0.0f;

    private AudioManager audioManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<MainGameManager>();
        playerCenter = GameObject.Find("PlayerCenter").transform;
        audioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
    }

    private void Update()
    {
        transform.rotation = Quaternion.AngleAxis(1.0f, transform.TransformDirection(Vector3.up)) * transform.rotation;

        if (absorbing)
        {
            //スコア獲得トリガーに触れたら：スコア獲得処理を転送、スコアオブジェクトをプレイヤーに吸引させ、消滅させる
            elapsedTime += Time.deltaTime;

            float posRate = (elapsedTime <= ABSORB_TIME) ? elapsedTime / ABSORB_TIME : 1.0f;

            transform.position += (playerCenter.position - transform.position) * posRate;

            float scaleRate = (elapsedTime <= SCALE_DOWN_TIME) ? elapsedTime / SCALE_DOWN_TIME : 1.0f;

            transform.localScale = Vector3.one * (1.0f - scaleRate);

            if ((playerCenter.position - transform.position).magnitude <= 0.1f)
            {
                gameManager.GetScore(score);

                //スコア獲得音
                AudioManager.SEData seData = audioManager.getStarSE;
                if (seData.clip != null) AudioSource.PlayClipAtPoint(seData.clip, transform.position, seData.volume);

                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        absorbing = true;
    }
}
