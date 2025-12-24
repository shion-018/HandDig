using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// お宝への接近を検出して音を鳴らすマネージャー
/// </summary>
public class TreasureProximitySoundManager : MonoBehaviour
{
    [Header("検出設定")]
    [Tooltip("検出するお宝のタグ（基本フィルタ）")]
    public string treasureTag = "Treasure";
    
    [Tooltip("検出更新間隔（秒）")]
    [Range(0.1f, 1f)]
    public float detectionInterval = 0.2f;
    
    [Tooltip("お宝接近音の最大検出距離")]
    public float maxDistance = 30f;
    
    [Tooltip("お宝接近音の最小検出距離")]
    public float minDistance = 5f;
    
    [Header("貴重なお宝の検出方法")]
    [Tooltip("検出方法を選択")]
    public DetectionMode detectionMode = DetectionMode.ComponentOnly;
    
    public enum DetectionMode
    {
        ComponentOnly,      // PreciousTreasureMarkerコンポーネントを持つもののみ
        PrefabListOnly,     // 設定したプレハブリストのみ
        Both                // コンポーネント OR プレハブリストのどちらか
    }
    
    [Header("プレハブリスト（PrefabListOnlyまたはBothモードで使用）")]
    [Tooltip("接近音を鳴らす貴重なお宝のプレハブリスト")]
    public List<GameObject> preciousTreasurePrefabs = new List<GameObject>();
    
    [Header("プレイヤー設定")]
    [Tooltip("プレイヤーのルートオブジェクト（自動検索される）")]
    public Transform playerTransform;
    
    [Header("コンパス設定")]
    [Tooltip("コンパスに付いたオーディオソース（音の再生に使用）")]
    public AudioSource compassAudioSource;
    
    [Header("音声設定")]
    [Tooltip("音声マネージャー（自動検索される）")]
    public DigSoundManager soundManager;
    
    [Tooltip("音声設定ファイル（自動検索される）")]
    public DigSoundSettings soundSettings;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを出力するか")]
    public bool enableDebugLog = false;
    
    private Dictionary<GameObject, float> treasureLastSoundTime = new Dictionary<GameObject, float>();
    private float lastDetectionTime = 0f;
    
    private static TreasureProximitySoundManager instance;
    
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static TreasureProximitySoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TreasureProximitySoundManager>();
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
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // プレイヤーを自動検索
        if (playerTransform == null)
        {
            GameObject playerRoot = GameObject.FindGameObjectWithTag("Player");
            if (playerRoot == null)
            {
                // VRプレイヤーの場合、OVRCameraRigを探す
                playerRoot = GameObject.Find("OVRCameraRig");
                if (playerRoot == null)
                {
                    // 最後の手段として、CharacterControllerを持つオブジェクトを探す
                    CharacterController controller = FindObjectOfType<CharacterController>();
                    if (controller != null)
                    {
                        playerRoot = controller.gameObject;
                    }
                }
            }
            
            if (playerRoot != null)
            {
                playerTransform = playerRoot.transform;
            }
        }
        
        // 音声マネージャーを自動検索
        if (soundManager == null)
        {
            soundManager = DigSoundManager.Instance;
        }
        
        // 音声設定を自動検索
        if (soundSettings == null && soundManager != null)
        {
            soundSettings = soundManager.soundSettings;
        }
        
        // コンパスのオーディオソースを自動検索
        if (compassAudioSource == null)
        {
            // CompassScriptを持つオブジェクトを探す
            CompassScript compassScript = FindObjectOfType<CompassScript>();
            if (compassScript != null)
            {
                compassAudioSource = compassScript.GetComponent<AudioSource>();
                if (compassAudioSource == null)
                {
                    // 子オブジェクトから探す
                    compassAudioSource = compassScript.GetComponentInChildren<AudioSource>();
                }
            }
            
            // 見つからない場合は名前で探す
            if (compassAudioSource == null)
            {
                GameObject compassObj = GameObject.Find("Compass");
                if (compassObj == null)
                {
                    compassObj = GameObject.FindGameObjectWithTag("Compass");
                }
                if (compassObj != null)
                {
                    compassAudioSource = compassObj.GetComponent<AudioSource>();
                    if (compassAudioSource == null)
                    {
                        compassAudioSource = compassObj.GetComponentInChildren<AudioSource>();
                    }
                }
            }
        }
        
