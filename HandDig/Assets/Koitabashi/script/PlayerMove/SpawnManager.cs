using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("スポーン設定")]
    [Tooltip("利用可能なスポーンポイントのリスト")]
    public List<SpawnPointMarker> spawnPoints = new List<SpawnPointMarker>();
    
    [Tooltip("プレイヤーのルートオブジェクト")]
    public GameObject playerRoot;
    
    [Tooltip("ワールドマネージャー")]
    public MC_World worldManager;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグ情報を表示するかどうか")]
    public bool showDebugInfo = true;

    private SpawnPointMarker selectedSpawnPoint;
    private List<Vector3Int> excludedChunks = new List<Vector3Int>();

    void Start()
    {
        // スポーンポイントが設定されているかチェック
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("[SpawnManager] スポーンポイントが設定されていません。固定スポーンを使用します。");
            return;
        }

        // ランダムでスポーンポイントを選択
        SelectRandomSpawnPoint();
        
        // 選択されたスポーンポイントの周囲9チャンクを除外
        SetupExcludedChunks();
        
        // プレイヤーのスポーンはMC_Worldが完了後に実行される
        Debug.Log("[SpawnManager] 初期化完了。プレイヤーのスポーンはワールド生成完了後に実行されます。");
    }

    /// <summary>
    /// ランダムでスポーンポイントを選択
    /// </summary>
    private void SelectRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return;
        
        int randomIndex = Random.Range(0, spawnPoints.Count);
        selectedSpawnPoint = spawnPoints[randomIndex];
        
        if (showDebugInfo)
        {
            Debug.Log($"[SpawnManager] スポーンポイントを選択: {selectedSpawnPoint.spawnPointName}");
        }
    }

    /// <summary>
    /// 選択されたスポーンポイントの周囲9チャンクを除外設定
    /// </summary>
    private void SetupExcludedChunks()
    {
        if (selectedSpawnPoint == null || worldManager == null) return;
        
        // 除外するチャンク座標を取得
        excludedChunks = selectedSpawnPoint.GetExcludedChunks(worldManager.chunkSize);
        
        // 各TreasureSpawnerに除外チャンクを設定
        foreach (var spawner in worldManager.treasureSpawners)
        {
            if (spawner != null)
            {
                foreach (var chunkCoord in excludedChunks)
                {
                    spawner.AddExcludedChunk(chunkCoord);
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[SpawnManager] {excludedChunks.Count}個のチャンクを除外設定しました");
        }
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
    /// 除外されているチャンクのリストを取得
    /// </summary>
    public List<Vector3Int> GetExcludedChunks()
    {
        return excludedChunks;
    }

    /// <summary>
    /// 手動でスポーンポイントを選択（デバッグ用）
    /// </summary>
    [ContextMenu("ランダムスポーン再実行")]
    public void RespawnRandom()
    {
        if (spawnPoints.Count == 0) return;
        
        SelectRandomSpawnPoint();
        SetupExcludedChunks();
        SpawnPlayerAtFinalPosition();
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo || selectedSpawnPoint == null) return;
        
        // 選択されたスポーンポイントを強調表示
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(selectedSpawnPoint.transform.position, 3f);
        
        // 除外チャンクの範囲を表示
        if (worldManager != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            foreach (var chunkCoord in excludedChunks)
            {
                Vector3 chunkWorldPos = new Vector3(
                    chunkCoord.x * worldManager.chunkSize,
                    chunkCoord.y * worldManager.chunkSize,
                    chunkCoord.z * worldManager.chunkSize
                );
                Gizmos.DrawWireCube(
                    chunkWorldPos + Vector3.one * worldManager.chunkSize * 0.5f,
                    Vector3.one * worldManager.chunkSize
                );
            }
        }
    }
} 