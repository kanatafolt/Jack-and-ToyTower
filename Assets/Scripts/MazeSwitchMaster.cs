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
    [SerializeField] SequenceOperator groupASequence;
    [SerializeField] SwitchManager[] groupA = new SwitchManager[1];
    [SerializeField] SwitchManager[] groupB = new SwitchManager[1];

    private MainGameManager gameManager;
    private bool firstSwitching = false;            //初回のスイッチ操作を行ったかどうか
    private bool controlEnabled = false;            //シークエンス後、スイッチのinitialPositionを再設定したかどうか
    private bool groupAIsOn = true;
    private bool prevGroupAIsOn = true;             //スイッチが感知された時にForceSwitchingを一度だけ行う

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<MainGameManager>();
    }

    private void Update()
    {
        if (!controlEnabled)
        {
            if (!firstSwitching)
            {
                if (gameManager.towerAppearanced)
                {
                    firstSwitching = true;
                    groupASequence.soundOn = false;
                    for (int i = 0; i < groupA.Length; i++) groupA[i].ForceSwitching(true);
                }
            }
            else
            {
                if (relianceSequence.sequenceFinished)
                {
                    groupASequence.soundOn = true;
                    controlEnabled = true;
                    for (int i = 0; i < groupA.Length; i++) groupA[i].InitialPositionUpdate();
                }
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
