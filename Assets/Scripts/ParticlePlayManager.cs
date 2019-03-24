////
//ParticlePlayManager.cs
//複数のパーティクルを管理し、まとめて再生するスクリプト
////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayManager : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particles = new ParticleSystem[1];

    public void Play()
    {
        //パーティクルをまとめて再生する
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Play();
        }
    }
}
