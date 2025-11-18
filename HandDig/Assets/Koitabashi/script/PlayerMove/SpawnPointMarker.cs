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
        Vector3 oldPosition = playerRoot.transform.position;
        
        // CharacterControllerがある場合は特別な処理
        CharacterController controller = playerRoot.GetComponent<CharacterController>();
        if (controller != null)
        {
            // CharacterControllerを一時的に無効化
            controller.enabled = false;
            
            // 位置を設定
            playerRoot.transform.position = spawnPosition + Vector3.up * 2f;
            
            // CharacterControllerを再度有効化
            controller.enabled = true;
            
            Debug.Log($"[SpawnPointMarker] CharacterController付きプレイヤーをスポーン: {spawnPointName} at {spawnPosition}");
        }
        else
        {
            // 通常の位置設定
            playerRoot.transform.position = spawnPosition + Vector3.up * 2f;
            Debug.Log($"[SpawnPointMarker] プレイヤーをスポーン: {spawnPointName} at {spawnPosition}");
        }
        
        Vector3 newPosition = playerRoot.transform.position;
        Debug.Log($"[SpawnPointMarker] 位置変更: {oldPosition} → {newPosition}");
    }

} 