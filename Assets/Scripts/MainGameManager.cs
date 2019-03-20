////
//MainGameManager.cs
//ゲーム全体の基本機能をもつスクリプト
//現在の機能：
//・各種キー入力を受け付け、ゲーム進行の操作を行う
//・スコアなどゲームデータを管理する
//・タワーの出現状態など、ゲームの進行情報を受け取り、また受け渡す
//・スコアなどUI表示を行う
////

#pragma warning disable 0649    //変数が初期化されていないという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    private float score = 0.0f;
    private Text scoreText;

    [HideInInspector] public bool towerAppearanced = false;

    public GameObject debugSphere;

    private void Start()
    {
        scoreText = GameObject.Find("ScoreNumText").GetComponent<Text>();
    }

    private void Update()
    {

    }

    private void OnGUI()
    {
        scoreText.text = score.ToString();
    }

    public void GetScore(float s)
    {
        //スコアを加算し、スコアUIの演出を行う
        score += s;
    }
}
