////
//SwitchManager.cs
//触れると起動するスイッチオブジェクトのON/OFFステートを管理、変化時の描画をするスクリプト
//カラーコードメモ：(ライトブルー)55879A→82C6E0、(グリーン)、4A8C4F→8BF392、(ピンク)AB5FAB→F69BF8
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
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
    private float elapsedTime;

    private AudioManager audioManager;
    private AudioSource audioSource;
    private DebugManager debugManager;

    private void Start()
    {
        ren = GetComponent<Renderer>();
        ren.material.EnableKeyword("_EMISSION");
        audioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        audioSource = gameObject.AddComponent<AudioSource>();
        initialEmission = Color.black;
        toEmission = ren.material.color;
        currentEmission = toEmission;

        if (GameObject.Find("DebugManager") != null) debugManager = GameObject.Find("DebugManager").GetComponent<DebugManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "player" && !isOn && elapsedTime / CHANGE_TIME <= 0.8f)
        {
            //スイッチにプレイヤーが触れたとき：スイッチをONにする
            SwitchOn(true);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.tag == "player" && !isOn && elapsedTime / CHANGE_TIME <= 0.5f)
        {
            //スイッチにプレイヤーが触れたとき：スイッチをONにする
            SwitchOn(true);
        }
    }

    private void Update()
    {
        if (debugManager != null) if (debugManager.suparForceOn && !isOn && elapsedTime / CHANGE_TIME <= 0.1f) SwitchOn(true);

        if (isOn)
        {
            if (timeLimit == 0.0f || elapsedTime < timeLimit)
            {
                //スイッチがONのとき、制限時間が設定されていないか、あるいは制限時間を超えていないとき：スイッチの変化処理を行う
                elapsedTime += Time.deltaTime;

                if (!switched)
                {
                    float changeRate = elapsedTime / CHANGE_TIME;

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
            else if (timeLimit > 0.0f && elapsedTime >= timeLimit)
            {
                //スイッチがONのとき、制限時間が設定されており、制限時間を超えていたら：スイッチをOFFにする
                elapsedTime = CHANGE_TIME;
                isOn = false;
                switched = true;
                currentPosition = transform.position;
                currentEmission = ren.material.GetColor("_EmissionColor");
            }
        }

        if (!isOn && switched)
        {
            //スイッチがOFFのとき、スイッチの逆変化が完了していないなら：スイッチの逆変化処理を行う
            elapsedTime -= Time.deltaTime;

            if (elapsedTime <= 0.0f)
            {
                elapsedTime = 0.0f;
                switched = false;
            }

            float changeRate = elapsedTime / CHANGE_TIME;

            transform.position = initialPosition + (currentPosition - initialPosition) * changeRate;
            ren.material.SetColor("_EmissionColor", new Color(initialEmission.r + (currentEmission.r - initialEmission.r) * changeRate,
                initialEmission.g + (currentEmission.g - initialEmission.g) * changeRate, initialEmission.b + (currentEmission.b - initialEmission.b) * changeRate));
        }
    }

    private void SwitchOn (bool soundOn)
    {
        //スイッチがONになる処理
        if (elapsedTime == 0.0f)
        {
            initialPosition = transform.position;
            toPosition = initialPosition + transform.TransformDirection(Vector3.down * moveDiff);
            currentPosition = initialPosition;
        }

        isOn = true;
        switched = false;
        currentPosition = transform.position;
        currentEmission = ren.material.GetColor("_EmissionColor");

        if (soundOn)
        {
            //スイッチがONになる音
            AudioManager.SEData seData = audioManager.switchOnSE;
            audioSource.volume = seData.volume;
            audioSource.pitch = seData.pitch;
            if (seData.clip != null) audioSource.PlayOneShot(seData.clip);
        }
    }

    public void ForceSwitching (bool switchOn)
    {
        if (switchOn)
        {
            //音無しでスイッチをONにする
            SwitchOn(false);
        }
        else
        {
            //スイッチをOFFにする
            elapsedTime = CHANGE_TIME;
            isOn = false;
            switched = true;
            currentPosition = transform.position;
            currentEmission = ren.material.GetColor("_EmissionColor");
        }
    }

    public void InitialPositionUpdate ()
    {
        if (!isOn)
        {
            //スイッチがOFFのとき：現在位置をinitialPositionとして、各変数を再度設定する
            initialPosition = transform.position;
            toPosition = initialPosition + transform.TransformDirection(Vector3.down * moveDiff);
            currentPosition = initialPosition;
        }
        else
        {
            //スイッチがONのとき：現在位置をtoPositionとして、各変数を再度設定する
            toPosition = transform.position;
            initialPosition = toPosition + transform.TransformDirection(Vector3.up * moveDiff);
            currentPosition = toPosition;
        }
    }
}
