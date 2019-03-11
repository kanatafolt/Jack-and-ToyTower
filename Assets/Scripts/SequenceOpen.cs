////
//SequenceOpen.cs
//指定したスイッチがONになることで、紐付いたステージオブジェクトが順番に展開されるギミックスクリプト
//各オブジェクトを移動・回転させながら展開することができるが、併用は想定していないため注意
//拡張案：時間制限付きスイッチを実装し、シークエンスに可逆性をもたせる(未実装)
////

#pragma warning disable 0649    //参照先がnullのままであるという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceOpen : MonoBehaviour
{
    [SerializeField] SwitchManager switchObj;           //このスイッチがONになるとシークエンスが展開開始
    [SerializeField] float openTime = 0.5f;             //展開時間
    [SerializeField] float openInterval = 0.1f;         //展開間隔

    [System.Serializable] [SerializeField] struct SequenceObjects {
        public Transform trans;                                 //展開対象オブジェクト
        public Vector3 moveDiff;                                //移動量
        public Vector3 rotDiff;                                 //回転量
        [HideInInspector] public Vector3 defaultPos;            //元々のpositionを保持
        [HideInInspector] public Quaternion defaultRot;         //元々のrotationを保持
        [HideInInspector] public Vector3 rotPivot;              //rotDiffから回転軸を分離
        [HideInInspector] public float rotAngle;                //rotDiffから回転角度を分離
    }
    [SerializeField] SequenceObjects[] seq;

    private float timeElapsed;
    private int sequencedCount = 0;
    private bool allSequenced = false;

    private void Start()
    {
        for (int i = 0; i < seq.Length; i++)
        {
            if (seq[i].trans != null)
            {
                seq[i].defaultPos = seq[i].trans.position;
                seq[i].defaultRot = seq[i].trans.rotation;
                seq[i].rotPivot = seq[i].rotDiff.normalized;
                seq[i].rotAngle = seq[i].rotDiff.magnitude;
            }
        }
    }

    private void Update()
    {
        if (switchObj.isOn && !allSequenced)
        {
            //スイッチがONかつ、シークエンスが完了していないとき
            timeElapsed += Time.deltaTime;

            //各ステージオブジェクトを時間差で展開していく
            for (int i = sequencedCount; i < seq.Length; i++)
            {
                if (timeElapsed >= openInterval * i && seq[i].trans != null)
                {
                    float diffRate = (timeElapsed - openInterval * i) / openTime;

                    if (diffRate > 1.0f)
                    {
                        diffRate = 1.0f;
                        sequencedCount++;
                    }

                    seq[i].trans.position = seq[i].defaultPos + seq[i].trans.TransformDirection(seq[i].moveDiff * diffRate);
                    seq[i].trans.rotation = Quaternion.AngleAxis(seq[i].rotAngle * diffRate, seq[i].trans.TransformDirection(seq[i].rotPivot)) * seq[i].defaultRot;
                }

                if (seq[i].trans == null && i == sequencedCount) sequencedCount++;
            }

            if (sequencedCount >= seq.Length) allSequenced = true;
        }
    }
}
