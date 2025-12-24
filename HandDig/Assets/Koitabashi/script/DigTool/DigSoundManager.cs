using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 掘削音声を管理するマネージャークラス
/// </summary>
public class DigSoundManager : MonoBehaviour
{
    [Header("音声設定")]
    [Tooltip("音声設定ファイル")]
    public DigSoundSettings soundSettings;
    
    [Tooltip("音声ソースのプールサイズ")]
    public int audioSourcePoolSize = 5;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを出力するか")]
    public bool enableDebugLog = true;

    private static DigSoundManager instance;
    private Queue<AudioSource> audioSourcePool;
    private List<AudioSource> activeAudioSources;
    
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static DigSoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DigSoundManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DigSoundManager");
                    instance = go.AddComponent<DigSoundManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSourcePool();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 設定ファイルが未設定の場合は自動で探す
        if (soundSettings == null)
        {
            soundSettings = Resources.Load<DigSoundSettings>("DigSoundSettings");
            if (soundSettings == null)
            {
                Debug.LogWarning("[DigSoundManager] DigSoundSettingsが見つかりません。音声機能が制限されます。");
            }
        }
    }

    /// <summary>
    /// 音声ソースプールを初期化
    /// </summary>
    private void InitializeAudioSourcePool()
    {
        audioSourcePool = new Queue<AudioSource>();
        activeAudioSources = new List<AudioSource>();

        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            CreateAudioSource();
        }
    }

    /// <summary>
    /// 新しい音声ソースを作成
    /// </summary>
    private void CreateAudioSource()
    {
        GameObject audioObj = new GameObject("AudioSource");
        audioObj.transform.SetParent(transform);
        
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = soundSettings != null && soundSettings.useSpatialBlending ? 1f : 0f;
        audioSource.maxDistance = soundSettings != null ? soundSettings.maxDistance : 50f;
        audioSource.volume = soundSettings != null ? soundSettings.baseVolume : 0.7f;
        audioSource.pitch = soundSettings != null ? soundSettings.basePitch : 1f;
        
        audioSourcePool.Enqueue(audioSource);
    }

    /// <summary>
    /// 音声ソースを取得（プールから）
    /// </summary>
    private AudioSource GetAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            AudioSource audioSource = audioSourcePool.Dequeue();
            activeAudioSources.Add(audioSource);
            return audioSource;
        }
        else
        {
            // プールが空の場合は新しいものを作成
            CreateAudioSource();
            AudioSource audioSource = audioSourcePool.Dequeue();
            activeAudioSources.Add(audioSource);
            return audioSource;
        }
    }

    /// <summary>
    /// 音声ソースをプールに戻す
    /// </summary>
    private void ReturnAudioSource(AudioSource audioSource)
    {
        if (activeAudioSources.Contains(audioSource))
        {
            activeAudioSources.Remove(audioSource);
            audioSourcePool.Enqueue(audioSource);
        }
    }

    /// <summary>
    /// つるはし掘削音を再生
    /// </summary>
    /// <param name="comboStage">コンボ段階（0-2）</param>
    /// <param name="position">再生位置</param>
    public void PlayPickaxeDigSound(int comboStage, Vector3 position)
    {
        if (soundSettings == null || soundSettings.pickaxeDigSounds == null) return;
        
        int index = Mathf.Clamp(comboStage, 0, soundSettings.pickaxeDigSounds.Length - 1);
        AudioClip clip = soundSettings.pickaxeDigSounds[index];
        
        if (clip != null)
        {
            PlaySoundAtPosition(clip, position, "PickaxeDig");
        }
        else if (enableDebugLog)
        {
            Debug.LogWarning($"[DigSoundManager] つるはし掘削音（コンボ{comboStage + 1}）が設定されていません。");
        }
    }

    /// <summary>
    /// つるはし爆発マーカー設置音を再生
    /// </summary>
    /// <param name="position">再生位置</param>
    public void PlayPickaxeExplosionMarkerSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.pickaxeExplosionMarkerSound == null) return;
        
        PlaySoundAtPosition(soundSettings.pickaxeExplosionMarkerSound, position, "PickaxeExplosionMarker");
    }

    /// <summary>
    /// つるはし爆発音を再生
    /// </summary>
    /// <param name="position">再生位置</param>
    public void PlayPickaxeExplosionSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.pickaxeExplosionSound == null) return;
        
        PlaySoundAtPosition(soundSettings.pickaxeExplosionSound, position, "PickaxeExplosion");
    }

    /// <summary>
    /// ドリル掘削音を再生
    /// </summary>
    /// <param name="position">再生位置</param>
    public void PlayDrillDigSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.drillDigSound == null) return;
        
        PlaySoundAtPosition(soundSettings.drillDigSound, position, "DrillDig");
    }

    /// <summary>
    /// 射出ドリル音を再生
    /// </summary>
    /// <param name="position">再生位置</param>
    public void PlayDrillProjectileSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.drillProjectileSound == null) return;
        
        PlaySoundAtPosition(soundSettings.drillProjectileSound, position, "DrillProjectile");
    }

    /// <summary>
    /// 手掘り音を再生
    /// </summary>
    /// <param name="position">再生位置</param>
    public void PlayHandDigSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.handDigSound == null) return;
        
        PlaySoundAtPosition(soundSettings.handDigSound, position, "HandDig");
    }

    /// <summary>
    /// お宝接近音を再生
    /// </summary>
    /// <param name="position">再生位置</param>
    public void PlayTreasureProximitySound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.treasureProximitySound == null) return;
        
        PlaySoundAtPosition(soundSettings.treasureProximitySound, position, "TreasureProximity");
    }

    /// <summary>
    /// 指定位置で音声を再生
    /// </summary>
    /// <param name="clip">再生する音声クリップ</param>
    /// <param name="position">再生位置</param>
    /// <param name="soundType">音声タイプ（デバッグ用）</param>
    private void PlaySoundAtPosition(AudioClip clip, Vector3 position, string soundType)
    {
        AudioSource audioSource = GetAudioSource();
        if (audioSource == null) return;

        audioSource.clip = clip;
        audioSource.transform.position = position;
        
        // 設定を適用
        if (soundSettings != null)
        {
            audioSource.volume = soundSettings.baseVolume;
            audioSource.pitch = soundSettings.basePitch;
            audioSource.spatialBlend = soundSettings.useSpatialBlending ? 1f : 0f;
            audioSource.maxDistance = soundSettings.maxDistance;
        }

        audioSource.Play();
        
        if (enableDebugLog)
        {
            Debug.Log($"[DigSoundManager] {soundType}音声再生: {clip.name} at {position}");
        }

        // 再生完了後にプールに戻す
        StartCoroutine(ReturnAudioSourceWhenFinished(audioSource));
    }

    /// <summary>
    /// 音声再生完了後にプールに戻す
    /// </summary>
    private IEnumerator ReturnAudioSourceWhenFinished(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        ReturnAudioSource(audioSource);
    }

    /// <summary>
    /// 全音声を停止
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var audioSource in activeAudioSources)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                ReturnAudioSource(audioSource);
            }
        }
    }

    /// <summary>
    /// 音声設定を更新
    /// </summary>
    public void UpdateSoundSettings(DigSoundSettings newSettings)
    {
        soundSettings = newSettings;
        
        // 既存の音声ソースに設定を適用
        foreach (var audioSource in activeAudioSources)
        {
            if (audioSource != null && soundSettings != null)
            {
                audioSource.volume = soundSettings.baseVolume;
                audioSource.pitch = soundSettings.basePitch;
                audioSource.spatialBlend = soundSettings.useSpatialBlending ? 1f : 0f;
                audioSource.maxDistance = soundSettings.maxDistance;
            }
        }
        
        Debug.Log("[DigSoundManager] 音声設定を更新しました。");
    }
}




