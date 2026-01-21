using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ゴール時の処理を管理するマネージャー
/// ゴール到達時の処理と、カギ収集時の処理の両方を管理
/// </summary>
public class GoalManager : MonoBehaviour
{
    [Header("ゴール設定")]
    [Tooltip("ゴール判定のトリガー")]
    public Collider goalTrigger;
    
    [Tooltip("ゴール時のワープ先位置")]
    public Transform goalWarpPoint;
    
    [Tooltip("プレイヤーのルートオブジェクト")]
    public GameObject playerRoot;
    
    [Header("フェード設定")]
    [Tooltip("フェードアウト時間（秒）")]
    public float fadeOutDuration = 1.0f;
    
    [Tooltip("フェードイン時間（秒）")]
    public float fadeInDuration = 1.0f;
    
    [Tooltip("フェード演出を使用するか")]
    public bool useFade = true;
    
    [Header("ゴールUI設定")]
    [Tooltip("ゴールUI表示時間（秒）")]
    public float goalUIDisplayDuration = 3.0f;
    
    [Header("カギ収集設定")]
    [Tooltip("カギ収集時の機能を有効にするか")]
    public bool enableKeyCollectionFeature = true;
    
    [Tooltip("監視するDoorController（カギの収集状況を確認）")]
    [SerializeField] private DoorController doorController;
    
    [Tooltip("自動検出するか（DoorControllerを自動で探す）")]
    [SerializeField] private bool autoDetectDoorController = true;
    
    [Header("鍵とドアの連携設定")]
    [Tooltip("鍵とドアの自動連携を有効にするか")]
    [SerializeField] private bool enableKeyDoorLinking = true;
    
    [Tooltip("鍵を自動検出するか")]
    [SerializeField] private bool autoDetectKeys = true;
    
    [Tooltip("ドアを自動検出するか")]
    [SerializeField] private bool autoDetectDoors = true;
    
    [Tooltip("鍵のタグ")]
    [SerializeField] private string keyTag = "Key";
    
    [Tooltip("ドアのタグ")]
    [SerializeField] private string doorTag = "Door";
    
    [Tooltip("手動設定された鍵")]
    [SerializeField] private List<KeyItem> manualKeys = new List<KeyItem>();
    
    [Tooltip("手動設定されたドア")]
    [SerializeField] private List<DoorController> manualDoors = new List<DoorController>();
    
    [Header("扉オブジェクト")]
    [Tooltip("強調する扉オブジェクト（複数登録可）")]
    [SerializeField] private List<GameObject> goalDoors = new List<GameObject>();
    
    [Header("カギ収集UI設定")]
    [Tooltip("カギ収集時の表示テキスト（インスペクターから編集可能）")]
    [SerializeField] private string keyCollectionMessage = "扉に向かおう";
    
    [Tooltip("カギ収集UI表示用のCanvas（自動生成可）")]
    [SerializeField] private Canvas keyCollectionCanvas;
    
    [Tooltip("カギ収集UI表示用のTextMeshProUGUI（自動生成可）")]
    [SerializeField] private TextMeshProUGUI keyCollectionText;
    
    [Tooltip("テキストの色")]
    [SerializeField] private Color keyCollectionTextColor = Color.white;
    
    [Tooltip("テキストのサイズ")]
    [SerializeField] private float keyCollectionTextSize = 46f;
    
    [Tooltip("表示時間（秒）")]
    [SerializeField] private float keyCollectionDisplayDuration = 3f;
    
    [Tooltip("フェードイン時間（秒）")]
    [SerializeField] private float keyCollectionFadeInDuration = 0.5f;
    
    [Tooltip("フェードアウト時間（秒）")]
    [SerializeField] private float keyCollectionFadeOutDuration = 0.5f;
    
    [Header("輪郭強調設定")]
    [Tooltip("輪郭強調機能を有効にするか")]
    [SerializeField] private bool enableOutlineEffect = true;
    
    [Tooltip("輪郭の色")]
    [SerializeField] private Color outlineColor = new Color(1f, 0.8f, 0f, 1f); // オレンジ系
    
    [Tooltip("最大強調時の輪郭の太さ")]
    [SerializeField] private float maxOutlineWidth = 0.1f;
    
    [Tooltip("最小強調時の輪郭の太さ（近くに来た時）")]
    [SerializeField] private float minOutlineWidth = 0.01f;
    
    [Tooltip("強調が最大になる距離")]
    [SerializeField] private float maxDistance = 50f;
    
