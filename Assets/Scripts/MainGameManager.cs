////
//MainGameManager.cs
//ゲーム全体の基本機能をもつスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //シーンのリセット
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
