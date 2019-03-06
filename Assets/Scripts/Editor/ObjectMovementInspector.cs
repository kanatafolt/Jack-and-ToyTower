////
//ObjectMovementInspector.cs
//ObjectMovement用のインスペクタ拡張スクリプト
//挙動(並進、回転など)のグループごとに有効/無効を設定し、有効時のみ関連する変数(移動量など)が編集できるようになる
//参考：[Unityエディター拡張] フィールドをフォルダでまとめる
//https://www.scriptlife.jp/contents/programming/2015/09/08/post-274/
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(ObjectMovement))]
public class ObjectMovementInspector : Editor
{
    private AnimBool translationGroup;
    private AnimBool rotationGroup;
    //private AnimBool circularOrbitGroup;

    public void OnEnable()
    {
        ObjectMovement obj = target as ObjectMovement;

        translationGroup = new AnimBool(obj.translationVelocity != Vector3.zero);
        rotationGroup = new AnimBool(obj.rotationVelocity != 0.0f);
        //circularOrbitGroup = new AnimBool(obj.circularOrbitVelocity != Vector3.zero);

        translationGroup.valueChanged.AddListener(Repaint);
        rotationGroup.valueChanged.AddListener(Repaint);
        //circularOrbitGroup.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI()
    {
        ObjectMovement obj = target as ObjectMovement;

        obj.lifeTime = EditorGUILayout.FloatField("Life Time", obj.lifeTime);

        translationGroup.target = EditorGUILayout.ToggleLeft("Translation", translationGroup.target);
        if (EditorGUILayout.BeginFadeGroup(translationGroup.faded))
        {
            obj.translationVelocity = EditorGUILayout.Vector3Field("Velocity", obj.translationVelocity);
        }
        EditorGUILayout.EndFadeGroup();

        rotationGroup.target = EditorGUILayout.ToggleLeft("Rotation", rotationGroup.target);
        if (EditorGUILayout.BeginFadeGroup(rotationGroup.faded))
        {
            obj.rotationPivot = EditorGUILayout.Vector3Field("Pivot", obj.rotationPivot);
            obj.rotationVelocity = EditorGUILayout.FloatField("Velocity", obj.rotationVelocity);
        }
        EditorGUILayout.EndFadeGroup();

        //circularOrbitGroup.target = EditorGUILayout.ToggleLeft("Circular Orbit", circularOrbitGroup.target);
        //if (EditorGUILayout.BeginFadeGroup(circularOrbitGroup.faded))
        //{
        //    obj.circularOrbitPivot = EditorGUILayout.Vector3Field("Pivot", obj.circularOrbitPivot);
        //    obj.circularOrbitRadius = EditorGUILayout.FloatField("Radius", obj.circularOrbitRadius);
        //    obj.circularOrbitVelocity = EditorGUILayout.FloatField("Velocity", obj.circularOrbitVelocity);
        //}
        //EditorGUILayout.EndFadeGroup();
    }
}
