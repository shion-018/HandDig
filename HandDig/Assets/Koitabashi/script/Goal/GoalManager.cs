using System.Collections;
using UnityEngine;

/// <summary>
/// ゴール時の処理を管理するマネージャー
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
    
    [Header("デバッグ")]
    [Tooltip("デバッグログを出力するか")]
    public bool enableDebugLog = true;

    private static GoalManager instance;
    private bool hasReachedGoal = false;
    
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

        if (enableDebugLog)
        {
            Debug.Log($"[GoalManager] プレイヤー設定: {(playerRoot != null ? playerRoot.name : "未設定")}");
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

        // 1. フェードアウト
        if (useFade && FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeOut(fadeOutDuration);
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

        // 4. ゴールUI表示
        if (GoalUI.Instance != null)
        {
            GoalUI.Instance.ShowGoalUI();
            
            // UI表示時間待機
            yield return new WaitForSeconds(goalUIDisplayDuration);
            
            GoalUI.Instance.HideGoalUI();
        }

        if (enableDebugLog)
        {
            Debug.Log("[GoalManager] ゴール処理完了");
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
}

