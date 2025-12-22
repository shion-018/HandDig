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
    
    [Tooltip("ドリル長押しモーター音（開始）")]
    public AudioClip drillMotorStartSound;
    
    [Tooltip("ドリル長押しモーター音（ループ）")]
    public AudioClip drillMotorLoopSound;
    
    [Tooltip("ドリル長押しモーター音（終了）")]
    public AudioClip drillMotorEndSound;
    
    [Tooltip("射出ドリル音（発射時）")]
    public AudioClip drillProjectileSound;
    
    [Header("手掘り音声設定")]
    [Tooltip("手掘り音")]
    public AudioClip handDigSound;
    
    [Header("お宝接近音設定")]
    [Tooltip("お宝接近音")]
    public AudioClip treasureProximitySound;
    
    [Tooltip("お宝接近音の最大距離")]
    public float treasureProximityMaxDistance = 30f;
    
    [Tooltip("お宝接近音の最小距離")]
    public float treasureProximityMinDistance = 5f;
    
    [Tooltip("お宝接近音の最大間隔（遠い時）")]
    public float treasureProximityMaxInterval = 2f;
    
    [Tooltip("お宝接近音の最小間隔（近い時）")]
    public float treasureProximityMinInterval = 0.3f;
    
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
