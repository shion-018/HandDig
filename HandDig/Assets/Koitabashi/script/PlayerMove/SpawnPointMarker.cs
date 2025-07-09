using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointMarker : MonoBehaviour
{
    [Header("スポーンポイント設定")]
    [Tooltip("このスポーンポイントの名前（デバッグ用）")]
    public string spawnPointName = "SpawnPoint";
    
    [Header("視覚的設定")]
    [Tooltip("エディタで表示するマーカーの色")]
    public Color markerColor = Color.green;
    
    [Tooltip("エディタで表示するマーカーのサイズ")]
    public float markerSize = 2f;
    
    [Tooltip("エディタで表示する掘り範囲のサイズ（DigVolume用）")]
    public float digRangeSize = 8f;

    private void OnDrawGizmos()
    {
        // エディタでスポーンポイントを視覚化
        Gizmos.color = markerColor;
        Gizmos.DrawWireSphere(transform.position, markerSize);
        
        // 掘り範囲も表示（DigVolume用）
        Gizmos.color = new Color(markerColor.r, markerColor.g, markerColor.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, digRangeSize);
        
        // スポーンポイントの名前を表示
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * markerSize, spawnPointName);
        #endif
    }

    /// <summary>
    /// このスポーンポイントでプレイヤーをスポーンする
    /// </summary>
    /// <param name="playerRoot">プレイヤーのルートオブジェクト</param>
    public void SpawnPlayer(GameObject playerRoot)
    {
        // スポーンポイントの位置を取得
        Vector3 spawnPosition = transform.position;
        
        // プレイヤーをスポーン位置に移動
        playerRoot.transform.position = spawnPosition + Vector3.up * 2f; // 少し上に配置
        
        Debug.Log($"[SpawnPointMarker] プレイヤーをスポーン: {spawnPointName} at {spawnPosition}");
    }

    /// <summary>
    /// このスポーンポイントの周囲9チャンクの座標を取得
    /// </summary>
    /// <param name="chunkSize">チャンクサイズ</param>
    /// <returns>除外するチャンク座標のリスト</returns>
    public List<Vector3Int> GetExcludedChunks(int chunkSize)
    {
        List<Vector3Int> excludedChunks = new List<Vector3Int>();
        
        // スポーンポイントのチャンク座標を計算
        Vector3Int centerChunk = new Vector3Int(
            Mathf.FloorToInt(transform.position.x / chunkSize),
            Mathf.FloorToInt(transform.position.y / chunkSize),
            Mathf.FloorToInt(transform.position.z / chunkSize)
        );
        
        // 周囲9チャンクを除外リストに追加
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                Vector3Int excludedChunk = new Vector3Int(
                    centerChunk.x + dx,
                    centerChunk.y,
                    centerChunk.z + dz
                );
                excludedChunks.Add(excludedChunk);
            }
        }
        
        return excludedChunks;
    }
} 