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
    [SerializeField] bool forceOn = false;              //デバッグ用変数：シークエンスを強制的に展開する

    [SerializeField] SwitchManager switchObj;           //このスイッチがONになるとシークエンスが展開開始
    [SerializeField] float openTime = 0.4f;             //展開時間
    [SerializeField] float openInterval = 0.2f;         //展開間隔

    [System.Serializable] [SerializeField] struct SequenceObjects {
        public Transform trans;                                 //展開対象オブジェクト
        public Vector3 moveDiff;                                //移動量
        public Vector3 rotDiff;                                 //回転量
        public bool isToAndFromObject;                          //ObjectToAndFromによって制御されるオブジェクトかどうか
        [HideInInspector] public Vector3 defaultPos;            //元々のpositionを保持
        [HideInInspector] public Quaternion defaultRot;         //元々のrotationを保持
        [HideInInspector] public Vector3 rotPivot;              //rotDiffから回転軸を分離
        [HideInInspector] public float rotAngle;                //rotDiffから回転角度を分離
        [HideInInspector] public bool sequenced;                //シークエンス完了時のイベントを管理する
    }
    [SerializeField] SequenceObjects[] seq = new SequenceObjects[1];

    private float elapsedTime, prevElapsedTime;
    private float finishTime;

    [SerializeField] bool quakeEffect = false;
    private AudioManager audioManager;

    private void Reset()
    {
        switchObj = transform.Find("Switch").gameObject.GetComponent<SwitchManager>();
    }

    private void OnDrawGizmos()
    {
        //デバッグ用：シーン編集時、シークエンス対象のオブジェクトに目印を表示する
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.3f);
        for (int i = 0; i < seq.Length; i++) if (seq[i].trans != null) Gizmos.DrawSphere(seq[i].trans.position + seq[i].trans.TransformDirection(Vector3.forward * -7.0f + Vector3.up * 0.25f), 0.45f);
    }

    private void Start()
    {
        audioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();

        finishTime = openInterval * (seq.Length - 1) + openTime;

        //ステージ設計時には展開後の状態で置かれているため、最初にシークエンスを逆向きに行う
        for (int i = 0; i < seq.Length; i++)
        {
            if (seq[i].trans != null)
            {
                seq[i].rotPivot = seq[i].rotDiff.normalized;
                seq[i].rotAngle = seq[i].rotDiff.magnitude;
                seq[i].trans.position = seq[i].trans.position + seq[i].trans.TransformDirection(-seq[i].moveDiff);
                seq[i].trans.rotation = Quaternion.AngleAxis(-seq[i].rotAngle, seq[i].trans.TransformDirection(seq[i].rotPivot)) * seq[i].trans.rotation;
            }
        }
    }

    private void Update()
    {
        prevElapsedTime = elapsedTime;

        if ((switchObj.isOn || forceOn) && elapsedTime < finishTime)    elapsedTime += Time.deltaTime;
        if (!switchObj.isOn && !forceOn && elapsedTime > 0.0f)         elapsedTime -= Time.deltaTime;

        if (elapsedTime != prevElapsedTime)
        {
            if (prevElapsedTime == 0.0f)
            {
                //初回起動時のみ初期位置を保存する
                for (int i = 0; i < seq.Length; i++)
                {
                    if (seq[i].trans != null)
                    {
                        seq[i].defaultPos = seq[i].trans.position;
                        seq[i].defaultRot = seq[i].trans.rotation;
                    }
                }

                //振動させる場合音を鳴らす
                if (quakeEffect)
                {
                    AudioManager.SEData seData = audioManager.earthQuakingSE;
                    if (seData.clip != null) AudioSource.PlayClipAtPoint(seData.clip, transform.position, seData.volume);
                }
            }

            //各ステージオブジェクトを時間差で展開していく
            for (int i = 0; i < seq.Length; i++)
            {
                if (seq[i].trans != null)
                {
                    float diffRate = (elapsedTime - openInterval * i) / openTime;
                    if (diffRate > 1.0f) diffRate = 1.0f;
                    if (diffRate < 0.0f) diffRate = 0.0f;

                    if (!seq[i].isToAndFromObject)
                    {
                        seq[i].trans.position = seq[i].defaultPos + seq[i].trans.TransformDirection(seq[i].moveDiff * diffRate);
                        seq[i].trans.rotation = Quaternion.AngleAxis(seq[i].rotAngle * diffRate, seq[i].trans.TransformDirection(seq[i].rotPivot)) * seq[i].defaultRot;
                    }
                    else
                    {
                        //シークエンス対象がToAndFromオブジェクトの場合
                    }

                    if (!seq[i].sequenced)
                    {
                        if (quakeEffect) seq[i].trans.position += seq[i].trans.TransformDirection(Vector3.right) * 0.2f * (elapsedTime % 0.1f - 0.05f) / 0.05f;         //振動させる場合横揺れ
                        if (quakeEffect) seq[i].trans.position += seq[i].trans.TransformDirection(Vector3.forward) * 0.2f * (elapsedTime % 0.15f - 0.075f) / 0.075f;    //振動させる場合奥揺れ
                    }

                    if (diffRate == 1.0f && !seq[i].sequenced)
                    {
                        if (quakeEffect) seq[i].trans.position = seq[i].defaultPos + seq[i].trans.TransformDirection(seq[i].moveDiff * diffRate);       //振動させた場合座標を正しい位置に戻して終わる
                        seq[i].sequenced = true;

                        //シークエンス完了音を鳴らす
                        AudioManager.SEData seData = audioManager.sequenceFinishSE;
                        if (seData.clip != null) AudioSource.PlayClipAtPoint(seData.clip, seq[i].trans.position, seData.volume);
                    }

                    if (diffRate > 0.0f && diffRate < 1.0f) seq[i].sequenced = false;
                }
            }

            if (elapsedTime >= finishTime) elapsedTime = finishTime;
            if (elapsedTime <= 0.0f) elapsedTime = 0.0f;
        }
    }
}
