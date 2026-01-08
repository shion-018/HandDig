using UnityEngine;

/// <summary>
/// ドアを開けるためのトリガー
/// コライダーが別のオブジェクトにある場合に使用
/// </summary>
[RequireComponent(typeof(Collider))]
public class DoorTrigger : MonoBehaviour
{
    [Header("ドア設定")]
    [Tooltip("連携するDoorController")]
    [SerializeField] private DoorController doorController;
    
    [Tooltip("自動検出するか")]
    [SerializeField] private bool autoDetectDoorController = true;
    
    [Header("プレイヤー設定")]
    [Tooltip("プレイヤーのタグ")]
    [SerializeField] private string playerTag = "Player";
    
    [Header("デバッグ")]
    [Tooltip("デバッグログを出力するか")]
    [SerializeField] private bool showDebugInfo = true;
    
    private void Start()
    {
        // ColliderのIsTriggerを有効化
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("[DoorTrigger] Colliderが見つかりません");
            }
        }
        
        // DoorControllerを自動検出
        if (autoDetectDoorController && doorController == null)
        {
            doorController = FindObjectOfType<DoorController>();
        }
        
        if (doorController == null && showDebugInfo)
        {
            Debug.LogWarning("[DoorTrigger] DoorControllerが見つかりません");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (doorController == null) return;
        
        if (showDebugInfo)
        {
            Debug.Log($"[DoorTrigger] OnTriggerEnter: {other.name}, Tag: {other.tag}");
        }
        
        // 鍵が全部集まっているかチェック
        int currentKeyCount = doorController.GetCurrentKeyCount();
        int requiredKeyCount = doorController.GetRequiredKeyCount();
        
        if (currentKeyCount < requiredKeyCount)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[DoorTrigger] 鍵が不足しています: {currentKeyCount}/{requiredKeyCount}");
            }
            return;
        }
        
        // 既にドアが開いているかチェック
        if (doorController.IsDoorOpen())
        {
            if (showDebugInfo)
            {
                Debug.Log("[DoorTrigger] ドアは既に開いています");
            }
            return;
        }
        
        // プレイヤーかどうかを判定
        bool isPlayer = other.CompareTag(playerTag);
        
        if (!isPlayer)
        {
            // プレイヤーの子オブジェクトかチェック
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null && other.transform.IsChildOf(playerObj.transform))
            {
                isPlayer = true;
                if (showDebugInfo)
                {
                    Debug.Log($"[DoorTrigger] プレイヤーの子オブジェクトとして検出: {other.name}");
                }
            }
            
            // CharacterControllerを持つかチェック（VRプレイヤーの場合）
            if (!isPlayer && other.GetComponent<CharacterController>() != null)
            {
                isPlayer = true;
                if (showDebugInfo)
                {
                    Debug.Log($"[DoorTrigger] CharacterControllerとして検出: {other.name}");
                }
            }
        }
        
        if (isPlayer)
        {
            if (showDebugInfo)
            {
                Debug.Log("[DoorTrigger] プレイヤーが接近しました。鍵が全部集まっているのでドアを開きます。");
            }
            doorController.OpenDoor();
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"[DoorTrigger] プレイヤーではありません: {other.name}");
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // CharacterControllerはOnCollisionEnterに反応しないが、念のため実装
        if (doorController == null) return;
        
        if (showDebugInfo)
        {
            Debug.Log($"[DoorTrigger] OnCollisionEnter: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        }
        
        // 鍵が全部集まっているかチェック
        int currentKeyCount = doorController.GetCurrentKeyCount();
        int requiredKeyCount = doorController.GetRequiredKeyCount();
        
        if (currentKeyCount < requiredKeyCount)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[DoorTrigger] 鍵が不足しています: {currentKeyCount}/{requiredKeyCount}");
            }
            return;
        }
        
        // 既にドアが開いているかチェック
        if (doorController.IsDoorOpen())
        {
            if (showDebugInfo)
            {
                Debug.Log("[DoorTrigger] ドアは既に開いています");
            }
            return;
        }
        
        // プレイヤーかどうかを判定
        bool isPlayer = collision.gameObject.CompareTag(playerTag);
        
        if (!isPlayer)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null && collision.transform.IsChildOf(playerObj.transform))
            {
                isPlayer = true;
            }
        }
        
        if (isPlayer)
        {
            if (showDebugInfo)
            {
                Debug.Log("[DoorTrigger] プレイヤーが衝突しました。鍵が全部集まっているのでドアを開きます。");
            }
            doorController.OpenDoor();
        }
    }
    
    private void Reset()
    {
        // エディタでコンポーネントを追加した時に自動でIsTriggerを有効化
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
}