        // 設定ファイルから距離を取得（未設定の場合）
        if (soundSettings != null)
        {
            if (maxDistance <= 0)
            {
                maxDistance = soundSettings.treasureProximityMaxDistance;
            }
            if (minDistance <= 0)
            {
                minDistance = soundSettings.treasureProximityMinDistance;
            }
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[TreasureProximitySoundManager] 初期化完了 - プレイヤー: {(playerTransform != null ? playerTransform.name : "未検出")}, 音声マネージャー: {(soundManager != null ? "検出" : "未検出")}, コンパスオーディオソース: {(compassAudioSource != null ? compassAudioSource.name : "未検出")}, 最大距離: {maxDistance}, 最小距離: {minDistance}");
        }
    }

    private void Update()
    {
        if (playerTransform == null || soundSettings == null || compassAudioSource == null)
        {
            return;
        }
        
        // コンパスのオーディオソースが未検出の場合は再検索
        if (compassAudioSource == null)
        {
            CompassScript compassScript = FindObjectOfType<CompassScript>();
            if (compassScript != null)
            {
                compassAudioSource = compassScript.GetComponent<AudioSource>();
                if (compassAudioSource == null)
                {
                    compassAudioSource = compassScript.GetComponentInChildren<AudioSource>();
                }
            }
        }
        
        if (compassAudioSource == null)
        {
            return;
        }
        
        // 検出間隔をチェック
        if (Time.time - lastDetectionTime < detectionInterval)
        {
            return;
        }
        
        lastDetectionTime = Time.time;
        
        // すべてのお宝を検出（タグで基本フィルタ）
        GameObject[] allTreasures = GameObject.FindGameObjectsWithTag(treasureTag);
        
        if (allTreasures == null || allTreasures.Length == 0)
        {
            return;
        }
        
        Vector3 playerPosition = playerTransform.position;
        
        foreach (GameObject treasure in allTreasures)
        {
            if (treasure == null) continue;
            
            // 貴重なお宝かどうかを判定
            if (!IsPreciousTreasure(treasure))
            {
                continue;
            }
            
            float distance = Vector3.Distance(playerPosition, treasure.transform.position);
            
            // 検出範囲内かチェック（maxDistanceより遠い場合は音を鳴らさない）
            if (distance <= maxDistance)
            {
                // 距離に応じて音の間隔を計算
                // minDistanceより近い場合は最短間隔を使用
                float normalizedDistance;
                if (distance <= minDistance)
                {
                    normalizedDistance = 1f; // 最短間隔
                }
                else
                {
                    normalizedDistance = Mathf.InverseLerp(maxDistance, minDistance, distance);
                }
                
                float currentInterval = Mathf.Lerp(
                    soundSettings.treasureProximityMaxInterval,
                    soundSettings.treasureProximityMinInterval,
                    normalizedDistance
                );
                
                // 前回の音から十分な時間が経過しているかチェック
                float lastSoundTime = 0f;
                if (treasureLastSoundTime.TryGetValue(treasure, out lastSoundTime))
                {
                    if (Time.time - lastSoundTime < currentInterval)
                    {
                        continue;
                    }
                }
                
                // 音を再生（コンパスのオーディオソースを直接制御）
                if (soundSettings != null && soundSettings.treasureProximitySound != null)
                {
                    // オーディオソースが既に再生中でない場合のみ再生
                    if (!compassAudioSource.isPlaying)
                    {
                        compassAudioSource.clip = soundSettings.treasureProximitySound;
                        compassAudioSource.volume = soundSettings.baseVolume;
                        compassAudioSource.pitch = soundSettings.basePitch;
                        compassAudioSource.Play();
                    }
                }
                treasureLastSoundTime[treasure] = Time.time;
                
                if (enableDebugLog)
                {
                    Debug.Log($"[TreasureProximitySoundManager] お宝接近音再生: {treasure.name} (距離: {distance:F2}m, 間隔: {currentInterval:F2}s)");
                }
            }
            else if (distance > maxDistance)
            {
                // 範囲外に出たお宝の記録をクリア（メモリ節約）
                treasureLastSoundTime.Remove(treasure);
            }
        }
    }
    
    /// <summary>
    /// お宝の記録をクリア（お宝が取得された時などに呼び出す）
    /// </summary>
    public void ClearTreasureRecord(GameObject treasure)
    {
        if (treasure != null && treasureLastSoundTime.ContainsKey(treasure))
        {
            treasureLastSoundTime.Remove(treasure);
        }
    }
    
    /// <summary>
    /// 貴重なお宝かどうかを判定
    /// </summary>
    private bool IsPreciousTreasure(GameObject treasure)
    {
        if (treasure == null) return false;
        
        bool hasComponent = treasure.GetComponent<PreciousTreasureMarker>() != null;
        bool isInPrefabList = IsTreasureFromPrefabList(treasure);
        
        switch (detectionMode)
        {
            case DetectionMode.ComponentOnly:
                return hasComponent;
                
            case DetectionMode.PrefabListOnly:
                return isInPrefabList;
                
            case DetectionMode.Both:
                return hasComponent || isInPrefabList;
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// お宝が設定したプレハブリストから生成されたものかどうかを判定
    /// </summary>
    private bool IsTreasureFromPrefabList(GameObject treasure)
    {
        if (preciousTreasurePrefabs == null || preciousTreasurePrefabs.Count == 0)
        {
            return false;
        }
        
        // プレハブのインスタンスIDを比較
        foreach (GameObject prefab in preciousTreasurePrefabs)
        {
            if (prefab == null) continue;
            
#if UNITY_EDITOR
            // エディタではPrefabUtilityを使って正確に判定
            if (PrefabUtility.GetCorrespondingObjectFromSource(treasure) == prefab)
            {
                return true;
            }
#else
            // ランタイムでは名前ベースで判定（プレハブとインスタンスの名前が同じ場合）
            // 注意: これは完全には正確ではないため、コンポーネント方式の使用を推奨
            if (treasure.name.Replace("(Clone)", "").Trim() == prefab.name)
            {
                return true;
            }
#endif
        }
        
        return false;
    }
}

