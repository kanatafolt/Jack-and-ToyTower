////
//Event_GameClear.cs
//ゲームクリアの演出を行うスクリプト
////

#pragma warning disable 0649    //変数が初期化されていないという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Event_GameClear : MonoBehaviour
{
    [SerializeField] SwitchManager goalSwitch;
    [SerializeField] ObjectScalePop goalTextPop, scoreBoardPop;
    [SerializeField] Text clearTimeText, smallScoreText, bigScoreText;
    [SerializeField] Text smallScoreQuantityLabel, bigScoreQuantityLabel;
    private bool cleared = false;
    private float elapsedTime = 0.0f;

    private MainGameManager gameManager;
    private AudioManager audioManager;
    private AudioSource audioSource;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<MainGameManager>();
        audioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (goalSwitch.isOn && !cleared)
        {
            if (elapsedTime == 0.0f)
            {
                gameManager.gameCleared = true;

                //ゴールの効果音を鳴らす
                AudioManager.SEData seData = audioManager.clearFanfareSE;
                audioSource.volume = seData.volume;
                audioSource.pitch = seData.pitch;
                if (seData.clip != null) audioSource.PlayOneShot(seData.clip);
            }

            elapsedTime += Time.deltaTime;

            if (elapsedTime >= 0.8f)
            {
                //ゴールテキストとクリアタイムを表示する
                goalTextPop.popStart = true;
                scoreBoardPop.popStart = true;

                //クラッカーを起動

                //クリア処理完了フラグを立てる
                cleared = true;
            }
        }
    }

    private void OnGUI()
    {
        //クリアタイム表示
        float minute = gameManager.playTime / 60.0f;
        float second = gameManager.playTime % 60.0f;
        clearTimeText.text = minute.ToString("0") + ":" + second.ToString("00");

        //スモールスター数表示
        smallScoreText.text = gameManager.smallStarCount.ToString();
        smallScoreQuantityLabel.text = gameManager.allSmallStarQuantity.ToString();

        //ビッグスター数表示
        bigScoreText.text = gameManager.bigStarCount.ToString();
        bigScoreQuantityLabel.text = gameManager.allBigStarQuantity.ToString();
    }
}
