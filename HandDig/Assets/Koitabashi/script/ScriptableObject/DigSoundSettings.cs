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
    
    [Header("つるはしボリューム設定")]
    [Tooltip("つるはし掘削音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float pickaxeDigVolume = -1f;
    
    [Tooltip("つるはし爆発音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float pickaxeExplosionVolume = -1f;
    
    [Tooltip("つるはしモード切替音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float pickaxeModeSwitchVolume = -1f;
    
    [Tooltip("つるはしモード切替音")]
    public AudioClip pickaxeModeSwitchSound;
    
    [Header("つるはしリバーブ設定")]
    [Tooltip("つるはし掘削音のリバーブ設定")]
    public ReverbSettings pickaxeDigReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Tooltip("つるはし爆発音のリバーブ設定")]
    public ReverbSettings pickaxeExplosionReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Tooltip("つるはしモード切替音のリバーブ設定")]
    public ReverbSettings pickaxeModeSwitchReverb = new ReverbSettings { enabled = false, preset = AudioReverbPreset.Cave };
    
    [Header("ドリル音声設定")]
    [Tooltip("ドリル掘削音")]
    public AudioClip drillDigSound;
    
    [Tooltip("射出ドリル音（発射時）")]
    public AudioClip drillProjectileSound;
    
    [Tooltip("ドリル掘削音の再生間隔制限（秒、この時間以内の連続再生を防ぐ）")]
    [Range(0f, 0.2f)]
    public float drillDigSoundCooldown = 0.05f;
    
    [Header("ドリルボリューム設定")]
    [Tooltip("ドリル掘削音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float drillDigVolume = -1f;
    
    [Tooltip("射出ドリル音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float drillProjectileVolume = -1f;
    
    [Tooltip("ドリルモード切替音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float drillModeSwitchVolume = -1f;
    
    [Tooltip("ドリルモード切替音")]
    public AudioClip drillModeSwitchSound;
    
    [Header("ドリルリバーブ設定")]
    [Tooltip("ドリル掘削音のリバーブ設定")]
    public ReverbSettings drillDigReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Tooltip("射出ドリル音のリバーブ設定")]
    public ReverbSettings drillProjectileReverb = new ReverbSettings { enabled = false, preset = AudioReverbPreset.Cave };
    
    [Tooltip("ドリルモード切替音のリバーブ設定")]
    public ReverbSettings drillModeSwitchReverb = new ReverbSettings { enabled = false, preset = AudioReverbPreset.Cave };
    
    [Header("手掘り音声設定")]
    [Tooltip("手掘り音")]
    public AudioClip handDigSound;
    
    [Header("手掘りボリューム設定")]
    [Tooltip("手掘り音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float handDigVolume = -1f;
    
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
    
    [Header("お宝接近音ボリューム設定")]
    [Tooltip("お宝接近音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float treasureProximityVolume = -1f;
    
    [Header("お宝接近音リバーブ設定")]
    [Tooltip("お宝接近音のリバーブ設定")]
    public ReverbSettings treasureProximityReverb = new ReverbSettings { enabled = false, preset = AudioReverbPreset.Cave };
    
    [Header("足音設定")]
    [Tooltip("足音（複数用意するとランダムに再生されます）")]
    public AudioClip[] footstepSounds;
    
    [Tooltip("足音の再生間隔（移動距離ベース）")]
    public float footstepDistance = 1.5f;
    
    [Tooltip("足音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float footstepVolume = -1f;
    
    [Tooltip("足音のピッチランダム範囲（±この値でランダムに変化）")]
    [Range(0f, 0.3f)]
    public float footstepPitchRandomness = 0.1f;
    
    [Header("足音リバーブ設定")]
    [Tooltip("足音のリバーブ設定")]
    public ReverbSettings footstepReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Header("ドリルモーター音声設定")]
    [Tooltip("ドリル開始音")]
    public AudioClip drillMotorStartSound;
    
    [Tooltip("ドリルループ音")]
    public AudioClip drillMotorLoopSound;
    
    [Tooltip("ドリル終了音")]
    public AudioClip drillMotorEndSound;
    
    [Header("ドリルモーターボリューム設定")]
    [Tooltip("ドリル開始音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float drillMotorStartVolume = -1f;
    
    [Tooltip("ドリルループ音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float drillMotorLoopVolume = -1f;
    
    [Tooltip("ドリル終了音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float drillMotorEndVolume = -1f;
    
    [Header("ドリルモーターリバーブ設定")]
    [Tooltip("ドリル開始音のリバーブ設定")]
    public ReverbSettings drillMotorStartReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Tooltip("ドリルループ音のリバーブ設定")]
    public ReverbSettings drillMotorLoopReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Tooltip("ドリル終了音のリバーブ設定")]
    public ReverbSettings drillMotorEndReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Header("鍵収集音声設定")]
    [Tooltip("鍵収集音")]
    public AudioClip keyCollectionSound;
    
    [Header("鍵収集音ボリューム設定")]
    [Tooltip("鍵収集音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float keyCollectionVolume = -1f;
    
    [Header("鍵収集音リバーブ設定")]
    [Tooltip("鍵収集音のリバーブ設定")]
    public ReverbSettings keyCollectionReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Header("お宝取得音声設定")]
    [Tooltip("お宝取得音")]
    public AudioClip treasureCollectionSound;
    
    [Header("お宝取得音ボリューム設定")]
    [Tooltip("お宝取得音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float treasureCollectionVolume = -1f;
    
    [Header("お宝取得音リバーブ設定")]
    [Tooltip("お宝取得音のリバーブ設定")]
    public ReverbSettings treasureCollectionReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Header("試練の部屋クリア音声設定")]
    [Tooltip("試練の部屋クリア音")]
    public AudioClip trialRoomClearSound;
    
    [Header("試練の部屋クリア音ボリューム設定")]
    [Tooltip("試練の部屋クリア音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float trialRoomClearVolume = -1f;
    
    [Header("試練の部屋クリア音リバーブ設定")]
    [Tooltip("試練の部屋クリア音のリバーブ設定")]
    public ReverbSettings trialRoomClearReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Header("ゴールドア開放音声設定")]
    [Tooltip("ゴールドア開放音")]
    public AudioClip goalDoorOpenSound;
    
    [Header("ゴールドア開放音ボリューム設定")]
    [Tooltip("ゴールドア開放音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float goalDoorOpenVolume = -1f;
    
    [Header("ゴールドア開放音リバーブ設定")]
    [Tooltip("ゴールドア開放音のリバーブ設定")]
    public ReverbSettings goalDoorOpenReverb = new ReverbSettings { enabled = true, preset = AudioReverbPreset.Cave };
    
    [Header("ゴールファンファーレ音声設定")]
    [Tooltip("ゴールファンファーレ音（ワープ後に再生）")]
    public AudioClip goalFanfareSound;
    
    [Header("ゴールファンファーレ音ボリューム設定")]
    [Tooltip("ゴールファンファーレ音のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float goalFanfareVolume = -1f;
    
    [Header("ゴールファンファーレ音リバーブ設定")]
    [Tooltip("ゴールファンファーレ音のリバーブ設定")]
    public ReverbSettings goalFanfareReverb = new ReverbSettings { enabled = false, preset = AudioReverbPreset.Cave };
    
    [Header("ジェットパック音声設定")]
    [Tooltip("ジェットパック音（ループ）")]
    public AudioClip jetpackSound;
    
    [Header("ジェットパックボリューム設定")]
    [Tooltip("ジェットパック通常時のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float jetpackIdleVolume = -1f;
    
    [Tooltip("ジェットパック起動時のボリューム（-1の場合はカテゴリ設定を使用、0-5で設定可能）")]
    [Range(-1f, 5f)]
    public float jetpackActiveVolume = -1f;
    
    [Header("ジェットパックリバーブ設定")]
    [Tooltip("ジェットパック音のリバーブ設定")]
    public ReverbSettings jetpackReverb = new ReverbSettings { enabled = false, preset = AudioReverbPreset.Cave };
    
    [Header("共通設定")]
    [Tooltip("音声の基本ボリューム（全音源のデフォルト、個別設定が-1の場合に使用、0-5で設定可能）")]
    [Range(0f, 5f)]
    public float baseVolume = 0.7f;
    
    [Tooltip("音声の基本ピッチ")]
    [Range(0.5f, 2f)]
    public float basePitch = 1f;
    
    [Tooltip("音声の空間化設定")]
    public bool useSpatialBlending = true;
    
    [Tooltip("音声の最大距離")]
    public float maxDistance = 50f;
    
    /// <summary>
    /// 音源タイプに応じたボリュームを取得（階層化：個別設定 → 共通設定）
    /// 個別設定が-1の場合はbaseVolumeをそのまま使用
    /// 個別設定が0以上の場合、個別設定 × baseVolume を返す
    /// </summary>
    public float GetVolumeForSoundType(string soundType)
    {
        float individualVolume = -1f;
        
        // 個別設定を取得
        switch (soundType)
        {
            case "PickaxeDig":
                individualVolume = pickaxeDigVolume;
                break;
            case "PickaxeExplosion":
            case "PickaxeExplosionMarker":
                individualVolume = pickaxeExplosionVolume;
                break;
            case "PickaxeModeSwitch":
                individualVolume = pickaxeModeSwitchVolume;
                break;
            case "DrillDig":
                individualVolume = drillDigVolume;
                break;
            case "DrillProjectile":
                individualVolume = drillProjectileVolume;
                break;
            case "DrillModeSwitch":
                individualVolume = drillModeSwitchVolume;
                break;
            case "HandDig":
                individualVolume = handDigVolume;
                break;
            case "Footstep":
                individualVolume = footstepVolume;
                break;
            case "TreasureProximity":
                individualVolume = treasureProximityVolume;
                break;
            case "KeyCollection":
                individualVolume = keyCollectionVolume;
                break;
            case "DrillMotorStart":
                individualVolume = drillMotorStartVolume;
                break;
            case "DrillMotorLoop":
                individualVolume = drillMotorLoopVolume;
                break;
            case "DrillMotorEnd":
                individualVolume = drillMotorEndVolume;
                break;
            case "TreasureCollection":
                individualVolume = treasureCollectionVolume;
                break;
            case "TrialRoomClear":
                individualVolume = trialRoomClearVolume;
                break;
            case "GoalDoorOpen":
                individualVolume = goalDoorOpenVolume;
                break;
            case "GoalFanfare":
                individualVolume = goalFanfareVolume;
                break;
            case "JetpackIdle":
                individualVolume = jetpackIdleVolume;
                break;
            case "JetpackActive":
                individualVolume = jetpackActiveVolume;
                break;
        }
        
        // 階層化：個別設定 → 共通設定
        if (individualVolume >= 0f)
        {
            return individualVolume * baseVolume;
        }
        else
        {
            return baseVolume;
        }
    }
}
