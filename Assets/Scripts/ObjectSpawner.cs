////
//ObjectSpawner.cs
//指定条件でプレハブのクローンを生成するスクリプト
////

#pragma warning disable 0649    //ゲームオブジェクトがnullのままであるという警告

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] GameObject spawnObj;
    [SerializeField] float generateCycle = 1.0f;
    [SerializeField] bool spawnOnAwake = true;
    private float elapsedTime = 0.0f;

    private void Start()
    {
        if (spawnOnAwake) elapsedTime = generateCycle;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= generateCycle && spawnObj != null)
        {
            elapsedTime -= generateCycle;
            Instantiate(spawnObj, transform.position, transform.rotation);
        }
    }
}
