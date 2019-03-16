////
//MainGameManager.cs
//ゲーム全体の基本機能をもつスクリプト
//現在の機能：
//・各種キー入力を受け付け、ゲーム進行の操作を行う(Rでシーンリセットなど)
//・スコアなどゲームデータを管理する
//・スコアなどUI表示を行う
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    public float score = 0.0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //シーンのリセット
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void OnGUI()
    {
        
    }
}
