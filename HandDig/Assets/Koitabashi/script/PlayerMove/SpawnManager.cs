using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("スポーン設定")]
    [Tooltip("利用するスポーンポイント")]
    public SpawnPointMarker spawnPoint;
    
    [Tooltip("プレイヤーのルートオブジェクト")]
    public GameObject playerRoot;
    
    [Tooltip("ワールドマネージャー")]
    public MC_World worldManager;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグ情報を表示するかどうか")]
    public bool showDebugInfo = true;

    private SpawnPointMarker selectedSpawnPoint;
    void Start()
    {
        // スポーンポイントが設定されているかチェック
        if (spawnPoint == null)
        {
            spawnPoint = GetComponentInChildren<SpawnPointMarker>();
        }

        if (spawnPoint == null)
        {
            Debug.LogError("[SpawnManager] スポーンポイントが設定されていません。");
            return;
        }

        selectedSpawnPoint = spawnPoint;
        if (showDebugInfo)
        {
            Debug.Log($"[SpawnManager] 固定スポーンポイントを使用: {selectedSpawnPoint.spawnPointName}");
        }
        
        Debug.Log("[SpawnManager] 初期化完了。プレイヤーのスポーンはワールド生成完了後に実行されます。");
    }

    /// <summary>
    /// ワールド生成完了後にプレイヤーを最終スポーン位置に移動
    /// </summary>
    public void SpawnPlayerAtFinalPosition()
    {
        if (selectedSpawnPoint == null || playerRoot == null)
        {
            Debug.LogError("[SpawnManager] 必要なコンポーネントが設定されていません");
            return;
        }
        
        // 選択されたスポーンポイントでプレイヤーをスポーン
        selectedSpawnPoint.SpawnPlayer(playerRoot);
        
        Debug.Log("[SpawnManager] プレイヤーを最終スポーン位置に移動しました");
    }

    /// <summary>
    /// 現在選択されているスポーンポイントを取得
    /// </summary>
    public SpawnPointMarker GetSelectedSpawnPoint()
    {
        return selectedSpawnPoint;
    }

    /// <summary>
    /// 現在設定されているスポーン位置を取得
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        return selectedSpawnPoint != null ? selectedSpawnPoint.transform.position : transform.position;
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo || selectedSpawnPoint == null) return;
        
        // 選択されたスポーンポイントを強調表示
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(selectedSpawnPoint.transform.position, 3f);
    }
} 