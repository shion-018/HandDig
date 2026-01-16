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
    
    // Transform追従型のAudioSource管理
    private Dictionary<Transform, AudioSource> attachedAudioSources = new Dictionary<Transform, AudioSource>();
    private Dictionary<AudioSource, Transform> audioSourceToTransform = new Dictionary<AudioSource, Transform>();
    
    // ドリル掘削音の再生間隔制限
    private float lastDrillDigSoundTime = 0f;
    
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
            // エディタモードではDontDestroyOnLoadを無効化（シーンを閉じたときにクリーンアップされるように）
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
            InitializeAudioSourcePool();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // インスタンスが自分自身の場合、クリーンアップ
        if (instance == this)
        {
            // すべての音声を停止
            StopAllSounds();
            
            // 追従型のAudioSourceをクリーンアップ
            foreach (var audioSource in attachedAudioSources.Values)
            {
                if (audioSource != null && audioSource.gameObject != null)
                {
                    Destroy(audioSource.gameObject);
                }
            }
            attachedAudioSources.Clear();
            audioSourceToTransform.Clear();
            
            // インスタンスをクリア
            instance = null;
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
        // プールが初期化されていない場合は初期化
        if (audioSourcePool == null)
        {
            InitializeAudioSourcePool();
        }
        
        GameObject audioObj = new GameObject("AudioSource");
        audioObj.transform.SetParent(transform);
        
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = soundSettings != null && soundSettings.useSpatialBlending ? 1f : 0f;
        audioSource.maxDistance = soundSettings != null ? soundSettings.maxDistance : 50f;
        audioSource.volume = soundSettings != null ? soundSettings.baseVolume : 0.7f;
        audioSource.pitch = soundSettings != null ? soundSettings.basePitch : 1f;
        audioSource.dopplerLevel = 0f; // ドップラー効果を無効化
        
        audioSourcePool.Enqueue(audioSource);
    }

    /// <summary>
    /// 音声ソースを取得（プールから）
    /// </summary>
    private AudioSource GetAudioSource()
    {
        // プールが初期化されていない場合は初期化
        if (audioSourcePool == null)
        {
            InitializeAudioSourcePool();
        }
        
        if (audioSourcePool.Count > 0)
        {
            AudioSource audioSource = audioSourcePool.Dequeue();
            if (activeAudioSources == null)
            {
                activeAudioSources = new List<AudioSource>();
            }
            activeAudioSources.Add(audioSource);
            return audioSource;
        }
        else
        {
            // プールが空の場合は新しいものを作成
            CreateAudioSource();
            AudioSource audioSource = audioSourcePool.Dequeue();
            if (activeAudioSources == null)
            {
                activeAudioSources = new List<AudioSource>();
            }
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
            ReverbSettings reverb = soundSettings != null ? soundSettings.pickaxeDigReverb : null;
            PlaySoundAtPosition(clip, position, "PickaxeDig", reverb);
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
        
        // マーカー設置音は爆発音と同じリバーブ設定を使用
        ReverbSettings reverb = soundSettings != null ? soundSettings.pickaxeExplosionReverb : null;
        PlaySoundAtPosition(soundSettings.pickaxeExplosionMarkerSound, position, "PickaxeExplosionMarker", reverb);
    }

    /// <summary>
    /// つるはし爆発音を再生
    /// </summary>
    /// <param name="position">再生位置</param>
    public void PlayPickaxeExplosionSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.pickaxeExplosionSound == null) return;
        
        ReverbSettings reverb = soundSettings != null ? soundSettings.pickaxeExplosionReverb : null;
        PlaySoundAtPosition(soundSettings.pickaxeExplosionSound, position, "PickaxeExplosion", reverb);
    }

    /// <summary>
    /// ドリル掘削音を再生（再生間隔制限付き）
    /// </summary>
    /// <param name="position">再生位置</param>
    public void PlayDrillDigSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.drillDigSound == null) return;
        
        // 再生間隔制限：一定時間以内の連続再生を防ぐ
        float cooldown = soundSettings.drillDigSoundCooldown;
        float currentTime = Time.time;
        if (currentTime - lastDrillDigSoundTime < cooldown)
        {
            return; // クールダウン中は再生しない
        }
        
        lastDrillDigSoundTime = currentTime;
        
        ReverbSettings reverb = soundSettings != null ? soundSettings.drillDigReverb : null;
        PlaySoundAtPosition(soundSettings.drillDigSound, position, "DrillDig", reverb);
    }

    /// <summary>
    /// 射出ドリル音を再生
    /// </summary>
    /// <param name="position">再生位置</param>
    public void PlayDrillProjectileSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.drillProjectileSound == null) return;
        
        ReverbSettings reverb = soundSettings != null ? soundSettings.drillProjectileReverb : null;
        PlaySoundAtPosition(soundSettings.drillProjectileSound, position, "DrillProjectile", reverb);
    }

    /// <summary>
    /// 手掘り音を再生
    /// </summary>
    /// <param name="position">再生位置</param>
    public void PlayHandDigSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.handDigSound == null) return;
        
        ReverbSettings reverb = soundSettings != null ? soundSettings.handDigReverb : null;
        PlaySoundAtPosition(soundSettings.handDigSound, position, "HandDig", reverb);
    }

    /// <summary>
    /// お宝接近音を再生
    /// </summary>
    /// <param name="position">再生位置</param>
    public void PlayTreasureProximitySound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.treasureProximitySound == null) return;
        
        ReverbSettings reverb = soundSettings != null ? soundSettings.treasureProximityReverb : null;
        PlaySoundAtPosition(soundSettings.treasureProximitySound, position, "TreasureProximity", reverb);
    }

    /// <summary>
    /// 足音を再生
    /// </summary>
    /// <param name="position">再生位置（プレイヤーの位置）</param>
    public void PlayFootstepSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.footstepSounds == null || soundSettings.footstepSounds.Length == 0) return;
        
        // 有効な音源をフィルタリング
        List<AudioClip> validClips = new List<AudioClip>();
        foreach (var clip in soundSettings.footstepSounds)
        {
            if (clip != null)
            {
                validClips.Add(clip);
            }
        }
        
        if (validClips.Count == 0) return;
        
        // ランダムに音源を選択
        AudioClip selectedClip = validClips[Random.Range(0, validClips.Count)];
        
        AudioSource audioSource = GetAudioSource();
        if (audioSource == null) return;

        audioSource.clip = selectedClip;
        audioSource.transform.position = position;
        
        // 足音専用の設定を適用（階層化されたボリューム設定を使用）
        audioSource.volume = soundSettings.GetVolumeForSoundType("Footstep");
        
        // ピッチにランダム性を追加（自然な変化）
        float pitchVariation = Random.Range(-soundSettings.footstepPitchRandomness, soundSettings.footstepPitchRandomness);
        audioSource.pitch = soundSettings.basePitch + pitchVariation;
        
        audioSource.spatialBlend = soundSettings.useSpatialBlending ? 1f : 0f;
        audioSource.maxDistance = soundSettings.maxDistance;
        
        // ドップラー効果を無効化
        audioSource.dopplerLevel = 0f;

        // リバーブエフェクトを適用（洞窟の反響効果）
        if (soundSettings != null && soundSettings.footstepReverb != null && soundSettings.footstepReverb.enabled)
        {
            ApplyReverb(audioSource.gameObject, soundSettings.footstepReverb);
        }

        audioSource.Play();
        
        if (enableDebugLog)
        {
            Debug.Log($"[DigSoundManager] 足音再生: {selectedClip.name} at {position} (ピッチ: {audioSource.pitch:F2})");
        }

        // 再生完了後にプールに戻す
        StartCoroutine(ReturnAudioSourceWhenFinished(audioSource));
    }

    /// <summary>
    /// 指定位置で音声を再生
    /// </summary>
    /// <param name="clip">再生する音声クリップ</param>
    /// <param name="position">再生位置</param>
    /// <param name="soundType">音声タイプ（デバッグ用）</param>
    private void PlaySoundAtPosition(AudioClip clip, Vector3 position, string soundType)
    {
        PlaySoundAtPosition(clip, position, soundType, null);
    }
    
    /// <summary>
    /// 指定位置で音声を再生（リバーブ設定付き）
    /// </summary>
    /// <param name="clip">再生する音声クリップ</param>
    /// <param name="position">再生位置</param>
    /// <param name="soundType">音声タイプ（デバッグ用）</param>
    /// <param name="reverbSettings">リバーブ設定（nullの場合は適用しない）</param>
    /// <param name="fadeInDuration">フェードイン時間（秒、0の場合はフェードインなし）</param>
    private void PlaySoundAtPosition(AudioClip clip, Vector3 position, string soundType, ReverbSettings reverbSettings, float fadeInDuration = 0f)
    {
        AudioSource audioSource = GetAudioSource();
        if (audioSource == null) return;

        audioSource.clip = clip;
        audioSource.transform.position = position;
        
        // 設定を適用（階層化されたボリューム設定を使用）
        if (soundSettings != null)
        {
            audioSource.volume = soundSettings.GetVolumeForSoundType(soundType);
            audioSource.pitch = soundSettings.basePitch;
            audioSource.spatialBlend = soundSettings.useSpatialBlending ? 1f : 0f;
            audioSource.maxDistance = soundSettings.maxDistance;
        }
        
        // ドップラー効果を無効化
        audioSource.dopplerLevel = 0f;

        // リバーブエフェクトを適用
        if (reverbSettings != null && reverbSettings.enabled)
        {
            ApplyReverb(audioSource.gameObject, reverbSettings);
        }

        // フェードイン処理
        if (fadeInDuration > 0f)
        {
            float targetVolume = audioSource.volume;
            audioSource.volume = 0f;
            audioSource.Play();
            StartCoroutine(FadeInAudioSource(audioSource, targetVolume, fadeInDuration));
        }
        else
        {
            audioSource.Play();
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[DigSoundManager] {soundType}音声再生: {clip.name} at {position} (ボリューム: {audioSource.volume:F2})");
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
    /// AudioSourceをフェードイン
    /// </summary>
    private IEnumerator FadeInAudioSource(AudioSource audioSource, float targetVolume, float duration)
    {
        float elapsed = 0f;
        float startVolume = 0f;
        
        while (elapsed < duration && audioSource != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        
        if (audioSource != null)
        {
            audioSource.volume = targetVolume;
        }
    }
    
    /// <summary>
    /// AudioSourceをフェードアウト
    /// </summary>
    /// <param name="audioSource">フェードアウトするAudioSource</param>
    /// <param name="duration">フェードアウト時間（秒）</param>
    /// <param name="stopAfterFade">フェードアウト後に停止するか</param>
    public void FadeOutAudioSource(AudioSource audioSource, float duration, bool stopAfterFade = true)
    {
        if (audioSource == null || !audioSource.isPlaying) return;
        
        StartCoroutine(FadeOutAudioSourceCoroutine(audioSource, duration, stopAfterFade));
    }
    
    /// <summary>
    /// AudioSourceをフェードアウトするコルーチン
    /// </summary>
    private IEnumerator FadeOutAudioSourceCoroutine(AudioSource audioSource, float duration, bool stopAfterFade)
    {
        float elapsed = 0f;
        float startVolume = audioSource.volume;
        
        while (elapsed < duration && audioSource != null && audioSource.isPlaying)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }
        
        if (audioSource != null)
        {
            audioSource.volume = 0f;
            if (stopAfterFade)
            {
                audioSource.Stop();
            }
        }
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
    /// リバーブエフェクトを適用
    /// </summary>
    /// <param name="audioGameObject">AudioSourceがアタッチされているGameObject</param>
    /// <param name="reverbSettings">リバーブ設定</param>
    private void ApplyReverb(GameObject audioGameObject, ReverbSettings reverbSettings)
    {
        if (reverbSettings == null || !reverbSettings.enabled) return;
        
        // AudioReverbFilterコンポーネントを取得または追加
        AudioReverbFilter reverbFilter = audioGameObject.GetComponent<AudioReverbFilter>();
        if (reverbFilter == null)
        {
            reverbFilter = audioGameObject.AddComponent<AudioReverbFilter>();
        }
        
        // プリセットを使用する場合
        if (reverbSettings.preset != AudioReverbPreset.User)
        {
            reverbFilter.reverbPreset = reverbSettings.preset;
        }
        else
        {
            // カスタム設定の場合
            reverbFilter.reverbPreset = AudioReverbPreset.User;
            reverbFilter.reverbLevel = Mathf.RoundToInt(reverbSettings.reverbLevel * 1000f); // -10000 to 2000
            reverbFilter.decayTime = reverbSettings.decayTime;
            // その他のパラメータはデフォルト値を使用
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[DigSoundManager] リバーブ適用: {reverbSettings.preset}");
        }
    }

    /// <summary>
    /// 音声設定を更新
    /// </summary>
    public void UpdateSoundSettings(DigSoundSettings newSettings)
    {
        soundSettings = newSettings;
        
        // 既存の音声ソースに設定を適用（注意：個別の音源タイプが不明なため、baseVolumeを使用）
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
        
        // 追従型のAudioSourceにも設定を適用（注意：個別の音源タイプが不明なため、baseVolumeを使用）
        foreach (var audioSource in attachedAudioSources.Values)
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
    
    /// <summary>
    /// Transformに追従する音声を再生（位置固定型）
    /// </summary>
    /// <param name="clip">再生する音声クリップ</param>
    /// <param name="targetTransform">追従するTransform</param>
    /// <param name="soundType">音声タイプ（デバッグ用）</param>
    /// <param name="reverbSettings">リバーブ設定</param>
    /// <param name="volume">ボリューム（nullの場合は基本ボリューム）</param>
    /// <param name="pitch">ピッチ（nullの場合は基本ピッチ）</param>
    /// <returns>再生中のAudioSource（停止時に使用）</returns>
    public AudioSource PlaySoundAtTransform(AudioClip clip, Transform targetTransform, string soundType = "Sound", ReverbSettings reverbSettings = null, float? volume = null, float? pitch = null)
    {
        if (clip == null || targetTransform == null) return null;
        
        // 既にこのTransformにAudioSourceがアタッチされている場合は再利用
        AudioSource audioSource;
        if (attachedAudioSources.ContainsKey(targetTransform))
        {
            audioSource = attachedAudioSources[targetTransform];
            // 既に再生中の場合は停止
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        else
        {
            // 新しいAudioSourceを作成してTransformにアタッチ
            GameObject audioObj = new GameObject($"AudioSource_{soundType}");
            audioObj.transform.SetParent(targetTransform);
            audioObj.transform.localPosition = Vector3.zero;
            audioObj.transform.localRotation = Quaternion.identity;
            
            audioSource = audioObj.AddComponent<AudioSource>();
            attachedAudioSources[targetTransform] = audioSource;
            audioSourceToTransform[audioSource] = targetTransform;
        }
        
        // 設定を適用（階層化されたボリューム設定を使用）
        audioSource.clip = clip;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        if (soundSettings != null)
        {
            // 個別ボリュームが指定されている場合はそれを使用、そうでなければ階層化された設定を使用
            audioSource.volume = volume ?? soundSettings.GetVolumeForSoundType(soundType);
            audioSource.pitch = pitch ?? soundSettings.basePitch;
            audioSource.spatialBlend = soundSettings.useSpatialBlending ? 1f : 0f;
            audioSource.maxDistance = soundSettings.maxDistance;
        }
        else
        {
            audioSource.volume = volume ?? 0.7f;
            audioSource.pitch = pitch ?? 1f;
        }
        
        // ドップラー効果を無効化
        audioSource.dopplerLevel = 0f;
        
        // リバーブエフェクトを適用
        if (reverbSettings != null && reverbSettings.enabled)
        {
            ApplyReverb(audioSource.gameObject, reverbSettings);
        }
        
        audioSource.Play();
        
        if (enableDebugLog)
        {
            Debug.Log($"[DigSoundManager] {soundType}音声再生（追従型）: {clip.name} at {targetTransform.name}");
        }
        
        // 再生完了後にクリーンアップ
        StartCoroutine(CleanupAttachedAudioSourceWhenFinished(audioSource, clip.length));
        
        return audioSource;
    }
    
    /// <summary>
    /// Transformに追従するループ音声を再生
    /// </summary>
    /// <param name="clip">再生する音声クリップ</param>
    /// <param name="targetTransform">追従するTransform</param>
    /// <param name="soundType">音声タイプ（デバッグ用）</param>
    /// <param name="reverbSettings">リバーブ設定</param>
    /// <param name="volume">ボリューム（nullの場合は基本ボリューム）</param>
    /// <param name="pitch">ピッチ（nullの場合は基本ピッチ）</param>
    /// <returns>再生中のAudioSource（停止時に使用）</returns>
    public AudioSource PlaySoundAtTransformLoop(AudioClip clip, Transform targetTransform, string soundType = "Sound", ReverbSettings reverbSettings = null, float? volume = null, float? pitch = null)
    {
        if (clip == null || targetTransform == null) return null;
        
        // 既にこのTransformにAudioSourceがアタッチされている場合は再利用
        AudioSource audioSource;
        if (attachedAudioSources.ContainsKey(targetTransform))
        {
            audioSource = attachedAudioSources[targetTransform];
            // 既に再生中の場合は停止
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        else
        {
            // 新しいAudioSourceを作成してTransformにアタッチ
            GameObject audioObj = new GameObject($"AudioSource_{soundType}_Loop");
            audioObj.transform.SetParent(targetTransform);
            audioObj.transform.localPosition = Vector3.zero;
            audioObj.transform.localRotation = Quaternion.identity;
            
            audioSource = audioObj.AddComponent<AudioSource>();
            attachedAudioSources[targetTransform] = audioSource;
            audioSourceToTransform[audioSource] = targetTransform;
        }
        
        // 設定を適用（階層化されたボリューム設定を使用）
        audioSource.clip = clip;
        audioSource.playOnAwake = false;
        audioSource.loop = true; // ループ有効
        
        if (soundSettings != null)
        {
            // 個別ボリュームが指定されている場合はそれを使用、そうでなければ階層化された設定を使用
            audioSource.volume = volume ?? soundSettings.GetVolumeForSoundType(soundType);
            audioSource.pitch = pitch ?? soundSettings.basePitch;
            audioSource.spatialBlend = soundSettings.useSpatialBlending ? 1f : 0f;
            audioSource.maxDistance = soundSettings.maxDistance;
        }
        else
        {
            audioSource.volume = volume ?? 0.7f;
            audioSource.pitch = pitch ?? 1f;
        }
        
        // ドップラー効果を無効化
        audioSource.dopplerLevel = 0f;
        
        // リバーブエフェクトを適用
        if (reverbSettings != null && reverbSettings.enabled)
        {
            ApplyReverb(audioSource.gameObject, reverbSettings);
        }
        
        audioSource.Play();
        
        if (enableDebugLog)
        {
            Debug.Log($"[DigSoundManager] {soundType}音声再生（追従型・ループ）: {clip.name} at {targetTransform.name}");
        }
        
        return audioSource;
    }
    
    /// <summary>
    /// Transformに追従している音声を停止
    /// </summary>
    /// <param name="targetTransform">停止するTransform</param>
    public void StopSoundAtTransform(Transform targetTransform)
    {
        if (targetTransform == null || !attachedAudioSources.ContainsKey(targetTransform)) return;
        
        AudioSource audioSource = attachedAudioSources[targetTransform];
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    /// <summary>
    /// Transformに追従しているAudioSourceを削除
    /// </summary>
    /// <param name="targetTransform">削除するTransform</param>
    public void RemoveAttachedAudioSource(Transform targetTransform)
    {
        if (targetTransform == null || !attachedAudioSources.ContainsKey(targetTransform)) return;
        
        AudioSource audioSource = attachedAudioSources[targetTransform];
        if (audioSource != null)
        {
            audioSource.Stop();
            if (audioSource.gameObject != null)
            {
                Destroy(audioSource.gameObject);
            }
        }
        
        attachedAudioSources.Remove(targetTransform);
        if (audioSource != null)
        {
            audioSourceToTransform.Remove(audioSource);
        }
    }
    
    /// <summary>
    /// 追従型AudioSourceの再生完了後にクリーンアップ
    /// </summary>
    private IEnumerator CleanupAttachedAudioSourceWhenFinished(AudioSource audioSource, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (audioSource != null && !audioSource.loop && !audioSource.isPlaying)
        {
            if (audioSourceToTransform.ContainsKey(audioSource))
            {
                Transform targetTransform = audioSourceToTransform[audioSource];
                RemoveAttachedAudioSource(targetTransform);
            }
        }
    }
    
    // ========== ドリルモーター音の再生メソッド ==========
    
    /// <summary>
    /// ドリル開始音を再生（Transform追従型）
    /// </summary>
    /// <param name="targetTransform">追従するTransform</param>
    /// <returns>再生中のAudioSource</returns>
    public AudioSource PlayDrillMotorStartSound(Transform targetTransform)
    {
        if (soundSettings == null || soundSettings.drillMotorStartSound == null) return null;
        
        ReverbSettings reverb = soundSettings.drillMotorStartReverb;
        return PlaySoundAtTransform(soundSettings.drillMotorStartSound, targetTransform, "DrillMotorStart", reverb);
    }
    
    /// <summary>
    /// ドリルループ音を再生（Transform追従型・ループ）
    /// </summary>
    /// <param name="targetTransform">追従するTransform</param>
    /// <returns>再生中のAudioSource</returns>
    public AudioSource PlayDrillMotorLoopSound(Transform targetTransform)
    {
        if (soundSettings == null || soundSettings.drillMotorLoopSound == null) return null;
        
        ReverbSettings reverb = soundSettings.drillMotorLoopReverb;
        return PlaySoundAtTransformLoop(soundSettings.drillMotorLoopSound, targetTransform, "DrillMotorLoop", reverb);
    }
    
    /// <summary>
    /// ドリル終了音を再生（Transform追従型）
    /// </summary>
    /// <param name="targetTransform">追従するTransform</param>
    /// <returns>再生中のAudioSource</returns>
    public AudioSource PlayDrillMotorEndSound(Transform targetTransform)
    {
        if (soundSettings == null || soundSettings.drillMotorEndSound == null) return null;
        
        ReverbSettings reverb = soundSettings.drillMotorEndReverb;
        return PlaySoundAtTransform(soundSettings.drillMotorEndSound, targetTransform, "DrillMotorEnd", reverb);
    }
    
    /// <summary>
    /// ドリルモーター音を全て停止
    /// </summary>
    /// <param name="targetTransform">停止するTransform</param>
    public void StopDrillMotorSounds(Transform targetTransform)
    {
        StopSoundAtTransform(targetTransform);
    }
    
    // ========== 鍵収集音の再生メソッド ==========
    
    /// <summary>
    /// 鍵収集音を再生（Transform追従型）
    /// </summary>
    /// <param name="targetTransform">追従するTransform</param>
    /// <returns>再生中のAudioSource</returns>
    public AudioSource PlayKeyCollectionSound(Transform targetTransform)
    {
        if (soundSettings == null || soundSettings.keyCollectionSound == null) return null;
        
        ReverbSettings reverb = soundSettings.keyCollectionReverb;
        return PlaySoundAtTransform(soundSettings.keyCollectionSound, targetTransform, "KeyCollection", reverb);
    }
    
    // ========== お宝取得音の再生メソッド ==========
    
    /// <summary>
    /// お宝取得音を再生（位置指定型）
    /// </summary>
    /// <param name="position">再生位置（お宝の位置）</param>
    public void PlayTreasureCollectionSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.treasureCollectionSound == null) return;
        
        ReverbSettings reverb = soundSettings.treasureCollectionReverb;
        PlaySoundAtPosition(soundSettings.treasureCollectionSound, position, "TreasureCollection", reverb);
    }
    
    // ========== 試練の部屋クリア音の再生メソッド ==========
    
    /// <summary>
    /// 試練の部屋クリア音を再生（位置指定型）
    /// </summary>
    /// <param name="position">再生位置（ドアの位置）</param>
    public void PlayTrialRoomClearSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.trialRoomClearSound == null) return;
        
        ReverbSettings reverb = soundSettings.trialRoomClearReverb;
        PlaySoundAtPosition(soundSettings.trialRoomClearSound, position, "TrialRoomClear", reverb);
    }
    
    // ========== ゴールドア開放音の再生メソッド ==========
    
    /// <summary>
    /// ゴールドア開放音を再生（位置指定型）
    /// </summary>
    /// <param name="position">再生位置（ドアの位置）</param>
    public void PlayGoalDoorOpenSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.goalDoorOpenSound == null) return;
        
        ReverbSettings reverb = soundSettings.goalDoorOpenReverb;
        PlaySoundAtPosition(soundSettings.goalDoorOpenSound, position, "GoalDoorOpen", reverb);
    }
    
    // ========== ゴールファンファーレ音の再生メソッド ==========
    
    /// <summary>
    /// ゴールファンファーレ音を再生（位置指定型、ワープ後に再生）
    /// </summary>
    /// <param name="position">再生位置（プレイヤーの位置）</param>
    public void PlayGoalFanfareSound(Vector3 position)
    {
        if (soundSettings == null || soundSettings.goalFanfareSound == null) return;
        
        ReverbSettings reverb = soundSettings.goalFanfareReverb;
        PlaySoundAtPosition(soundSettings.goalFanfareSound, position, "GoalFanfare", reverb);
    }
    
    // ========== ジェットパック音の再生メソッド ==========
    
    /// <summary>
    /// ジェットパック音を再生（Transform追従型・ループ）
    /// </summary>
    /// <param name="targetTransform">追従するTransform</param>
    /// <param name="isActive">ジェットパック起動中かどうか（trueで音量を上げる）</param>
    /// <returns>再生中のAudioSource（音量変更時に使用）</returns>
    public AudioSource PlayJetpackSound(Transform targetTransform, bool isActive = false)
    {
        if (soundSettings == null || soundSettings.jetpackSound == null) return null;
        
        // 既にこのTransformにAudioSourceがアタッチされている場合は再利用
        AudioSource audioSource;
        if (attachedAudioSources.ContainsKey(targetTransform))
        {
            audioSource = attachedAudioSources[targetTransform];
        }
        else
        {
            // 新しいAudioSourceを作成してTransformにアタッチ
            GameObject audioObj = new GameObject("AudioSource_Jetpack_Loop");
            audioObj.transform.SetParent(targetTransform);
            audioObj.transform.localPosition = Vector3.zero;
            audioObj.transform.localRotation = Quaternion.identity;
            
            audioSource = audioObj.AddComponent<AudioSource>();
            attachedAudioSources[targetTransform] = audioSource;
            audioSourceToTransform[audioSource] = targetTransform;
            
            // 設定を適用
            audioSource.clip = soundSettings.jetpackSound;
            audioSource.playOnAwake = false;
            audioSource.loop = true; // ループ有効
            
            if (soundSettings != null)
            {
                audioSource.pitch = soundSettings.basePitch;
                audioSource.spatialBlend = soundSettings.useSpatialBlending ? 1f : 0f;
                audioSource.maxDistance = soundSettings.maxDistance;
            }
            
            // ドップラー効果を無効化
            audioSource.dopplerLevel = 0f;
            
            // リバーブエフェクトを適用
            ReverbSettings reverb = soundSettings.jetpackReverb;
            if (reverb != null && reverb.enabled)
            {
                ApplyReverb(audioSource.gameObject, reverb);
            }
            
            audioSource.Play();
        }
        
        // 音量を設定（起動中かどうかで切り替え）
        UpdateJetpackVolume(audioSource, isActive);
        
        if (enableDebugLog)
        {
            Debug.Log($"[DigSoundManager] ジェットパック音声再生（追従型・ループ）: {soundSettings.jetpackSound.name} at {targetTransform.name} (Active: {isActive})");
        }
        
        return audioSource;
    }
    
    /// <summary>
    /// ジェットパック音の音量を更新（滑らかにフェード）
    /// </summary>
    /// <param name="audioSource">更新するAudioSource</param>
    /// <param name="isActive">ジェットパック起動中かどうか</param>
    /// <param name="fadeDuration">フェード時間（秒）</param>
    public void UpdateJetpackVolume(AudioSource audioSource, bool isActive, float fadeDuration = 0.3f)
    {
        if (audioSource == null || soundSettings == null) return;
        
        string soundType = isActive ? "JetpackActive" : "JetpackIdle";
        float targetVolume = soundSettings.GetVolumeForSoundType(soundType);
        
        // 既にフェード中の場合は停止
        if (jetpackFadeCoroutine != null)
        {
            StopCoroutine(jetpackFadeCoroutine);
        }
        
        // フェードコルーチンを開始
        jetpackFadeCoroutine = StartCoroutine(FadeJetpackVolume(audioSource, targetVolume, fadeDuration));
        
        if (enableDebugLog)
        {
            Debug.Log($"[DigSoundManager] ジェットパック音量更新開始: {soundType} = {targetVolume:F2} (フェード時間: {fadeDuration}秒)");
        }
    }
    
    // ジェットパック音量フェード用のコルーチン参照
    private Coroutine jetpackFadeCoroutine;
    
    /// <summary>
    /// ジェットパック音量を滑らかにフェード
    /// </summary>
    private IEnumerator FadeJetpackVolume(AudioSource audioSource, float targetVolume, float duration)
    {
        if (audioSource == null) yield break;
        
        float startVolume = audioSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration && audioSource != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        
        if (audioSource != null)
        {
            audioSource.volume = targetVolume;
        }
        
        jetpackFadeCoroutine = null;
    }
    
    /// <summary>
    /// ジェットパック音を停止
    /// </summary>
    /// <param name="targetTransform">停止するTransform</param>
    public void StopJetpackSound(Transform targetTransform)
    {
        StopSoundAtTransform(targetTransform);
    }
}




