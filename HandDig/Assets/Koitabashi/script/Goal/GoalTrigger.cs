using UnityEngine;

/// <summary>
/// ゴール判定のトリガーを管理するスクリプト
/// 既存のDoorControllerと連携してゴール処理を実行
/// </summary>
public class GoalTrigger : MonoBehaviour
{
    [Header("ゴール設定")]
    [Tooltip("ゴール時のワープ先位置")]
    public Transform goalWarpPoint;
    
    [Tooltip("プレイヤーのルートオブジェクト")]
    public GameObject playerRoot;
    
    [Header("連携設定")]
    [Tooltip("連携するDoorController")]
    public DoorController doorController;
    
    [Tooltip("ドアが開いた後にゴール判定を有効にするか")]
    public bool requireDoorOpen = true;
    
    [Header("デバッグ")]
    [Tooltip("デバッグログを出力するか")]
    public bool enableDebugLog = true;

    private bool hasReachedGoal = false;
    private bool isGoalEnabled = false;

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

        // DoorControllerとの連携設定
        if (doorController != null)
        {
            if (requireDoorOpen)
            {
                // ドアが開いた時にゴール判定を有効化
                doorController.OnDoorOpened += EnableGoal;
                isGoalEnabled = false;
            }
            else
            {
                // 常にゴール判定を有効
                isGoalEnabled = true;
            }
        }
        else
        {
            // DoorControllerがない場合は常に有効
            isGoalEnabled = true;
        }

        if (enableDebugLog)
        {
            Debug.Log($"[GoalTrigger] 初期化完了 - プレイヤー: {(playerRoot != null ? playerRoot.name : "未設定")}, ゴール有効: {isGoalEnabled}");
        }
    }

    private void OnDestroy()
    {
        // イベントの購読解除
        if (doorController != null)
        {
            doorController.OnDoorOpened -= EnableGoal;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ゴール判定が有効で、プレイヤーがゴールに到達した場合
        if (!hasReachedGoal && isGoalEnabled && GoalManager.Instance != null && GoalManager.Instance.IsPlayerObject(other.gameObject))
        {
            hasReachedGoal = true;
            if (enableDebugLog)
            {
                Debug.Log("[GoalTrigger] ゴール到達を検知。GoalManagerへ委譲します。");
            }
            GoalManager.Instance.StartGoalSequence(goalWarpPoint, playerRoot);
        }
    }

    /// <summary>
    /// ゴール判定を有効化（DoorControllerから呼び出される）
    /// </summary>
    private void EnableGoal()
    {
        isGoalEnabled = true;
        
        if (enableDebugLog)
        {
            Debug.Log("[GoalTrigger] ドアが開いたため、ゴール判定を有効化しました");
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
            Debug.Log("[GoalTrigger] ゴール状態をリセットしました");
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
    /// ゴール判定が有効かどうか
    /// </summary>
    public bool IsGoalEnabled()
    {
        return isGoalEnabled;
    }

    /// <summary>
    /// 手動でゴール判定を有効化
    /// </summary>
    public void EnableGoalManually()
    {
        EnableGoal();
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
            if (GoalManager.Instance != null)
            {
                GoalManager.Instance.StartGoalSequence(goalWarpPoint, playerRoot);
            }
        }
    }
}



