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
    
    [Header("輪郭強調設定")]
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
        InitializeOutlineEffects();
        
        if (enableDebugLog)
        {
            Debug.Log($"[GoalManager] カギ収集機能初期化完了。扉数: {goalDoors.Count}");
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
            keyCollectionText.color = keyCollectionTextColor;
            keyCollectionText.fontSize = keyCollectionTextSize;
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
        if (!allKeysCollected && enableKeyCollectionFeature)
        {
            ActivateKeyCollectionGoal();
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
        
        // 扉の輪郭を強調
        EnableDoorOutlines();
    }
    
    /// <summary>
    /// カギ収集UIを表示
    /// </summary>
    private void ShowKeyCollectionUI()
    {
        if (keyCollectionCanvas != null)
        {
            keyCollectionCanvas.gameObject.SetActive(true);
        }
        
        if (keyCollectionText != null)
        {
            keyCollectionText.text = keyCollectionMessage;
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
        // カギ収集時の輪郭強度を更新
        if (enableKeyCollectionFeature && allKeysCollected && playerTransform != null)
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
    /// プレイヤーをゴール位置にワープ
    /// </summary>
    private void WarpPlayerToGoal()
    {
        if (playerRoot == null || goalWarpPoint == null) return;

        Vector3 warpPosition = goalWarpPoint.position;
        Vector3 oldPosition = playerRoot.transform.position;

        // CharacterControllerがある場合は特別な処理
        CharacterController controller = playerRoot.GetComponent<CharacterController>();
        if (controller != null)
        {
            // CharacterControllerを一時的に無効化
            controller.enabled = false;
            
            // 位置を設定
            playerRoot.transform.position = warpPosition;
            
            // CharacterControllerを再度有効化
            controller.enabled = true;
            
            if (enableDebugLog)
            {
                Debug.Log($"[GoalManager] CharacterController付きプレイヤーをワープ: {oldPosition} → {warpPosition}");
            }
        }
        else
        {
            // 通常の位置設定
            playerRoot.transform.position = warpPosition;
            
            if (enableDebugLog)
            {
                Debug.Log($"[GoalManager] プレイヤーをワープ: {oldPosition} → {warpPosition}");
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
    
    private void OnDestroy()
    {
        // イベントの購読を解除
        if (doorController != null)
        {
            doorController.OnKeyCollected -= OnKeyCollected;
            doorController.OnDoorOpened -= OnDoorOpened;
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


