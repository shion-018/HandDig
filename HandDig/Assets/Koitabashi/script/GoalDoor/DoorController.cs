using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 鍵や特定のタグが付いたオブジェクトを指定の数取ったらドアが開くコントローラー（回転ベース）
/// </summary>
public class DoorController : MonoBehaviour
{
    [System.Serializable]
    public class DoorPanel
    {
        [Header("ドアパネル設定")]
        public Transform doorTransform;
        public Vector3 rotationAxis = Vector3.up; // 回転軸（通常はY軸）
        public float openAngle = 90f; // 開く角度
        public float closedAngle = 0f; // 閉じた角度
        public float openSpeed = 90f; // 開く速度（度/秒）
        public float closeSpeed = 90f; // 閉じる速度（度/秒）
        public bool isOpen = false;
        public bool isMoving = false;
        
        [Header("回転の中心点")]
        public Vector3 pivotOffset = Vector3.zero; // ドアの回転中心からのオフセット
    }
    
    [Header("ドアパネル設定")]
    [SerializeField] private DoorPanel leftDoor = new DoorPanel();
    [SerializeField] private DoorPanel rightDoor = new DoorPanel();
    [SerializeField] private bool useBothDoors = true; // 両方のドアを使用するか
    
    [Header("鍵収集設定")]
    [SerializeField] private int requiredKeyCount = 3;
    [SerializeField] private string keyTag = "Key";
    [SerializeField] private List<GameObject> specificObjects = new List<GameObject>();
    
    [Header("テスト設定")]
    [SerializeField] private KeyCode testOpenKey = KeyCode.O;
    [SerializeField] private KeyCode testCloseKey = KeyCode.C;
    [SerializeField] private bool enableTestMode = true;
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugInfo = true;
    
    private bool isDoorOpen = false;
    private bool isMoving = false;
    private int collectedKeyCount = 0;
    private HashSet<GameObject> collectedObjects = new HashSet<GameObject>();
    
    // イベント
    public System.Action OnDoorOpened;
    public System.Action OnDoorClosed;
    public System.Action<int> OnKeyCollected; // 引数は現在の鍵の数
    
    private void Start()
    {
        InitializeDoors();
        
        if (showDebugInfo)
        {
            Debug.Log($"[DoorController] ドア初期化完了。必要鍵数: {requiredKeyCount}");
        }
    }
    
    /// <summary>
    /// ドアを初期化
    /// </summary>
    private void InitializeDoors()
    {
        // 左ドアの初期化
        if (leftDoor.doorTransform == null)
        {
            leftDoor.doorTransform = transform;
        }
        SetDoorRotation(leftDoor, leftDoor.closedAngle);
        
        // 右ドアの初期化
        if (rightDoor.doorTransform == null && useBothDoors)
        {
            rightDoor.doorTransform = transform;
        }
        if (useBothDoors)
        {
            SetDoorRotation(rightDoor, rightDoor.closedAngle);
        }
    }
    
    private void Update()
    {
        // テスト用キー入力
        if (enableTestMode)
        {
            if (Input.GetKeyDown(testOpenKey))
            {
                if (showDebugInfo)
                {
                    Debug.Log("[DoorController] テスト用：ドアを開く");
                }
                OpenDoor();
            }
            
            if (Input.GetKeyDown(testCloseKey))
            {
                if (showDebugInfo)
                {
                    Debug.Log("[DoorController] テスト用：ドアを閉じる");
                }
                CloseDoor();
            }
        }
    }
    
