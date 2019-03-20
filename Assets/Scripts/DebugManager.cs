////
//DebugManager.cs
//デバッグ用の各種機能を扱うスクリプト
//機能を停止させる場合はシーン上でオブジェクトごと無効化すること
//現在の機能一覧：
//・デバッグ用情報表示
//・プレイヤーの初期位置を変更する
//・シーンのリセット
//・すべてのスイッチを強制的にONにするSuperForceOn機能
//・スペクテイターモード(スペースとシフトによる空中移動、重力・当たり判定無視)の切り替え
//タワーが出現した状態でゲームが始まるTowerAppearance機能
////

#pragma warning disable 0649    //変数が初期化されていないという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DebugManager : MonoBehaviour
{
    [SerializeField] GUISkin skin;                              //デバッグUIのGUIスキン
    [SerializeField] GameObject spawner;                        //プレイヤーの初期位置
    public bool suparForceOn = false;                           //スイッチの一括管理
    public bool spectatorMode = false;                          //スペクテイターモード
    [SerializeField] bool towerAppearance = true;               //タワーが出現済みの状態でゲームが始まる

    private KeyCode restartKey = KeyCode.R;
    private KeyCode superForceOnKey = KeyCode.F;
    private KeyCode spectatorKey = KeyCode.E;

    private MainGameManager gameManager;
    private GameObject player, cameraRig;
    private Rigidbody playerRb;
    private bool spectatored = false;           //スペクテイターモードと一致させ、切り替えた瞬間に一度だけ処理を行うための変数

    private void Reset()
    {
        spawner = GameObject.Find("PlayerSpawner");
    }

    private void OnDrawGizmos()
    {
        //デバッグ用：シーン編集時、プレイヤーの初期位置に目印を表示する
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.5f);
        Gizmos.DrawCube(spawner.transform.position, Vector3.one * 0.2f);
    }

    private void Awake()
    {
        if (towerAppearance)
        {
            //TowerAppearanceが有効のとき：タワースイッチを無効化する
            GameObject.Find("TowerSwitch(Floor)").SetActive(false);
        }
    }

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<MainGameManager>();
        player = GameObject.Find("Jack");
        cameraRig = GameObject.Find("CameraRig");
        playerRb = player.GetComponent<Rigidbody>();

        //プレイヤーの初期位置を変更する
        player.transform.position = spawner.transform.position + Vector3.up * 1.5f;

        if (towerAppearance)
        {
            gameManager.towerAppearanced = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(restartKey))
        {
            //Rキー：シーンのリセット
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(superForceOnKey))
        {
            //Tキー：SuperForceOnを有効化
            suparForceOn = true;
        }

        if (Input.GetKeyDown(spectatorKey))
        {
            //Gキー：スペクテイターモードの切り替え(スペースとシフトによる空中移動、重力・当たり判定無視)
            spectatorMode = (spectatorMode) ? false : true;
        }

        if (spectatorMode)
        {
            if (!spectatored)
            {
                player.GetComponent<PlayerCollisionSwitcher>().SetCollisionOff();
                playerRb.useGravity = false;
                spectatored = true;
            }

            //スペクテイターモードでの移動処理
            playerRb.velocity = Vector3.zero;
            playerRb.position += cameraRig.transform.TransformDirection(new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("FloatingVertical"), Input.GetAxisRaw("Vertical")) * 0.15f);
        }
        else
        {
            if (spectatored)
            {
                player.GetComponent<PlayerCollisionSwitcher>().SetCollisionOn();
                playerRb.useGravity = true;
                spectatored = false;
            }
        }
    }

    private void OnGUI()
    {
        //デバッグ時の画面表示を行う
        float leftMargin = 10.0f;       //左マージン
        float topMargin = 10.0f;        //上マージン(自動改行送り)
        float lineWidth = 1000.0f;    //一行の横幅
        float lineHeight = 28.0f;       //一行の縦幅

        GUI.Label(new Rect(leftMargin, topMargin, lineWidth, lineHeight), "DEBUG MODE", skin.GetStyle("label"));
        topMargin += lineHeight;

        GUI.Label(new Rect(leftMargin, topMargin, lineWidth, lineHeight), restartKey + " : Restart", skin.GetStyle("label"));
        topMargin += lineHeight;

        GUI.Label(new Rect(leftMargin, topMargin, lineWidth, lineHeight), superForceOnKey + " : Supar Force On", skin.GetStyle("label"));
        topMargin += lineHeight;

        GUI.Label(new Rect(leftMargin, topMargin, lineWidth, lineHeight), spectatorKey + " : Spectator Mode (" + spectatorMode + ")", skin.GetStyle("label"));
        topMargin += lineHeight;
    }
}
