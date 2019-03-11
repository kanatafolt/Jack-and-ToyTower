////
//SwitchManager.cs
//スイッチオブジェクトのON/OFF動作を管理する基本機能スクリプト
//スイッチの種類一覧
//タッチスイッチ(touch)：プレイヤーが接触したら
//ヒットスイッチ(hardHit)：接触したプレイヤーのvelocity.sqrMagnitudeが一定値以上なら
////

#pragma warning disable 0649    //ゲームオブジェクトがnullのままであるという警告

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{
    enum SwitchType { touchSwitch, hitSwitch }
    [SerializeField] SwitchType switchType = SwitchType.touchSwitch;

    public bool isOn = false;
    private float timeElapsed;
    private bool switched = false;

    [System.Serializable] [SerializeField] struct TouchSwitch           //タッチスイッチのパラメータ
    {
        public Color fromColor;             //OFFのときの色
        public Color toColor;               //ONのときの色
        public float colorChangeTime;       //色が変化する時間
        public Light spotLight;             //Spot Lightを格納しておく
    }
    [SerializeField] TouchSwitch touchSwitch = new TouchSwitch { fromColor = new Color(0.853f, 0.457f, 0.858f), toColor = new Color(0.993f, 0.674f, 1.000f, 1.0f), colorChangeTime = 0.1f };

    [System.Serializable] [SerializeField] struct HitSwitch             //ヒットスイッチのパラメータ
    {

    }
    [SerializeField] HitSwitch hitSwitch = new HitSwitch { };

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "player" && !isOn)
        {
            //スイッチにプレイヤーが衝突したとき(スイッチ起動後は無視)
            switch (switchType)
            {
                //タッチスイッチ
                case SwitchType.touchSwitch:
                    isOn = true;
                    break;

                //ヒットスイッチ
                case SwitchType.hitSwitch:
                    if (collision.gameObject.GetComponent<Rigidbody>().velocity.sqrMagnitude >= 1.0f)
                    {
                        isOn = true;
                    }
                    break;
            }
        }
    }

    private void Update()
    {
        if (isOn && !switched)
        {
            //スイッチ起動後、スイッチの変化処理を行う(スイッチ変化後は無視)
            timeElapsed += Time.deltaTime;

            switch (switchType)
            {
                //タッチスイッチ
                case SwitchType.touchSwitch:
                    if (timeElapsed > touchSwitch.colorChangeTime)
                    {
                        timeElapsed = touchSwitch.colorChangeTime;
                        switched = true;
                    }

                    GetComponent<Renderer>().material.color = touchSwitch.fromColor + new Color (touchSwitch.toColor.r - touchSwitch.fromColor.r, 
                        touchSwitch.toColor.g - touchSwitch.fromColor.g, touchSwitch.toColor.b - touchSwitch.fromColor.b) * timeElapsed / touchSwitch.colorChangeTime;
                    touchSwitch.spotLight.intensity = timeElapsed / touchSwitch.colorChangeTime;
                    break;

                //ヒットスイッチ
                case SwitchType.hitSwitch:

                    break;
            }
        }
    }
}
