////
//SequenceSwitch.cs
//スイッチがオンになることで、紐付いたステージオブジェクトが順番に展開されるギミックスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceSwitch : MonoBehaviour
{
    private bool sequenced = false;
    [SerializeField] float openInterval = 0.1f;

    [System.Serializable] [SerializeField] struct StageObj {
        public GameObject a;
        public Vector3 posDiff;
        public Vector3 rotDiff;
    }
    [SerializeField] StageObj[] stageObj;

    private void OnCollisionEnter(Collision collision)
    {
        
    }
}
