////
//MazeSwitchMaster.cs
//二種類のスイッチのトグル管理、及び複数のスイッチの同期を行うスクリプト
////

#pragma warning disable 0649    //変数が初期化されていないという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeSwitchMaster : MonoBehaviour
{
    [SerializeField] SequenceOperator relianceSequence;
    [SerializeField] SwitchManager[] groupA = new SwitchManager[1];
    [SerializeField] SwitchManager[] groupB = new SwitchManager[1];

    private bool controlEnabled = false;            //タワーが起動するまでは制御を開始しない
    private bool groupAIsOn = true;
    private bool prevGroupAIsOn = true;             //スイッチが感知された時にForceSwitchingを一度だけ行う

    private void Update()
    {
        if (!controlEnabled)
        {
            if (relianceSequence.sequenceFinished)
            {
                controlEnabled = true;
                for (int i = 0; i < groupA.Length; i++) groupA[i].ForceSwitching(true);
            }
        }

        if (controlEnabled)
        {
            if (groupAIsOn)
            {
                //グループAがONのとき：グループBがONになったかどうかを監視する
                for (int i = 0; i < groupB.Length; i++) if (groupB[i].isOn) groupAIsOn = false;
            }
            else
            {
                //グループBがONのとき：グループAがONになったかどうかを監視する
                for (int i = 0; i < groupA.Length; i++) if (groupA[i].isOn) groupAIsOn = true;
            }

            if (groupAIsOn != prevGroupAIsOn)
            {
                //スイッチの変更が感知されたらスイッチ切り替え処理を行う
                if (groupAIsOn)
                {
                    for (int i = 0; i < groupA.Length; i++) groupA[i].ForceSwitching(true);
                    for (int i = 0; i < groupB.Length; i++) groupB[i].ForceSwitching(false);
                }
                else
                {
                    for (int i = 0; i < groupA.Length; i++) groupA[i].ForceSwitching(false);
                    for (int i = 0; i < groupB.Length; i++) groupB[i].ForceSwitching(true);
                }

                prevGroupAIsOn = groupAIsOn;
            }
        }
    }
}
