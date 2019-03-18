////
//GenerateObjectEdge.cs
//チュートリアルオブジェクトなどに付与し、四方向にオブジェクトを複製することでエッジ強調を行うスクリプト
////

#pragma warning disable 0649    //変数が初期化されていないという警告を無視する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateObjectEdge : MonoBehaviour
{
    [SerializeField] GameObject baseObj;
    [SerializeField] float edgeWidth = 0.02f;
    [SerializeField] float backDepth = 0.01f;
    [SerializeField] Material edgeMat;
    private GameObject[] edgeObj = new GameObject[4];
    private float[] rightDir = new float[4] { 1.0f, -1.0f, 1.0f, -1.0f };
    private float[] upDir = new float[4] { 1.0f, 1.0f, -1.0f, -1.0f };

    private void Reset()
    {
        baseObj = transform.Find("default").gameObject;
    }

    private void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            if (baseObj != null)
            {
                edgeObj[i] = Instantiate(baseObj, transform);
                edgeObj[i].transform.position = baseObj.transform.position + baseObj.transform.TransformDirection((Vector3.right * rightDir[i] + Vector3.up * upDir[i]) * edgeWidth + Vector3.forward * -backDepth);
                if (edgeMat != null) edgeObj[i].GetComponent<Renderer>().material = edgeMat;
            }
        }
    }
}
