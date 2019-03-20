////
//AudioManager.cs
//効果音のデータを管理するスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public struct SEData
    {
        public AudioClip clip;
        public float volume;
        public float pitch;
    }

    public SEData hopWalkSE;
    public SEData jumpSE;
    public SEData randingSE;
    public SEData getStarSE;
    public SEData switchOnSE;
    //public SEData switchOffSE;
    public SEData sequenceFinishSE;
    public SEData objectToAndFromSE;

    //ステージオブジェクト関係
    public SEData safetyNetSE;

    //イベントシーン関係
    public SEData earthQuakingSE;
}