    /// <summary>
    /// 鍵やオブジェクトを収集する
    /// </summary>
    /// <param name="collectedObject">収集されたオブジェクト</param>
    public void CollectKey(GameObject collectedObject)
    {
        if (collectedObjects.Contains(collectedObject))
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"[DoorController] 既に収集済みのオブジェクトです: {collectedObject.name}");
            }
            return;
        }
        
        // タグで判定
        bool isKeyByTag = !string.IsNullOrEmpty(keyTag) && collectedObject.CompareTag(keyTag);
        
        // 特定オブジェクトリストで判定
        bool isKeyByList = specificObjects.Contains(collectedObject);
        
        if (isKeyByTag || isKeyByList)
        {
            collectedObjects.Add(collectedObject);
            collectedKeyCount++;
            
            if (showDebugInfo)
            {
                Debug.Log($"[DoorController] 鍵を収集しました: {collectedObject.name} (現在の鍵数: {collectedKeyCount}/{requiredKeyCount})");
            }
            
            OnKeyCollected?.Invoke(collectedKeyCount);
            
            // 必要な鍵数に達したらドアを開く
            if (collectedKeyCount >= requiredKeyCount && !isDoorOpen)
            {
                if (showDebugInfo)
                {
                    Debug.Log("[DoorController] 必要な鍵数に達しました。ドアを開きます。");
                }
                OpenDoor();
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"[DoorController] 鍵ではないオブジェクトです: {collectedObject.name}");
            }
        }
    }
    
    /// <summary>
    /// ドアを開く
    /// </summary>
    public void OpenDoor()
    {
        if (IsAnyDoorMoving()) return;
        
        StartCoroutine(RotateDoor(leftDoor, leftDoor.openAngle, leftDoor.openSpeed, true));
        if (useBothDoors)
        {
            StartCoroutine(RotateDoor(rightDoor, rightDoor.openAngle, rightDoor.openSpeed, true));
        }
    }
    
    /// <summary>
    /// ドアを閉じる
    /// </summary>
    public void CloseDoor()
    {
        if (IsAnyDoorMoving()) return;
        
        StartCoroutine(RotateDoor(leftDoor, leftDoor.closedAngle, leftDoor.closeSpeed, false));
        if (useBothDoors)
        {
            StartCoroutine(RotateDoor(rightDoor, rightDoor.closedAngle, rightDoor.closeSpeed, false));
        }
    }
    
    /// <summary>
    /// ドアを回転させるコルーチン
    /// </summary>
    private System.Collections.IEnumerator RotateDoor(DoorPanel doorPanel, float targetAngle, float speed, bool opening)
    {
        if (doorPanel.doorTransform == null) yield break;
        
        doorPanel.isMoving = true;
        float startAngle = GetCurrentDoorAngle(doorPanel);
        float angleDifference = Mathf.DeltaAngle(startAngle, targetAngle);
        float duration = Mathf.Abs(angleDifference) / speed;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            SetDoorRotation(doorPanel, currentAngle);
            yield return null;
        }
        
        SetDoorRotation(doorPanel, targetAngle);
        doorPanel.isMoving = false;
        doorPanel.isOpen = opening;
        
        // 全てのドアが動き終わったらイベントを発火
        if (!IsAnyDoorMoving())
        {
            if (opening)
            {
                OnDoorOpened?.Invoke();
                if (showDebugInfo)
                {
                    Debug.Log("[DoorController] ドアが開きました");
                }
            }
            else
            {
                OnDoorClosed?.Invoke();
                if (showDebugInfo)
                {
                    Debug.Log("[DoorController] ドアが閉じました");
                }
            }
        }
    }
    
    /// <summary>
    /// ドアの回転を設定
    /// </summary>
    private void SetDoorRotation(DoorPanel doorPanel, float angle)
    {
        if (doorPanel.doorTransform == null) return;
        
        Vector3 rotation = doorPanel.rotationAxis * angle;
        doorPanel.doorTransform.localRotation = Quaternion.Euler(rotation);
    }
    
    /// <summary>
    /// 現在のドアの角度を取得
    /// </summary>
    private float GetCurrentDoorAngle(DoorPanel doorPanel)
    {
        if (doorPanel.doorTransform == null) return 0f;
        
        Vector3 currentRotation = doorPanel.doorTransform.localRotation.eulerAngles;
        
        // 回転軸に応じて角度を取得
        if (doorPanel.rotationAxis.x > 0.5f)
            return currentRotation.x;
        else if (doorPanel.rotationAxis.y > 0.5f)
            return currentRotation.y;
        else if (doorPanel.rotationAxis.z > 0.5f)
            return currentRotation.z;
        
        return currentRotation.y; // デフォルトはY軸
    }
    
    /// <summary>
    /// いずれかのドアが動いているかチェック
    /// </summary>
    private bool IsAnyDoorMoving()
    {
        return leftDoor.isMoving || (useBothDoors && rightDoor.isMoving);
    }
    
    /// <summary>
    /// 鍵の収集状況をリセット
    /// </summary>
    public void ResetKeyCollection()
    {
        collectedKeyCount = 0;
        collectedObjects.Clear();
        
        if (showDebugInfo)
        {
            Debug.Log("[DoorController] 鍵の収集状況をリセットしました");
        }
    }
    
    /// <summary>
    /// 現在の鍵数を取得
    /// </summary>
    public int GetCurrentKeyCount()
    {
        return collectedKeyCount;
    }
    
    /// <summary>
    /// 必要な鍵数を取得
    /// </summary>
    public int GetRequiredKeyCount()
    {
        return requiredKeyCount;
    }
    
    /// <summary>
    /// ドアが開いているかどうか
    /// </summary>
    public bool IsDoorOpen()
    {
        return leftDoor.isOpen && (!useBothDoors || rightDoor.isOpen);
    }
    
    /// <summary>
    /// 特定オブジェクトをリストに追加
    /// </summary>
    public void AddSpecificObject(GameObject obj)
    {
        if (!specificObjects.Contains(obj))
        {
            specificObjects.Add(obj);
            if (showDebugInfo)
            {
                Debug.Log($"[DoorController] 特定オブジェクトを追加しました: {obj.name}");
            }
        }
    }
    
    /// <summary>
    /// 特定オブジェクトをリストから削除
    /// </summary>
    public void RemoveSpecificObject(GameObject obj)
    {
        if (specificObjects.Contains(obj))
        {
            specificObjects.Remove(obj);
            if (showDebugInfo)
            {
                Debug.Log($"[DoorController] 特定オブジェクトを削除しました: {obj.name}");
            }
        }
    }
    
    /// <summary>
    /// 左ドアの設定を取得
    /// </summary>
    public DoorPanel GetLeftDoor()
    {
        return leftDoor;
    }
    
    /// <summary>
    /// 右ドアの設定を取得
    /// </summary>
    public DoorPanel GetRightDoor()
    {
        return rightDoor;
    }
    
    /// <summary>
    /// 両方のドアを使用するかどうか
    /// </summary>
    public bool IsUsingBothDoors()
    {
        return useBothDoors;
    }
    
    /// <summary>
    /// 両方のドアを使用するかどうかを設定
    /// </summary>
    public void SetUseBothDoors(bool useBoth)
    {
        useBothDoors = useBoth;
    }
    
    private void OnDrawGizmosSelected()
    {
        // 左ドアの回転軸を可視化
        if (leftDoor.doorTransform != null)
        {
            Gizmos.color = Color.blue;
            Vector3 pivot = leftDoor.doorTransform.position + leftDoor.pivotOffset;
            Gizmos.DrawLine(pivot, pivot + leftDoor.rotationAxis * 2f);
            Gizmos.DrawWireSphere(pivot, 0.1f);
        }
        
        // 右ドアの回転軸を可視化
        if (useBothDoors && rightDoor.doorTransform != null)
        {
            Gizmos.color = Color.red;
            Vector3 pivot = rightDoor.doorTransform.position + rightDoor.pivotOffset;
            Gizmos.DrawLine(pivot, pivot + rightDoor.rotationAxis * 2f);
            Gizmos.DrawWireSphere(pivot, 0.1f);
        }
    }
}
