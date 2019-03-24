////
//ObjectToAndFromMulti.cs
//ObjectToAndFromの複数規則対応版
//ステージオブジェクトを複数の規則に従って回転・移動させるスクリプト。詳細はObjectToAndFrom参照
//(余裕があれば元スクリプトと統合したいが暫定対応)
////

#pragma warning disable 0649    //変数が初期化されていないという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectToAndFromMulti : MonoBehaviour
{
    private bool isOn = false;                              //falseのときオブジェクトの運動は停止する
    [SerializeField] SwitchManager relianceSwitch;          //スイッチへの依存：nullではない場合、SwitchのisOnによって起動する
    private Rigidbody rb;
    [HideInInspector] public bool pausing = false;          //trueのときオブジェクトの運動を一時停止させる

    [System.Serializable] [SerializeField] struct ToAndFromRule
    {
        public bool isInfinity;                             //片道無限運動設定：trueのとき、isReturnに関わらず常に往路方向へ進む
        public float leftAndRightRotateAngle;               //左右方向への回転量(world.up軸回転)
        public float upAndDownMoveDistance;                 //上下方向への移動量(local.up方向並進)
        public float forwardAndBackMoveDistance;            //前後方向への移動量(local.forward方向並進)
        public float tiltRotateAngle;                       //ロール軸での回転量(local.forward軸回転)
        public float pitchRotateAngle;                      //ピッチ軸での回転量(local.right軸回転)
        public float moveTime;                              //片道の移動にかかる時間
        public float intervalTime;                          //折り返すまでの待機時間
        public float delayTime;                             //初回起動までの遅延時間
        [HideInInspector] public bool isReturn;             //falseなら往路、trueなら復路
        [HideInInspector] public bool isInterval;           //インターバル(往路・復路間の待機時間)中かどうか
        [HideInInspector] public float elapsedTime;
    }

    [SerializeField] ToAndFromRule[] rules = new ToAndFromRule[1];

    private MainGameManager gameManager;
    private AudioManager audioManager;
    private AudioSource audioSource;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<MainGameManager>();
        audioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        audioSource = gameObject.AddComponent<AudioSource>();

        for (int i = 0; i < rules.Length; i++)
        {
            if (rules[i].delayTime > 0.0f)
            {
                //ディレイタイムが設定されている場合、初回のみ待機時間が発生する
                rules[i].isInterval = true;
                rules[i].elapsedTime = rules[i].intervalTime - rules[i].delayTime;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isOn)
        {
            //共通起動条件：タワーの出現が完了する
            if (gameManager.towerAppearanced)
            {
                if (relianceSwitch == null) isOn = true;            //スイッチへの依存がない場合：すぐに動作開始する
                else if(relianceSwitch.isOn) isOn = true;           //スイッチへの依存がある場合：スイッチがオンになったら動作開始する

                if (isOn)
                {
                    //初回起動処理：対象オブジェクトにRigidbodyを付与または取得する
                    rb = (GetComponent<Rigidbody>() != null) ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                }
            }
        }

        if (isOn && !pausing)
        {
            Vector3 posDiff = rb.position;
            Quaternion rotDiff = rb.rotation;

            for (int i = 0; i < rules.Length; i++)
            {
                //各ルールごとに移動処理を行う
                if (rules[i].isInterval)
                {
                    //インターバル中の処理
                    rules[i].elapsedTime += Time.deltaTime;

                    if (rules[i].elapsedTime >= rules[i].intervalTime)
                    {
                        rules[i].isInterval = false;
                        rules[i].elapsedTime = (rules[i].isReturn) ? rules[i].moveTime : 0.0f;
                    }
                }

                if (!rules[i].isInterval)
                {
                    //移動中の処理
                    float deltaMoveRate = 0.0f;
                    if (!rules[i].isReturn)
                    {
                        rules[i].elapsedTime += Time.deltaTime;
                        deltaMoveRate = (rules[i].elapsedTime <= rules[i].moveTime) ? Time.deltaTime : Time.deltaTime - (rules[i].elapsedTime - rules[i].moveTime);
                    }
                    else
                    {
                        rules[i].elapsedTime -= Time.deltaTime;
                        deltaMoveRate = (rules[i].elapsedTime >= 0.0f) ? -Time.deltaTime : -Time.deltaTime - rules[i].elapsedTime;
                    }
                    deltaMoveRate = deltaMoveRate / rules[i].moveTime;

                    //移動・回転処理を加算
                    posDiff = transform.TransformDirection(Vector3.up * rules[i].upAndDownMoveDistance + Vector3.forward * rules[i].forwardAndBackMoveDistance) * deltaMoveRate + posDiff;

                    rotDiff = Quaternion.AngleAxis(rules[i].pitchRotateAngle * deltaMoveRate, transform.right) * Quaternion.AngleAxis(rules[i].tiltRotateAngle * deltaMoveRate, transform.forward)
                        * Quaternion.AngleAxis(rules[i].leftAndRightRotateAngle * deltaMoveRate, Vector3.up) * rotDiff;

                    if (rules[i].elapsedTime <= 0.0f || rules[i].elapsedTime >= rules[i].moveTime)
                    {
                        //往路・復路で目標位置を超えたら
                        rules[i].isReturn = (rules[i].isReturn || rules[i].isInfinity) ? false : true;
                        rules[i].isInterval = true;
                        rules[i].elapsedTime = 0.0f;

                        if (!rules[i].isInfinity || rules[i].intervalTime > 0.0f)
                        {
                            //往復運動、またはインターバル時間が設定されている場合：移動完了音を鳴らす
                            AudioManager.SEData seData = audioManager.objectToAndFromSE;
                            audioSource.volume = seData.volume;
                            audioSource.pitch = seData.pitch;
                            if (seData.clip != null) audioSource.PlayOneShot(seData.clip);
                        }
                    }
                }
            }

            //移動・回転を行う
            rb.MovePosition(posDiff);
            rb.MoveRotation(rotDiff);
        }
    }
}
