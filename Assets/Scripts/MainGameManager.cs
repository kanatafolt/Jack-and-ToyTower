////
//MainGameManager.cs
//ゲーム全体の基本機能をもつスクリプト
//現在の機能：
//・各種キー入力を受け付け、ゲーム進行の操作を行う
//・プレイ時間、スコアなどゲームデータを管理する
//・タワーの出現状態など、ゲームの進行情報を受け取り、また受け渡す
//・スコアなどUI表示を行う
////

#pragma warning disable 0649    //変数が初期化されていないという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGameManager : MonoBehaviour
{
    public enum ScoreType { smallStar, bigStar };

    public float playTime = 0.0f;
    public float score = 0.0f;
    public float smallStarCount = 0.0f, bigStarCount = 0.0f;
    public float allSmallStarQuantity, allBigStarQuantity;

    private Text scoreText;

    [HideInInspector] public bool towerAppearanced = false;
    [HideInInspector] public bool gameCleared = false;

    private void Start()
    {
        scoreText = GameObject.Find("ScoreNumText").GetComponent<Text>();
    }

    private void Update()
    {
        if (towerAppearanced && !gameCleared)
        {
            playTime += Time.deltaTime;
        }
    }

    private void OnGUI()
    {
        //画面上のスコア表示を更新
        scoreText.text = score.ToString();
    }

    public void AddScore(ScoreType scoreType)
    {
        //スコアを加算する
        switch (scoreType)
        {
            case ScoreType.smallStar:

                score += 10.0f;
                smallStarCount += 1.0f;

                break;

            case ScoreType.bigStar:

                score += 50.0f;
                bigStarCount += 1.0f;

                break;
        }
    }
}