    [Tooltip("強調が最小になる距離")]
    [SerializeField] private float minDistance = 5f;
    
    [Tooltip("壁越しでも見えるようにするか")]
    [SerializeField] private bool visibleThroughWalls = true;
    
    [Header("デバッグ")]
    [Tooltip("デバッグログを出力するか")]
    public bool enableDebugLog = true;

    private static GoalManager instance;
    private bool hasReachedGoal = false;
    
    // カギ収集関連
    private bool allKeysCollected = false;
    private Dictionary<GameObject, OutlineEffect> outlineEffects = new Dictionary<GameObject, OutlineEffect>();
    private Transform playerTransform;
    
    // 鍵とドアの連携関連
    private List<KeyItem> allKeys = new List<KeyItem>();
    private List<DoorController> allDoors = new List<DoorController>();
    private Coroutine keyCollectionUICoroutine;
    
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static GoalManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GoalManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GoalManager");
                    instance = go.AddComponent<GoalManager>();
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
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // プレイヤーが見つからない場合は自動検索
        if (playerRoot == null)
        {
            playerRoot = GameObject.FindGameObjectWithTag("Player");
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
        }

        // プレイヤーのTransformを設定（カギ収集機能用）
        if (playerRoot != null)
        {
            playerTransform = playerRoot.transform;
        }

        if (enableDebugLog)
        {
            Debug.Log($"[GoalManager] プレイヤー設定: {(playerRoot != null ? playerRoot.name : "未設定")}");
        }
        
        // カギ収集機能を初期化
        if (enableKeyCollectionFeature)
        {
            InitializeKeyCollection();
        }
        
