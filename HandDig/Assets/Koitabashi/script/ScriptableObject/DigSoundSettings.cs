using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReverbSettings
{
    [Tooltip("リバーブを適用するか")]
    public bool enabled = false;
    
    [Tooltip("リバーブプリセット（Cave = 洞窟、Custom = カスタム設定）")]
    public AudioReverbPreset preset = AudioReverbPreset.Cave;
    
    [Tooltip("リバーブの強度（0-1、プリセット使用時は無視されます）")]
    [Range(0f, 1f)]
    public float reverbLevel = 0.5f;
    
    [Tooltip("リバーブの減衰時間（秒、プリセット使用時は無視されます）")]
    [Range(0.1f, 20f)]
    public float decayTime = 1.5f;
}

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
    
    [Header("つるはしリバーブ設定")]
    [Tooltip("つるはし掘削音のリバーブ設定")]
    public ReverbSettings pickaxeDigReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Tooltip("つるはし爆発音のリバーブ設定")]
    public ReverbSettings pickaxeExplosionReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Header("ドリル音声設定")]
    [Tooltip("ドリル掘削音")]
    public AudioClip drillDigSound;
    
    [Tooltip("射出ドリル音（発射時）")]
    public AudioClip drillProjectileSound;
    
    [Header("ドリルリバーブ設定")]
    [Tooltip("ドリル掘削音のリバーブ設定")]
    public ReverbSettings drillDigReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Tooltip("射出ドリル音のリバーブ設定")]
    public ReverbSettings drillProjectileReverb = new ReverbSettings { enabled = false, preset = AudioReverbPreset.Cave };
    
    [Header("手掘り音声設定")]
    [Tooltip("手掘り音")]
    public AudioClip handDigSound;
    
    [Header("手掘りリバーブ設定")]
    [Tooltip("手掘り音のリバーブ設定")]
    public ReverbSettings handDigReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Header("お宝接近音声設定")]
    [Tooltip("お宝接近音")]
    public AudioClip treasureProximitySound;
    
    [Tooltip("お宝接近音の最大検出距離")]
    public float treasureProximityMaxDistance = 30f;
    
    [Tooltip("お宝接近音の最小検出距離")]
    public float treasureProximityMinDistance = 5f;
    
    [Tooltip("お宝接近音の最大再生間隔（遠距離時）")]
    public float treasureProximityMaxInterval = 2f;
    
    [Tooltip("お宝接近音の最小再生間隔（近距離時）")]
    public float treasureProximityMinInterval = 0.3f;
    
    [Header("お宝接近音リバーブ設定")]
    [Tooltip("お宝接近音のリバーブ設定")]
    public ReverbSettings treasureProximityReverb = new ReverbSettings { enabled = false, preset = AudioReverbPreset.Cave };
    
    [Header("足音設定")]
    [Tooltip("足音（複数用意するとランダムに再生されます）")]
    public AudioClip[] footstepSounds;
    
    [Tooltip("足音の再生間隔（移動距離ベース）")]
    public float footstepDistance = 1.5f;
    
    [Tooltip("足音のボリューム（0-1）")]
    [Range(0f, 1f)]
    public float footstepVolume = 0.5f;
    
    [Tooltip("足音のピッチランダム範囲（±この値でランダムに変化）")]
    [Range(0f, 0.3f)]
    public float footstepPitchRandomness = 0.1f;
    
    [Header("足音リバーブ設定")]
    [Tooltip("足音のリバーブ設定")]
    public ReverbSettings footstepReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
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
