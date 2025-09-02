using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dig/DigSoundSettings", fileName = "NewDigSoundSettings")]
public class DigSoundSettings : ScriptableObject
{
    [Header("つるはし音声設定")]
    [Tooltip("つるはし掘削音（コンボ段階別）")]
    public AudioClip[] pickaxeDigSounds = new AudioClip[3]; // 0: 通常, 1: コンボ2, 2: コンボ3
    
    [Tooltip("つるはし爆発マーカー設置音")]
    public AudioClip pickaxeExplosionMarkerSound;
    
    [Tooltip("つるはし爆発音")]
    public AudioClip pickaxeExplosionSound;
    
    [Header("ドリル音声設定")]
    [Tooltip("ドリル掘削音")]
    public AudioClip drillDigSound;
    
    [Header("手掘り音声設定")]
    [Tooltip("手掘り音")]
    public AudioClip handDigSound;
    
    [Header("共通設定")]
    [Tooltip("音声の基本ボリューム")]
    [Range(0f, 1f)]
    public float baseVolume = 0.7f;
    
    [Tooltip("音声の基本ピッチ")]
    [Range(0.5f, 2f)]
    public float basePitch = 1f;
    
    [Tooltip("音声の空間化設定")]
    public bool useSpatialBlending = true;
    
    [Tooltip("音声の最大距離")]
    public float maxDistance = 50f;
}