        // 鍵とドアの連携を初期化
        if (enableKeyDoorLinking)
        {
            InitializeKeyDoorLinking();
        }
    }
    
    /// <summary>
    /// 鍵とドアの連携を初期化
    /// </summary>
    private void InitializeKeyDoorLinking()
    {
        if (autoDetectKeys)
        {
            DetectKeys();
        }
        
        if (autoDetectDoors)
        {
            DetectDoors();
        }
        
        // 手動設定された鍵とドアを追加
        RegisterManualKeys();
        RegisterManualDoors();
        
        // 鍵とドアを連携
        ConnectKeysAndDoors();
        
        if (enableDebugLog)
        {
            Debug.Log($"[GoalManager] 鍵とドアの連携初期化完了。鍵数: {allKeys.Count}, ドア数: {allDoors.Count}");
        }
    }
    
    /// <summary>
    /// シーン内の鍵を自動検出
    /// </summary>
    private void DetectKeys()
    {
        // タグで検出
        GameObject[] keyObjects = GameObject.FindGameObjectsWithTag(keyTag);
        foreach (GameObject keyObj in keyObjects)
        {
            KeyItem keyItem = keyObj.GetComponent<KeyItem>();
            if (keyItem != null && !allKeys.Contains(keyItem))
            {
                allKeys.Add(keyItem);
            }
        }
        
        // KeyItemコンポーネントで検出
        KeyItem[] allKeyItems = FindObjectsOfType<KeyItem>();
        foreach (KeyItem keyItem in allKeyItems)
        {
            if (!allKeys.Contains(keyItem))
            {
                allKeys.Add(keyItem);
            }
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[GoalManager] 自動検出で{allKeys.Count}個の鍵を発見しました");
        }
    }
    
    /// <summary>
    /// シーン内のドアを自動検出
    /// </summary>
    private void DetectDoors()
    {
        // タグで検出
        GameObject[] doorObjects = GameObject.FindGameObjectsWithTag(doorTag);
        foreach (GameObject doorObj in doorObjects)
        {
            DoorController doorController = doorObj.GetComponent<DoorController>();
            if (doorController != null && !allDoors.Contains(doorController))
            {
                allDoors.Add(doorController);
            }
        }
        
        // DoorControllerコンポーネントで検出
        DoorController[] allDoorControllers = FindObjectsOfType<DoorController>();
        foreach (DoorController doorController in allDoorControllers)
        {
            if (!allDoors.Contains(doorController))
            {
                allDoors.Add(doorController);
            }
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[GoalManager] 自動検出で{allDoors.Count}個のドアを発見しました");
        }
    }
    
    /// <summary>
    /// 手動設定された鍵を登録
    /// </summary>
    private void RegisterManualKeys()
    {
        foreach (KeyItem key in manualKeys)
        {
            if (key != null && !allKeys.Contains(key))
            {
                allKeys.Add(key);
            }
        }
    }
    
    /// <summary>
    /// 手動設定されたドアを登録
    /// </summary>
    private void RegisterManualDoors()
    {
        foreach (DoorController door in manualDoors)
        {
            if (door != null && !allDoors.Contains(door))
            {
                allDoors.Add(door);
            }
        }
    }
    
    /// <summary>
    /// 鍵とドアを連携
    /// </summary>
    private void ConnectKeysAndDoors()
    {
        foreach (KeyItem key in allKeys)
        {
            if (key != null)
            {
                // 鍵が収集された時のイベントを設定
                key.OnKeyCollected += OnKeyCollectedFromLinking;
            }
        }
        
        if (enableDebugLog)
        {
            Debug.Log("[GoalManager] 鍵とドアの連携を設定しました");
        }
    }
    
    /// <summary>
    /// 鍵が収集された時の処理（鍵とドアの連携用）
    /// </summary>
    private void OnKeyCollectedFromLinking(KeyItem collectedKey)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[GoalManager] 鍵が収集されました（連携）: {collectedKey.name}");
        }
        
        // 全てのドアに鍵収集を通知
        foreach (DoorController door in allDoors)
        {
            if (door != null)
            {
                door.CollectKey(collectedKey.gameObject);
            }
        }
    }
    
    /// <summary>
    /// カギ収集機能を初期化
    /// </summary>
    private void InitializeKeyCollection()
    {
        // DoorControllerを検出
        if (autoDetectDoorController && doorController == null)
        {
            doorController = FindObjectOfType<DoorController>();
        }
        
        if (doorController != null)
        {
            // カギ収集イベントを購読
            doorController.OnKeyCollected += OnKeyCollected;
            doorController.OnDoorOpened += OnDoorOpened;
            
            // 既に全てのカギが集まっているかチェック
            int currentKeyCount = doorController.GetCurrentKeyCount();
            int requiredKeyCount = doorController.GetRequiredKeyCount();
            
            if (currentKeyCount >= requiredKeyCount || doorController.IsDoorOpen())
            {
                if (enableDebugLog)
                {
                    Debug.Log("[GoalManager] 既に全てのカギが集まっています");
                }
                ActivateKeyCollectionGoal();
            }
        }
        else
        {
            if (enableDebugLog)
            {
                Debug.LogWarning("[GoalManager] DoorControllerが見つかりません（カギ収集機能は無効）");
            }
        }
        
        // UIを初期化
        InitializeKeyCollectionUI();
        
        // 扉の輪郭エフェクトを初期化
        if (enableOutlineEffect)
        {
            InitializeOutlineEffects();
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[GoalManager] カギ収集機能初期化完了。扉数: {goalDoors.Count}, 輪郭強調: {enableOutlineEffect}");
        }
    }
    
    /// <summary>
    /// カギ収集UIを初期化
    /// </summary>
    private void InitializeKeyCollectionUI()
    {
        if (keyCollectionCanvas == null)
        {
            CreateKeyCollectionUI();
        }
        
        if (keyCollectionText != null)
        {
            keyCollectionText.text = keyCollectionMessage;
            keyCollectionText.fontSize = keyCollectionTextSize;
            // 初期状態では透明に設定
            keyCollectionText.color = new Color(keyCollectionTextColor.r, keyCollectionTextColor.g, keyCollectionTextColor.b, 0f);
        }
        
        // 初期状態では非表示
        if (keyCollectionCanvas != null)
        {
            keyCollectionCanvas.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// カギ収集UIを自動作成
    /// </summary>
    private void CreateKeyCollectionUI()
    {
        GameObject canvasObj = new GameObject("KeyCollectionCanvas");
        canvasObj.transform.SetParent(transform);
        
        keyCollectionCanvas = canvasObj.AddComponent<Canvas>();
        keyCollectionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        keyCollectionCanvas.sortingOrder = 998; // TutorialManagerより下
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        GameObject textObj = new GameObject("KeyCollectionText");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        keyCollectionText = textObj.AddComponent<TextMeshProUGUI>();
        keyCollectionText.text = keyCollectionMessage;
        keyCollectionText.color = keyCollectionTextColor;
        keyCollectionText.fontSize = keyCollectionTextSize;
        keyCollectionText.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = keyCollectionText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.15f);
        rect.anchorMax = new Vector2(0.5f, 0.15f);
        rect.sizeDelta = new Vector2(900, 120);
        rect.anchoredPosition = Vector2.zero;
    }
    
    /// <summary>
    /// 輪郭エフェクトを初期化
    /// </summary>
    private void InitializeOutlineEffects()
    {
        foreach (GameObject door in goalDoors)
        {
            if (door != null)
            {
                OutlineEffect effect = door.GetComponent<OutlineEffect>();
                if (effect == null)
                {
                    effect = door.AddComponent<OutlineEffect>();
                }
                
                effect.outlineColor = outlineColor;
                effect.outlineWidth = 0f; // 初期状態では非表示
                effect.visibleThroughWalls = visibleThroughWalls;
                
                outlineEffects[door] = effect;
            }
        }
    }
    
    /// <summary>
    /// カギが収集された時の処理
    /// </summary>
    private void OnKeyCollected(int currentKeyCount)
    {
        if (doorController == null || !enableKeyCollectionFeature) return;
        
        int requiredKeyCount = doorController.GetRequiredKeyCount();
        
        if (enableDebugLog)
        {
            Debug.Log($"[GoalManager] カギ収集: {currentKeyCount}/{requiredKeyCount}");
        }
        
        // 全てのカギが集まったかチェック
        if (currentKeyCount >= requiredKeyCount && !allKeysCollected)
        {
            ActivateKeyCollectionGoal();
        }
    }
    
    /// <summary>
    /// ドアが開いた時の処理
    /// </summary>
    private void OnDoorOpened()
    {
        // テキスト表示は鍵をすべて集めた時のみ行うため、ここでは何もしない
        if (enableDebugLog)
        {
            Debug.Log("[GoalManager] ドアが開きました（テキスト表示は鍵収集時にのみ実行）");
        }
    }
    
    /// <summary>
    /// カギ収集時のゴールを有効化
    /// </summary>
    private void ActivateKeyCollectionGoal()
    {
        if (allKeysCollected || !enableKeyCollectionFeature) return;
        
        allKeysCollected = true;
        
        if (enableDebugLog)
        {
            Debug.Log("[GoalManager] 全てのカギが集まりました！扉の輪郭を強調します");
        }
        
        // UIを表示
        ShowKeyCollectionUI();
        
        // 扉の輪郭を強調（有効な場合のみ）
        if (enableOutlineEffect)
        {
            EnableDoorOutlines();
        }
    }
    
    /// <summary>
    /// カギ収集UIを表示
    /// </summary>
    private void ShowKeyCollectionUI()
    {
        if (keyCollectionUICoroutine != null)
        {
            StopCoroutine(keyCollectionUICoroutine);
        }
        
        keyCollectionUICoroutine = StartCoroutine(ShowKeyCollectionUICoroutine());
    }
    
    /// <summary>
    /// カギ収集UI表示のコルーチン（フェードイン・フェードアウト付き）
    /// </summary>
    private IEnumerator ShowKeyCollectionUICoroutine()
    {
        // Canvasをアクティブにする
        if (keyCollectionCanvas != null)
        {
            keyCollectionCanvas.gameObject.SetActive(true);
        }
        
        // テキストを設定
        if (keyCollectionText != null)
        {
            keyCollectionText.text = keyCollectionMessage;
        }
        
        // フェードイン
        yield return StartCoroutine(FadeInKeyCollectionUI());
        
        // 表示時間待機
        yield return new WaitForSeconds(keyCollectionDisplayDuration);
        
        // フェードアウト
        yield return StartCoroutine(FadeOutKeyCollectionUI());
        
        // Canvasを非アクティブにする
        if (keyCollectionCanvas != null)
        {
            keyCollectionCanvas.gameObject.SetActive(false);
        }
        
        keyCollectionUICoroutine = null;
    }
    
    /// <summary>
    /// カギ収集UIのフェードイン
    /// </summary>
    private IEnumerator FadeInKeyCollectionUI()
    {
        float elapsed = 0f;
        Color startTextColor = new Color(keyCollectionTextColor.r, keyCollectionTextColor.g, keyCollectionTextColor.b, 0f);
        Color endTextColor = keyCollectionTextColor;
        
        while (elapsed < keyCollectionFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / keyCollectionFadeInDuration;
            
            if (keyCollectionText != null)
            {
                keyCollectionText.color = Color.Lerp(startTextColor, endTextColor, t);
            }
            
            yield return null;
        }
        
        // 最終値を設定
        if (keyCollectionText != null)
        {
            keyCollectionText.color = endTextColor;
        }
    }
    
    /// <summary>
    /// カギ収集UIのフェードアウト
    /// </summary>
    private IEnumerator FadeOutKeyCollectionUI()
    {
        float elapsed = 0f;
        Color startTextColor = keyCollectionTextColor;
        Color endTextColor = new Color(keyCollectionTextColor.r, keyCollectionTextColor.g, keyCollectionTextColor.b, 0f);
        
        while (elapsed < keyCollectionFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / keyCollectionFadeOutDuration;
            
            if (keyCollectionText != null)
            {
                keyCollectionText.color = Color.Lerp(startTextColor, endTextColor, t);
            }
            
            yield return null;
        }
        
        // 最終値を設定
        if (keyCollectionText != null)
        {
            keyCollectionText.color = endTextColor;
        }
    }
    
    /// <summary>
    /// 扉の輪郭を有効化
    /// </summary>
    private void EnableDoorOutlines()
    {
        foreach (var kvp in outlineEffects)
        {
            GameObject door = kvp.Key;
            OutlineEffect effect = kvp.Value;
            
            if (door != null && effect != null)
            {
                effect.enabled = true;
            }
        }
    }
    
    private void Update()
    {
        // カギ収集時の輪郭強度を更新（輪郭強調が有効な場合のみ）
        if (enableKeyCollectionFeature && enableOutlineEffect && allKeysCollected && playerTransform != null)
        {
            UpdateOutlineIntensity();
        }
    }
    
    /// <summary>
    /// 輪郭の強度を距離に応じて更新
    /// </summary>
    private void UpdateOutlineIntensity()
    {
        Vector3 playerPos = playerTransform.position;
        
        foreach (var kvp in outlineEffects)
        {
            GameObject door = kvp.Key;
            OutlineEffect effect = kvp.Value;
            
            if (door == null || effect == null || !effect.enabled) continue;
            
            // 扉までの距離を計算
            float distance = Vector3.Distance(playerPos, door.transform.position);
            
            // 距離に応じて輪郭の太さを調整
            float normalizedDistance = Mathf.InverseLerp(minDistance, maxDistance, distance);
            float outlineWidth = Mathf.Lerp(minOutlineWidth, maxOutlineWidth, normalizedDistance);
            
            effect.outlineWidth = outlineWidth;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーがゴールに到達した場合
        if (!hasReachedGoal && IsPlayer(other.gameObject))
        {
            hasReachedGoal = true;
            StartCoroutine(HandleGoalSequence());
        }
    }

    /// <summary>
    /// 指定されたオブジェクトがプレイヤーかどうかを判定（公開API）
    /// </summary>
    public bool IsPlayerObject(GameObject obj)
    {
        if (obj == null) return false;
        
        // 直接プレイヤーかチェック
        if (obj.CompareTag("Player")) return true;
        
        // プレイヤーの子オブジェクトかチェック
        if (playerRoot != null && obj.transform.IsChildOf(playerRoot.transform)) return true;
        
        // CharacterControllerを持つかチェック
        if (obj.GetComponent<CharacterController>() != null) return true;
        
        // VRプレイヤーの場合
        if (obj.name.Contains("OVRCameraRig") || obj.name.Contains("VRPlayer")) return true;
        
        return false;
    }

    // 互換性のための内部メソッド（既存処理からの呼び出し用）
    private bool IsPlayer(GameObject obj)
    {
        return IsPlayerObject(obj);
    }

    /// <summary>
    /// ゴール時の一連の処理を実行
    /// </summary>
    private IEnumerator HandleGoalSequence()
    {
        if (enableDebugLog)
        {
            Debug.Log("[GoalManager] ゴール到達！処理開始");
        }

        // 1. フェードアウト（フェード完了時にゴールUI表示）
        if (useFade && FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeOutWithCallback(fadeOutDuration, OnFadeOutComplete);
        }
        else
        {
            // フェードを使用しない場合は即座にゴールUIを表示
            OnFadeOutComplete();
        }

        // 2. プレイヤーをワープ
        if (goalWarpPoint != null && playerRoot != null)
        {
            WarpPlayerToGoal();
            
            // ワープ後にファンファーレ音を再生（プレイヤーの位置から）
            if (DigSoundManager.Instance != null && playerRoot != null)
            {
                DigSoundManager.Instance.PlayGoalFanfareSound(playerRoot.transform.position);
            }
        }

        // 3. フェードイン
        if (useFade && FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeIn(fadeInDuration);
        }

        if (enableDebugLog)
        {
            Debug.Log("[GoalManager] ゴール処理完了");
        }
    }

    /// <summary>
    /// フェードアウト完了時のコールバック
    /// </summary>
    private void OnFadeOutComplete()
    {
        // ゴールUI表示
        if (GoalUI.Instance != null)
        {
            GoalUI.Instance.ShowGoalUI();
            
            // UI表示開始のコルーチンを起動
            StartCoroutine(HandleGoalUIDisplay());
        }
    }

    /// <summary>
    /// ゴールUI表示の処理
    /// </summary>
    private IEnumerator HandleGoalUIDisplay()
    {
        // UI表示時間待機
        yield return new WaitForSeconds(goalUIDisplayDuration);
        
        // UI非表示
        if (GoalUI.Instance != null)
        {
            GoalUI.Instance.HideGoalUI();
        }
    }

    /// <summary>
    /// 外部からゴールシーケンスを開始（委譲用公開API）
    /// </summary>
    /// <param name="warpPoint">ゴール後にワープする地点（null可）</param>
    /// <param name="playerOverride">プレイヤールートの明示指定（null可）</param>
    public void StartGoalSequence(Transform warpPoint = null, GameObject playerOverride = null)
    {
        if (hasReachedGoal) return;

        if (warpPoint != null)
        {
            goalWarpPoint = warpPoint;
        }

        if (playerOverride != null)
        {
            playerRoot = playerOverride;
        }

        hasReachedGoal = true;
        StartCoroutine(HandleGoalSequence());
    }

    /// <summary>
    /// リザルト用スクリプトにお宝データを設定（リフレクション使用）
    /// </summary>
    private void SetTreasureDataToResultScript()
    {
        // VRDigToolManagerからお宝データを取得
        if (VRDigToolManager.Instance == null)
        {
            Debug.LogError("[GoalManager] VRDigToolManager.Instanceが見つかりませんでした");
            return;
        }

        int totalTreasure = VRDigToolManager.Instance.GetResultTotalTreasureCount();
        int specialTreasure = VRDigToolManager.Instance.GetResultSpecialTreasureCount();
        int digPower = VRDigToolManager.Instance.GetResultDigPower();

        Debug.Log($"[GoalManager] お宝データ取得: 総数={totalTreasure}, 特殊={specialTreasure}, 掘る力={digPower}");

        // ResultColliderScriptを探す（非アクティブも含む）
        ResultColliderScript resultScript = FindObjectOfType<ResultColliderScript>(true);
        if (resultScript == null)
        {
            Debug.LogError("[GoalManager] ResultColliderScriptが見つかりませんでした");
            return;
        }

        Debug.Log($"[GoalManager] ResultColliderScriptを発見: {resultScript.gameObject.name}");

        // リフレクションを使ってResultColliderScriptのprivate変数に直接アクセス
        System.Type type = typeof(ResultColliderScript);
        System.Reflection.FieldInfo getAllJewelsField = type.GetField("_getAllJewels", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo getUniqueJewelsField = type.GetField("_getUniqueJewels", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo digPowerField = type.GetField("_digPower", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (getAllJewelsField == null)
        {
            Debug.LogError("[GoalManager] _getAllJewelsフィールドが見つかりませんでした");
        }
        if (getUniqueJewelsField == null)
        {
            Debug.LogError("[GoalManager] _getUniqueJewelsフィールドが見つかりませんでした");
        }
        if (digPowerField == null)
        {
            Debug.LogError("[GoalManager] _digPowerフィールドが見つかりませんでした");
        }

        if (getAllJewelsField != null && getUniqueJewelsField != null && digPowerField != null)
        {
            Debug.Log($"[GoalManager] リフレクションで値を設定開始: 総数={totalTreasure}, 特殊={specialTreasure}, 掘る力={digPower}");
            
            // 設定前の値を確認
            int beforeAll = (int)getAllJewelsField.GetValue(resultScript);
            int beforeUnique = (int)getUniqueJewelsField.GetValue(resultScript);
            int beforePower = (int)digPowerField.GetValue(resultScript);
            Debug.Log($"[GoalManager] 設定前の値: 総数={beforeAll}, 特殊={beforeUnique}, 掘る力={beforePower}");

            getAllJewelsField.SetValue(resultScript, totalTreasure);
            getUniqueJewelsField.SetValue(resultScript, specialTreasure);
            digPowerField.SetValue(resultScript, digPower);

            // 設定後の値を確認
            int checkAll = (int)getAllJewelsField.GetValue(resultScript);
            int checkUnique = (int)getUniqueJewelsField.GetValue(resultScript);
            int checkPower = (int)digPowerField.GetValue(resultScript);

            Debug.Log($"[GoalManager] リザルトデータを設定完了: 総数={checkAll}, 特殊={checkUnique}, 掘る力={checkPower}");
            
            if (checkAll != totalTreasure || checkUnique != specialTreasure || checkPower != digPower)
            {
                Debug.LogError($"[GoalManager] 値の設定に失敗しました！期待値: 総数={totalTreasure}, 特殊={specialTreasure}, 掘る力={digPower}");
            }
        }
        else
        {
            Debug.LogError("[GoalManager] ResultColliderScriptの変数にアクセスできませんでした");
            if (getAllJewelsField == null) Debug.LogError("[GoalManager] _getAllJewelsフィールドが見つかりません");
            if (getUniqueJewelsField == null) Debug.LogError("[GoalManager] _getUniqueJewelsフィールドが見つかりません");
            if (digPowerField == null) Debug.LogError("[GoalManager] _digPowerフィールドが見つかりません");
        }
    }

    /// <summary>
    /// プレイヤーをゴール位置にワープ
    /// </summary>
    private void WarpPlayerToGoal()
    {
        if (playerRoot == null || goalWarpPoint == null) return;

        Vector3 warpPosition = goalWarpPoint.position;
        Quaternion warpRotation = goalWarpPoint.rotation;
        Vector3 oldPosition = playerRoot.transform.position;
        Quaternion oldRotation = playerRoot.transform.rotation;

        // CharacterControllerがある場合は特別な処理
        CharacterController controller = playerRoot.GetComponent<CharacterController>();
        if (controller != null)
        {
            // CharacterControllerを一時的に無効化
            controller.enabled = false;
            
            // 位置と向きを設定
            playerRoot.transform.position = warpPosition;
            playerRoot.transform.rotation = warpRotation;
            
            // CharacterControllerを再度有効化
            controller.enabled = true;
            
            if (enableDebugLog)
            {
                Debug.Log($"[GoalManager] CharacterController付きプレイヤーをワープ: {oldPosition} → {warpPosition}, 向きも変更: {oldRotation.eulerAngles} → {warpRotation.eulerAngles}");
            }
        }
        else
        {
            // 通常の位置と向きの設定
            playerRoot.transform.position = warpPosition;
            playerRoot.transform.rotation = warpRotation;
            
            if (enableDebugLog)
            {
                Debug.Log($"[GoalManager] プレイヤーをワープ: {oldPosition} → {warpPosition}, 向きも変更: {oldRotation.eulerAngles} → {warpRotation.eulerAngles}");
            }
        }
    }

    /// <summary>
    /// ゴール状態をリセット（テスト用）
    /// </summary>
    public void ResetGoal()
    {
        hasReachedGoal = false;
        
        if (enableDebugLog)
        {
            Debug.Log("[GoalManager] ゴール状態をリセットしました");
        }
    }

    /// <summary>
    /// 全てのゴール関連の状態をリセット（シーンリロード時用）
    /// </summary>
    public void ResetAllGoalState()
    {
        hasReachedGoal = false;
        allKeysCollected = false;
        
        // 鍵の収集状況もリセット
        ResetAllKeys();
        
        if (enableDebugLog)
        {
            Debug.Log("[GoalManager] 全てのゴール関連の状態をリセットしました");
        }
    }

    /// <summary>
    /// ゴールに到達したかどうか
    /// </summary>
    public bool HasReachedGoal()
    {
        return hasReachedGoal;
    }

    /// <summary>
    /// 手動でゴール処理を開始（テスト用）
    /// </summary>
    [ContextMenu("テスト: ゴール処理開始")]
    public void TestGoalSequence()
    {
        if (!hasReachedGoal)
        {
            hasReachedGoal = true;
            StartCoroutine(HandleGoalSequence());
        }
    }
    
    /// <summary>
    /// 扉を追加
    /// </summary>
    public void AddGoalDoor(GameObject door)
    {
        if (door != null && !goalDoors.Contains(door))
        {
            goalDoors.Add(door);
            
            // 輪郭エフェクトを追加
            OutlineEffect effect = door.GetComponent<OutlineEffect>();
            if (effect == null)
            {
                effect = door.AddComponent<OutlineEffect>();
            }
            
            effect.outlineColor = outlineColor;
            effect.outlineWidth = 0f;
            effect.visibleThroughWalls = visibleThroughWalls;
            effect.enabled = false;
            
            outlineEffects[door] = effect;
            
            if (enableDebugLog)
            {
                Debug.Log($"[GoalManager] 扉を追加しました: {door.name}");
            }
        }
    }
    
    /// <summary>
    /// 扉を削除
    /// </summary>
    public void RemoveGoalDoor(GameObject door)
    {
        if (door != null && goalDoors.Contains(door))
        {
            goalDoors.Remove(door);
            
            if (outlineEffects.ContainsKey(door))
            {
                OutlineEffect effect = outlineEffects[door];
                if (effect != null)
                {
                    Destroy(effect);
                }
                outlineEffects.Remove(door);
            }
            
            if (enableDebugLog)
            {
                Debug.Log($"[GoalManager] 扉を削除しました: {door.name}");
            }
        }
    }
    
    /// <summary>
    /// カギ収集メッセージを設定
    /// </summary>
    public void SetKeyCollectionMessage(string message)
    {
        keyCollectionMessage = message;
        if (keyCollectionText != null)
        {
            keyCollectionText.text = message;
        }
    }
    
    /// <summary>
    /// 鍵を手動で追加
    /// </summary>
    public void AddKey(KeyItem key)
    {
        if (key != null && !allKeys.Contains(key))
        {
            allKeys.Add(key);
            key.OnKeyCollected += OnKeyCollectedFromLinking;
            
            if (enableDebugLog)
            {
                Debug.Log($"[GoalManager] 鍵を追加しました: {key.name}");
            }
        }
    }
    
    /// <summary>
    /// ドアを手動で追加
    /// </summary>
    public void AddDoor(DoorController door)
    {
        if (door != null && !allDoors.Contains(door))
        {
            allDoors.Add(door);
            
            if (enableDebugLog)
            {
                Debug.Log($"[GoalManager] ドアを追加しました: {door.name}");
            }
        }
    }
    
    /// <summary>
    /// 鍵を削除
    /// </summary>
    public void RemoveKey(KeyItem key)
    {
        if (key != null && allKeys.Contains(key))
        {
            allKeys.Remove(key);
            key.OnKeyCollected -= OnKeyCollectedFromLinking;
            
            if (enableDebugLog)
            {
                Debug.Log($"[GoalManager] 鍵を削除しました: {key.name}");
            }
        }
    }
    
    /// <summary>
    /// ドアを削除
    /// </summary>
    public void RemoveDoor(DoorController door)
    {
        if (door != null && allDoors.Contains(door))
        {
            allDoors.Remove(door);
            
            if (enableDebugLog)
            {
                Debug.Log($"[GoalManager] ドアを削除しました: {door.name}");
            }
        }
    }
    
    /// <summary>
    /// 全ての鍵の収集状況をリセット
    /// </summary>
    public void ResetAllKeys()
    {
        foreach (DoorController door in allDoors)
        {
            if (door != null)
            {
                door.ResetKeyCollection();
            }
        }
        
        if (enableDebugLog)
        {
            Debug.Log("[GoalManager] 全ての鍵の収集状況をリセットしました");
        }
    }
    
    /// <summary>
    /// 輪郭強調機能のオンオフを設定
    /// </summary>
    public void SetOutlineEffectEnabled(bool enabled)
    {
        enableOutlineEffect = enabled;
        
        if (enabled)
        {
            // 有効化：既にカギが集まっている場合は輪郭を表示
            if (allKeysCollected)
            {
                EnableDoorOutlines();
            }
        }
        else
        {
            // 無効化：輪郭を非表示
            DisableDoorOutlines();
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[GoalManager] 輪郭強調機能を{(enabled ? "有効" : "無効")}にしました");
        }
    }
    
    /// <summary>
    /// 扉の輪郭を無効化
    /// </summary>
    private void DisableDoorOutlines()
    {
        foreach (var kvp in outlineEffects)
        {
            OutlineEffect effect = kvp.Value;
            if (effect != null)
            {
                effect.enabled = false;
            }
        }
    }
    
    private void OnDestroy()
    {
        // イベントの購読を解除
        if (doorController != null)
        {
            doorController.OnKeyCollected -= OnKeyCollected;
            doorController.OnDoorOpened -= OnDoorOpened;
        }
        
        // 鍵とドアの連携のイベントを解除
        foreach (KeyItem key in allKeys)
        {
            if (key != null)
            {
                key.OnKeyCollected -= OnKeyCollectedFromLinking;
            }
        }
    }
    
    /// <summary>
    /// テスト用：カギ収集ゴールを有効化
    /// </summary>
    [ContextMenu("テスト: カギ収集ゴールを有効化")]
    public void TestActivateKeyCollectionGoal()
    {
        ActivateKeyCollectionGoal();
    }
}


