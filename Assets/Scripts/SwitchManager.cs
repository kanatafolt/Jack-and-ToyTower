////
//SwitchManager.cs
//触れると起動するスイッチオブジェクトのON/OFFステートを管理、変化時の描画をするスクリプト
//カラーコードメモ：(ライトブルー)55879A→82C6E0、(グリーン)、4A8C4F→8BF392、(ピンク)AB5FAB→F69BF8
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SwitchManager : MonoBehaviour
{
    const float CHANGE_TIME = 0.1f;                     //スイッチの変化にかかる時間

    [HideInInspector] public bool isOn = false;
    [SerializeField] float timeLimit = 0.0f;            //スイッチがOFFになるまでの制限時間　0ならOFFにならない
    [SerializeField] float moveDiff = 0.1f;             //スイッチの沈む幅　0にすることでマテリアルだけが変化するスイッチも作成可能
    private bool switched = false;                      //スイッチオブジェクトの変化が完了したかどうか(isOnとは異なる)
    private Renderer ren;
    private Vector3 initialPosition, toPosition, currentPosition;
    private Color initialEmission, toEmission, currentEmission;
    private float timeElapsed;

    private AudioManager audioManager;
    private AudioSource audioSource;

    private void Start()
    {
        ren = GetComponent<Renderer>();
        GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        audioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        audioSource = GetComponent<AudioSource>();
        initialPosition = transform.position;
        toPosition = initialPosition + transform.TransformDirection(Vector3.down * moveDiff);
        currentPosition = initialPosition;
        initialEmission = Color.black;
        toEmission = GetComponent<Renderer>().material.color;
        currentEmission = toEmission;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "player" && !isOn && timeElapsed / CHANGE_TIME <= 0.8f)
        {
            //スイッチにプレイヤーが触れたとき：スイッチをONにする
            isOn = true;
            switched = false;
            currentPosition = transform.position;
            currentEmission = GetComponent<Renderer>().material.GetColor("_EmissionColor");

            //スイッチがONになる音
            AudioManager.SEData seData = audioManager.switchOnSE;
            audioSource.volume = seData.volume;
            audioSource.pitch = seData.pitch;
            if (seData.clip != null) audioSource.PlayOneShot(seData.clip);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.tag == "player" && !isOn && timeElapsed / CHANGE_TIME <= 0.5f)
        {
            //スイッチにプレイヤーが触れたとき：スイッチをONにする
            isOn = true;
            switched = false;
            currentPosition = transform.position;
            currentEmission = GetComponent<Renderer>().material.GetColor("_EmissionColor");

            //スイッチがONになる音
            AudioManager.SEData seData = audioManager.switchOnSE;
            audioSource.volume = seData.volume;
            audioSource.pitch = seData.pitch;
            if (seData.clip != null) audioSource.PlayOneShot(seData.clip);
        }
    }

    private void Update()
    {

        if (isOn)
        {
            if (timeLimit == 0.0f || timeElapsed < timeLimit)
            {
                //スイッチがONのとき、制限時間が設定されていないか、あるいは制限時間を超えていないとき：スイッチの変化処理を行う
                timeElapsed += Time.deltaTime;

                if (!switched)
                {
                    float changeRate = timeElapsed / CHANGE_TIME;

                    if (changeRate > 1.0f)
                    {
                        changeRate = 1.0f;
                        switched = true;
                    }

                    transform.position = currentPosition + (toPosition - currentPosition) * changeRate;
                    ren.material.SetColor("_EmissionColor", new Color(currentEmission.r + (toEmission.r - currentEmission.r) * changeRate,
                        currentEmission.g + (toEmission.g - currentEmission.g) * changeRate, currentEmission.b + (toEmission.b - currentEmission.b) * changeRate));
                }

            }
            else if (timeLimit > 0.0f && timeElapsed >= timeLimit)
            {
                //スイッチがONのとき、制限時間が設定されており、制限時間を超えていたら：スイッチをOFFにする
                timeElapsed = CHANGE_TIME;
                isOn = false;
                switched = true;
                currentPosition = transform.position;
                currentEmission = GetComponent<Renderer>().material.GetColor("_EmissionColor");
            }
        }

        if (!isOn && timeLimit > 0.0f && switched)
        {
            //スイッチがOFFのとき、制限時間が設定されており、スイッチの逆変化が完了していないとき：スイッチの逆変化処理を行う
            timeElapsed -= Time.deltaTime;

            if (timeElapsed <= 0.0f)
            {
                timeElapsed = 0.0f;
                switched = false;
            }

            float changeRate = timeElapsed / CHANGE_TIME;

            transform.position = initialPosition + (currentPosition - initialPosition) * changeRate;
            ren.material.SetColor("_EmissionColor", new Color(initialEmission.r + (currentEmission.r - initialEmission.r) * changeRate,
                initialEmission.g + (currentEmission.g - initialEmission.g) * changeRate, initialEmission.b + (currentEmission.b - initialEmission.b) * changeRate));
        }
    }
}
