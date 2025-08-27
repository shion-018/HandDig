using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigSoundManager : MonoBehaviour
{
    [Header("つるはしの音")]
    [Tooltip("つるはしの通常掘削音")]
    public AudioClip pickaxeDigSound;
    [Tooltip("つるはしのコンボ掘削音（段階1）")]
    public AudioClip pickaxeCombo1Sound;
    [Tooltip("つるはしのコンボ掘削音（段階2）")]
    public AudioClip pickaxeCombo2Sound;
    [Tooltip("つるはしのコンボ掘削音（段階3）")]
    public AudioClip pickaxeCombo3Sound;
    [Tooltip("つるはしの爆発マーカー設置音")]
    public AudioClip pickaxeExplosionMarkerSound;
    [Tooltip("つるはしの爆発音")]
    public AudioClip pickaxeExplosionSound;

    [Header("ドリルの音")]
    [Tooltip("ドリルの掘削音")]
    public AudioClip drillDigSound;

    [Header("手掘りの音")]
    [Tooltip("手掘りの音")]
    public AudioClip handDigSound;

    [Header("音声設定")]
    [Tooltip("音の音量（0-1）")]
    [Range(0f, 1f)]
    public float volume = 0.7f;
    [Tooltip("音のピッチ変動範囲")]
    [Range(0f, 0.5f)]
    public float pitchVariation = 0.1f;

    private AudioSource audioSource;

    void Start()
    {
        // AudioSourceコンポーネントを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // AudioSourceの初期設定
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1f; // 3D音声
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 20f;
    }

    /// <summary>
    /// つるはしの掘削音を再生
    /// </summary>
    /// <param name="comboStage">コンボ段階（0-2）</param>
    /// <param name="position">音を再生する位置</param>
    public void PlayPickaxeDigSound(int comboStage, Vector3 position)
    {
        AudioClip clipToPlay = pickaxeDigSound; // デフォルト

        // コンボ段階に応じて音を選択
        switch (comboStage)
        {
            case 0:
                clipToPlay = pickaxeDigSound;
                break;
            case 1:
                clipToPlay = pickaxeCombo1Sound != null ? pickaxeCombo1Sound : pickaxeDigSound;
                break;
            case 2:
                clipToPlay = pickaxeCombo2Sound != null ? pickaxeCombo2Sound : pickaxeDigSound;
                break;
            case 3:
                clipToPlay = pickaxeCombo3Sound != null ? pickaxeCombo3Sound : pickaxeDigSound;
                break;
        }

        PlaySoundAtPosition(clipToPlay, position);
    }

    /// <summary>
    /// つるはしの爆発マーカー設置音を再生
    /// </summary>
    /// <param name="position">音を再生する位置</param>
    public void PlayPickaxeExplosionMarkerSound(Vector3 position)
    {
        PlaySoundAtPosition(pickaxeExplosionMarkerSound, position);
    }

    /// <summary>
    /// つるはしの爆発音を再生
    /// </summary>
    /// <param name="position">音を再生する位置</param>
    public void PlayPickaxeExplosionSound(Vector3 position)
    {
        PlaySoundAtPosition(pickaxeExplosionSound, position);
    }

    /// <summary>
    /// ドリルの掘削音を再生
    /// </summary>
    /// <param name="position">音を再生する位置</param>
    public void PlayDrillDigSound(Vector3 position)
    {
        PlaySoundAtPosition(drillDigSound, position);
    }

    /// <summary>
    /// 手掘りの音を再生
    /// </summary>
    /// <param name="position">音を再生する位置</param>
    public void PlayHandDigSound(Vector3 position)
    {
        PlaySoundAtPosition(handDigSound, position);
    }

    /// <summary>
    /// 指定位置で音を再生
    /// </summary>
    /// <param name="clip">再生する音声クリップ</param>
    /// <param name="position">再生位置</param>
    private void PlaySoundAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null)
        {
            Debug.LogWarning("[DigSoundManager] 音声クリップが設定されていません");
            return;
        }

        // 一時的なAudioSourceを作成して3D音声を再生
        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = position;
        
        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.spatialBlend = 1f; // 3D音声
        tempSource.rolloffMode = AudioRolloffMode.Linear;
        tempSource.maxDistance = 20f;
        
        // ピッチ変動を追加
        tempSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        
        tempSource.Play();
        
        // 音声再生後にオブジェクトを削除
        Destroy(tempAudio, clip.length + 0.1f);
        
        Debug.Log($"[DigSoundManager] 音声再生: {clip.name} at {position}");
    }

    /// <summary>
    /// 音量を設定
    /// </summary>
    /// <param name="newVolume">新しい音量（0-1）</param>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// 現在の音量を取得
    /// </summary>
    /// <returns>現在の音量</returns>
    public float GetVolume()
    {
        return volume;
    }
}
