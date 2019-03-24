////
//SequenceOperator.cs
//指定したスイッチがONになることで、紐付いたステージオブジェクトが順番に展開されるギミックスクリプト
//各オブジェクトを移動・回転させながら展開することができるが、併用は想定していないため注意
//シークエンス対象のステージオブジェクトを配列に設定したら、そのオブジェクトが「格納位置から展開位置までの移動・回転」量を設定する
//その上で、対象のステージオブジェクトは「展開位置」に設置しておく(ゲームスタート時に自動で格納される)
////

#pragma warning disable 0649    //変数が初期化されていないという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceOperator : MonoBehaviour
{
    [SerializeField] bool forceOn = false;                          //デバッグ用変数：シークエンスを強制的に展開する

    [SerializeField] SwitchManager switchObj;                       //このスイッチがONになるとシークエンスが展開開始
    [SerializeField] float openTime = 0.4f;                         //展開時間
    [SerializeField] float openInterval = 0.2f;                     //展開間隔
    [HideInInspector] public bool sequenceFinished = false;         //シークエンスが完了したかどうか
    [HideInInspector] public bool soundOn = true;                   //falseの場合SEが鳴らない

    [System.Serializable] [SerializeField] struct SequenceObjects
    {
        public Transform trans;                                             //展開対象オブジェクト
        public Vector3 moveDiff;                                            //移動量
        public Vector3 rotDiff;                                             //回転量
        //public ObjectToAndFrom toAndFrom;                       //展開対象がObjectToAndFromによって制御されているなら一時停止させる
        [HideInInspector] public Rigidbody rb;                              //オブジェクトごとのRigidbody
        [HideInInspector] public Vector3 rotPivot;                          //rotDiffから回転軸を分離
        [HideInInspector] public float rotAngle;                            //rotDiffから回転角度を分離
        [HideInInspector] public bool sequenced;                            //シークエンス完了時のイベントを管理する
        [HideInInspector] public Vector3 quakeDiff;                         //振動させる場合、その振動量
        [HideInInspector] public ObjectToAndFrom toAndFrom;                 //展開対象がObjectToAndFromによって制御されているなら一時停止させる
        [HideInInspector] public ObjectToAndFromMulti toAndFromMulti;       //展開対象がObjectToAndFromによって制御されているなら一時停止させる
    }
    [SerializeField] SequenceObjects[] seq = new SequenceObjects[1];

    private float elapsedTime, prevElapsedTime;
    private float finishTime;
    [SerializeField] bool quakeEffect = false;
    [SerializeField] float quakeWidth = 0.0f;
    private bool hasRigidbody = false;              //動作中のみRigidbodyを付与する

    private AudioManager audioManager;
    private AudioSource quakeAudioSource;

    private void Reset()
    {
        switchObj = transform.Find("Switch").gameObject.GetComponent<SwitchManager>();
    }

    private void OnDrawGizmos()
    {
        //デバッグ用：シーン編集時、シークエンス対象のオブジェクトに目印を表示する
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        for (int i = 0; i < seq.Length; i++) if (seq[i].trans != null) Gizmos.DrawSphere(seq[i].trans.position, 0.45f);
    }

    private void Start()
    {
        audioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();

        finishTime = openInterval * (seq.Length - 1) + openTime;

        //シークエンス初期化処理：ステージ設計時には展開後の状態で置かれているため、最初にシークエンスを逆向きに行う
        for (int i = 0; i < seq.Length; i++)
        {
            if (seq[i].trans != null)
            {
                seq[i].rotPivot = seq[i].rotDiff.normalized;
                seq[i].rotAngle = seq[i].rotDiff.magnitude;
                seq[i].trans.position = seq[i].trans.TransformDirection(-seq[i].moveDiff) + seq[i].trans.position;
                seq[i].trans.rotation = Quaternion.AngleAxis(-seq[i].rotAngle, seq[i].trans.TransformDirection(seq[i].rotPivot)) * seq[i].trans.rotation;
                seq[i].sequenced = true;
                seq[i].quakeDiff = Vector3.zero;
                if (seq[i].trans.gameObject.GetComponent<ObjectToAndFrom>() != null) seq[i].toAndFrom = seq[i].trans.gameObject.GetComponent<ObjectToAndFrom>();
                if (seq[i].trans.gameObject.GetComponent<ObjectToAndFromMulti>() != null) seq[i].toAndFromMulti = seq[i].trans.gameObject.GetComponent<ObjectToAndFromMulti>();
            }
        }
    }

    private void FixedUpdate()
    {
        prevElapsedTime = elapsedTime;

        if ((switchObj.isOn || forceOn) && elapsedTime < finishTime)    elapsedTime += Time.deltaTime;
        if (!switchObj.isOn && !forceOn && elapsedTime > 0.0f)         elapsedTime -= Time.deltaTime;

        if (elapsedTime != prevElapsedTime)
        {
            if (!hasRigidbody)
            {
                //初回起動処理：シークエンス対象オブジェクトにRigidbodyを付与あるいは取得する
                for (int i = 0; i < seq.Length; i++)
                {
                    seq[i].rb = (seq[i].trans.gameObject.GetComponent<Rigidbody>() != null) ? seq[i].trans.gameObject.GetComponent<Rigidbody>() : seq[i].trans.gameObject.AddComponent<Rigidbody>();
                    seq[i].rb.isKinematic = true;

                    if (seq[i].toAndFrom != null) seq[i].toAndFrom.pausing = true;
                    if (seq[i].toAndFromMulti != null) seq[i].toAndFromMulti.pausing = true;
                }
                hasRigidbody = true;

                //振動させる場合音を鳴らす
                if (quakeEffect && soundOn)
                {
                    AudioManager.SEData seData = audioManager.earthQuakingSE;
                    if (seData.clip != null) AudioSource.PlayClipAtPoint(seData.clip, transform.position, seData.volume);
                }
            }

            //シークエンス動作中の処理
            for (int i = 0; i < seq.Length; i++)
            {
                //各ステージオブジェクトを時間差で展開していく
                if (seq[i].trans != null)
                {
                    float seqElapsedTime = elapsedTime - openInterval * i;                                      //シークエンスごとの起動時間からの累積時間を算出する

                    if (seqElapsedTime > 0.0f && seqElapsedTime < openTime)
                    {
                        //シークエンスごとの稼働時間内に入ったらシークエンス完了フラグをfalseにする
                        seq[i].sequenced = false;
                        if (seq[i].toAndFrom != null) seq[i].toAndFrom.pausing = true;
                        if (seq[i].toAndFromMulti != null) seq[i].toAndFromMulti.pausing = true;
                    }

                    if (!seq[i].sequenced)
                    {
                        //シークエンスごとの稼働時間内なら
                        float deltaMoveRate = 0.0f;
                        if (elapsedTime > prevElapsedTime)
                        {
                            deltaMoveRate = Time.deltaTime;
                            if (seqElapsedTime < Time.deltaTime) deltaMoveRate = seqElapsedTime;                                    //シークエンス初回フレームで余分に動かないようにする
                            if (seqElapsedTime > openTime) deltaMoveRate = Time.deltaTime - (seqElapsedTime - openTime);            //シークエンス最終フレームで余分に動かないようにする
                        }
                        if (elapsedTime < prevElapsedTime)
                        {
                            deltaMoveRate = -Time.deltaTime;
                            if (openTime - seqElapsedTime < Time.deltaTime) deltaMoveRate = -(openTime - seqElapsedTime);           //逆シークエンス初回フレームで余分に動かないようにする
                            if (seqElapsedTime < 0.0f) deltaMoveRate = -Time.deltaTime - seqElapsedTime;                            //逆シークエンス最終フレームで余分に動かないようにする
                        }
                        deltaMoveRate = deltaMoveRate / openTime;

                        Vector3 qd = Vector3.zero;
                        if (quakeEffect)
                        {
                            //振動させる場合：振動処理
                            qd = -seq[i].quakeDiff;
                            seq[i].quakeDiff = (Vector3.right * (seqElapsedTime % 0.1f - 0.05f) / 0.05f + Vector3.forward * (seqElapsedTime % 0.15f - 0.075f) / 0.075f) * quakeWidth;
                            if (seqElapsedTime > 0.0f && seqElapsedTime < openTime)
                            {
                                qd += seq[i].quakeDiff;
                            }
                        }

                        seq[i].rb.MovePosition(seq[i].trans.TransformDirection(seq[i].moveDiff * deltaMoveRate) + seq[i].rb.position + qd);
                        seq[i].rb.MoveRotation(Quaternion.AngleAxis(seq[i].rotAngle * deltaMoveRate, seq[i].trans.TransformDirection(seq[i].rotPivot)) * seq[i].rb.rotation);

                        if (seqElapsedTime <= 0.0f || seqElapsedTime >= openTime)
                        {
                            //シークエンスが完了したとき
                            seq[i].sequenced = true;
                            if (seq[i].toAndFrom != null) seq[i].toAndFrom.pausing = false;
                            if (seq[i].toAndFromMulti != null) seq[i].toAndFromMulti.pausing = false;

                            //シークエンス完了音を鳴らす
                            if (soundOn)
                            {
                                AudioManager.SEData seData = audioManager.sequenceFinishSE;
                                if (seData.clip != null) AudioSource.PlayClipAtPoint(seData.clip, seq[i].trans.position, seData.volume);
                            }
                        }
                    }
                }
            }

            if (elapsedTime >= finishTime)
            {
                sequenceFinished = true;
                elapsedTime = finishTime;
            }

            if (elapsedTime <= 0.0f)
            {
                sequenceFinished = false;
                elapsedTime = 0.0f;
            }
        }

        if (prevElapsedTime == elapsedTime && hasRigidbody)
        {
            //要らなくなったRigidbodyを破棄する処理
            for (int i = 0; i < seq.Length; i++)
            {
                if (seq[i].toAndFrom == null && seq[i].toAndFromMulti == null)
                {
                    Destroy(seq[i].rb);
                }
            }
            hasRigidbody = false;
        }
    }
}
