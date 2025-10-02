using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 鍵とドアの連携を管理するマネージャー
/// シーン内の鍵とドアを自動的に検出・連携させる
/// </summary>
public class KeyDoorManager : MonoBehaviour
{
    [Header("自動検出設定")]
    [SerializeField] private bool autoDetectKeys = true;
    [SerializeField] private bool autoDetectDoors = true;
    [SerializeField] private string keyTag = "Key";
    [SerializeField] private string doorTag = "Door";
    
    [Header("手動設定")]
    [SerializeField] private List<KeyItem> manualKeys = new List<KeyItem>();
    [SerializeField] private List<DoorController> manualDoors = new List<DoorController>();
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugInfo = true;
    
    private List<KeyItem> allKeys = new List<KeyItem>();
    private List<DoorController> allDoors = new List<DoorController>();
    
    // イベント
    public System.Action<KeyItem> OnKeyRegistered;
    public System.Action<DoorController> OnDoorRegistered;
    
    private void Start()
    {
        InitializeManager();
    }
    
    /// <summary>
    /// マネージャーを初期化
    /// </summary>
    private void InitializeManager()
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
        
        if (showDebugInfo)
        {
            Debug.Log($"[KeyDoorManager] 初期化完了。鍵数: {allKeys.Count}, ドア数: {allDoors.Count}");
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
                OnKeyRegistered?.Invoke(keyItem);
            }
        }
        
        // KeyItemコンポーネントで検出
        KeyItem[] allKeyItems = FindObjectsOfType<KeyItem>();
        foreach (KeyItem keyItem in allKeyItems)
        {
            if (!allKeys.Contains(keyItem))
            {
                allKeys.Add(keyItem);
                OnKeyRegistered?.Invoke(keyItem);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[KeyDoorManager] 自動検出で{allKeys.Count}個の鍵を発見しました");
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
                OnDoorRegistered?.Invoke(doorController);
            }
        }
        
        // DoorControllerコンポーネントで検出
        DoorController[] allDoorControllers = FindObjectsOfType<DoorController>();
        foreach (DoorController doorController in allDoorControllers)
        {
            if (!allDoors.Contains(doorController))
            {
                allDoors.Add(doorController);
                OnDoorRegistered?.Invoke(doorController);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[KeyDoorManager] 自動検出で{allDoors.Count}個のドアを発見しました");
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
                OnKeyRegistered?.Invoke(key);
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
                OnDoorRegistered?.Invoke(door);
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
                key.OnKeyCollected += OnKeyCollected;
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[KeyDoorManager] 鍵とドアの連携を設定しました");
        }
    }
    
    /// <summary>
    /// 鍵が収集された時の処理
    /// </summary>
    private void OnKeyCollected(KeyItem collectedKey)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[KeyDoorManager] 鍵が収集されました: {collectedKey.name}");
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
    /// 鍵を手動で追加
    /// </summary>
    public void AddKey(KeyItem key)
    {
        if (key != null && !allKeys.Contains(key))
        {
            allKeys.Add(key);
            key.OnKeyCollected += OnKeyCollected;
            OnKeyRegistered?.Invoke(key);
            
            if (showDebugInfo)
            {
                Debug.Log($"[KeyDoorManager] 鍵を追加しました: {key.name}");
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
            OnDoorRegistered?.Invoke(door);
            
            if (showDebugInfo)
            {
                Debug.Log($"[KeyDoorManager] ドアを追加しました: {door.name}");
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
            key.OnKeyCollected -= OnKeyCollected;
            
            if (showDebugInfo)
            {
                Debug.Log($"[KeyDoorManager] 鍵を削除しました: {key.name}");
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
            
            if (showDebugInfo)
            {
                Debug.Log($"[KeyDoorManager] ドアを削除しました: {door.name}");
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
        
        if (showDebugInfo)
        {
            Debug.Log("[KeyDoorManager] 全ての鍵の収集状況をリセットしました");
        }
    }
    
    /// <summary>
    /// 全てのドアを開く（テスト用）
    /// </summary>
    public void OpenAllDoors()
    {
        foreach (DoorController door in allDoors)
        {
            if (door != null)
            {
                door.OpenDoor();
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[KeyDoorManager] 全てのドアを開きました");
        }
    }
    
    /// <summary>
    /// 全てのドアを閉じる（テスト用）
    /// </summary>
    public void CloseAllDoors()
    {
        foreach (DoorController door in allDoors)
        {
            if (door != null)
            {
                door.CloseDoor();
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[KeyDoorManager] 全てのドアを閉じました");
        }
    }
    
    /// <summary>
    /// 現在の鍵数を取得
    /// </summary>
    public int GetTotalKeyCount()
    {
        return allKeys.Count;
    }
    
    /// <summary>
    /// 現在のドア数を取得
    /// </summary>
    public int GetTotalDoorCount()
    {
        return allDoors.Count;
    }
    
    private void OnDestroy()
    {
        // イベントの購読を解除
        foreach (KeyItem key in allKeys)
        {
            if (key != null)
            {
                key.OnKeyCollected -= OnKeyCollected;
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // 鍵の位置を可視化
        Gizmos.color = Color.yellow;
        foreach (KeyItem key in allKeys)
        {
            if (key != null)
            {
                Gizmos.DrawWireSphere(key.transform.position, 0.5f);
            }
        }
        
        // ドアの位置を可視化
        Gizmos.color = Color.blue;
        foreach (DoorController door in allDoors)
        {
            if (door != null)
            {
                Gizmos.DrawWireCube(door.transform.position, Vector3.one);
            }
        }
    }
}
