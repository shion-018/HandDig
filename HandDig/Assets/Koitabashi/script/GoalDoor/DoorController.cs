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
    
    [Header("開き方設定")]
    [Tooltip("アニメーションを使用するか（falseの場合は回転方式）")]
    [SerializeField] private bool useAnimation = false;
    
    [Tooltip("ドアのAnimator（アニメーション方式の場合）")]
    [SerializeField] private Animator doorAnimator;
    
    [Tooltip("開くアニメーションのトリガー名")]
    [SerializeField] private string openAnimationTrigger = "Open";
    
    [Tooltip("閉じるアニメーションのトリガー名")]
    [SerializeField] private string closeAnimationTrigger = "Close";
    
    [Tooltip("開くアニメーションのブールパラメータ名（トリガーの代わりに使用可）")]
    [SerializeField] private string isOpenBoolParameter = "IsOpen";
    
    [Header("プレイヤー接近検知")]
    [Tooltip("プレイヤーが近づいたときに自動で開くか")]
    [SerializeField] private bool openOnPlayerApproach = false;
    
    [Tooltip("プレイヤー検知用のコライダー（IsTriggerを有効にすること）")]
    [SerializeField] private Collider approachTrigger;
    
    [Tooltip("自動検出するか（このオブジェクトのColliderを使用）")]
    [SerializeField] private bool autoDetectTrigger = true;
    
    [Tooltip("プレイヤーのタグ")]
    [SerializeField] private string playerTag = "Player";
    
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
        
        // アニメーション方式の場合
        if (useAnimation)
        {
            InitializeAnimation();
        }
        
        // プレイヤー接近検知の初期化
        if (openOnPlayerApproach)
        {
            InitializeApproachTrigger();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[DoorController] ドア初期化完了。必要鍵数: {requiredKeyCount}, アニメーション方式: {useAnimation}, 接近検知: {openOnPlayerApproach}");
        }
    }
    
    /// <summary>
    /// アニメーションを初期化
    /// </summary>
    private void InitializeAnimation()
    {
        // Animatorを自動検出
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
            if (doorAnimator == null && leftDoor.doorTransform != null)
            {
                doorAnimator = leftDoor.doorTransform.GetComponent<Animator>();
            }
        }
        
        if (doorAnimator == null)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("[DoorController] Animatorが見つかりません。アニメーション方式は使用できません。");
            }
            useAnimation = false;
        }
    }
    
    /// <summary>
    /// 接近検知トリガーを初期化
    /// </summary>
    private void InitializeApproachTrigger()
    {
        // コライダーを自動検出
        if (approachTrigger == null && autoDetectTrigger)
        {
            approachTrigger = GetComponent<Collider>();
            if (approachTrigger == null)
            {
                // 子オブジェクトから検索
                approachTrigger = GetComponentInChildren<Collider>();
            }
        }
        
        if (approachTrigger != null)
        {
            // IsTriggerを有効化
            approachTrigger.isTrigger = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"[DoorController] 接近検知トリガーを設定: {approachTrigger.name}, IsTrigger: {approachTrigger.isTrigger}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("[DoorController] 接近検知用のコライダーが見つかりません。このオブジェクトまたは子オブジェクトにColliderを追加してください。");
            }
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
        
        // 既存の回転を保持（closedAngleが0で、ドアに既に回転がかかっている場合は現在の角度を使用）
        if (leftDoor.doorTransform != null)
        {
            float currentAngle = GetCurrentDoorAngle(leftDoor);
            // closedAngleが0で、現在の角度が0でない場合は、現在の角度をclosedAngleとして使用
            if (leftDoor.closedAngle == 0f && Mathf.Abs(currentAngle) > 0.01f)
            {
                leftDoor.closedAngle = currentAngle;
                if (showDebugInfo)
                {
                    Debug.Log($"[DoorController] 左ドアの既存の回転を保持: {currentAngle}度");
                }
            }
            // アニメーション方式でない場合のみ回転を設定（アニメーション方式の場合は回転を変更しない）
            if (!useAnimation)
            {
                SetDoorRotation(leftDoor, leftDoor.closedAngle);
            }
        }
        
        // 右ドアの初期化
        if (rightDoor.doorTransform == null && useBothDoors)
        {
            rightDoor.doorTransform = transform;
        }
        if (useBothDoors && rightDoor.doorTransform != null)
        {
            // 既存の回転を保持
            float currentAngle = GetCurrentDoorAngle(rightDoor);
            if (rightDoor.closedAngle == 0f && Mathf.Abs(currentAngle) > 0.01f)
            {
                rightDoor.closedAngle = currentAngle;
                if (showDebugInfo)
                {
                    Debug.Log($"[DoorController] 右ドアの既存の回転を保持: {currentAngle}度");
                }
            }
            // アニメーション方式でない場合のみ回転を設定
            if (!useAnimation)
            {
                SetDoorRotation(rightDoor, rightDoor.closedAngle);
            }
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
            
            // 鍵を集めただけではドアは開かない（コライダーに触れた時だけ開く）
            if (collectedKeyCount >= requiredKeyCount && showDebugInfo)
            {
                Debug.Log($"[DoorController] 必要な鍵数に達しました ({collectedKeyCount}/{requiredKeyCount})。コライダーに触れるとドアが開きます。");
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
        if (isDoorOpen) return;
        
        // ゴールドア開放音を再生（ドアの位置から）
        if (DigSoundManager.Instance != null)
        {
            DigSoundManager.Instance.PlayGoalDoorOpenSound(transform.position);
        }
        
        if (useAnimation)
        {
            OpenDoorWithAnimation();
        }
        else
        {
            OpenDoorWithRotation();
        }
    }
    
    /// <summary>
    /// アニメーションでドアを開く
    /// </summary>
    private void OpenDoorWithAnimation()
    {
        if (doorAnimator == null) return;
        
        // ブールパラメータを使用する場合
        if (!string.IsNullOrEmpty(isOpenBoolParameter))
        {
            doorAnimator.SetBool(isOpenBoolParameter, true);
        }
        
        // トリガーを使用する場合
        if (!string.IsNullOrEmpty(openAnimationTrigger))
        {
            doorAnimator.SetTrigger(openAnimationTrigger);
        }
        
        isDoorOpen = true;
        
        // アニメーション完了を待つ（簡易版：アニメーションの長さを取得）
        if (doorAnimator.GetCurrentAnimatorStateInfo(0).length > 0)
        {
            StartCoroutine(WaitForAnimationComplete(true));
        }
        else
        {
            // アニメーション情報が取得できない場合は即座にイベント発火
            OnDoorOpened?.Invoke();
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[DoorController] アニメーションでドアを開きます");
        }
    }
    
    /// <summary>
    /// 回転でドアを開く（既存の処理）
    /// </summary>
    private void OpenDoorWithRotation()
    {
        if (IsAnyDoorMoving()) return;
        
        StartCoroutine(RotateDoor(leftDoor, leftDoor.openAngle, leftDoor.openSpeed, true));
        if (useBothDoors)
        {
            StartCoroutine(RotateDoor(rightDoor, rightDoor.openAngle, rightDoor.openSpeed, true));
        }
    }
    
    /// <summary>
    /// アニメーション完了を待つ
    /// </summary>
    private System.Collections.IEnumerator WaitForAnimationComplete(bool opening)
    {
        if (doorAnimator == null) yield break;
        
        // アニメーションの長さを取得
        AnimatorStateInfo stateInfo = doorAnimator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        
        yield return new WaitForSeconds(animationLength);
        
        if (opening)
        {
            OnDoorOpened?.Invoke();
            if (showDebugInfo)
            {
                Debug.Log("[DoorController] ドアが開きました（アニメーション完了）");
            }
        }
        else
        {
            OnDoorClosed?.Invoke();
            if (showDebugInfo)
            {
                Debug.Log("[DoorController] ドアが閉じました（アニメーション完了）");
            }
        }
    }
    
    /// <summary>
    /// ドアを閉じる
    /// </summary>
    public void CloseDoor()
    {
        if (!isDoorOpen) return;
        
        if (useAnimation)
        {
            CloseDoorWithAnimation();
        }
        else
        {
            CloseDoorWithRotation();
        }
    }
    
    /// <summary>
    /// アニメーションでドアを閉じる
    /// </summary>
    private void CloseDoorWithAnimation()
    {
        if (doorAnimator == null) return;
        
        // ブールパラメータを使用する場合
        if (!string.IsNullOrEmpty(isOpenBoolParameter))
        {
            doorAnimator.SetBool(isOpenBoolParameter, false);
        }
        
        // トリガーを使用する場合
        if (!string.IsNullOrEmpty(closeAnimationTrigger))
        {
            doorAnimator.SetTrigger(closeAnimationTrigger);
        }
        
        isDoorOpen = false;
        
        // アニメーション完了を待つ
        if (doorAnimator.GetCurrentAnimatorStateInfo(0).length > 0)
        {
            StartCoroutine(WaitForAnimationComplete(false));
        }
        else
        {
            OnDoorClosed?.Invoke();
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[DoorController] アニメーションでドアを閉じます");
        }
    }
    
    /// <summary>
    /// 回転でドアを閉じる（既存の処理）
    /// </summary>
    private void CloseDoorWithRotation()
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
    
    /// <summary>
    /// プレイヤーが接近トリガーに入った時
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[DoorController] OnTriggerEnter: {other.name}, Tag: {other.tag}, openOnPlayerApproach: {openOnPlayerApproach}, isDoorOpen: {isDoorOpen}");
        }
        
        if (!openOnPlayerApproach)
        {
            if (showDebugInfo)
            {
                Debug.Log("[DoorController] 接近検知が無効です");
            }
            return;
        }
        
        if (isDoorOpen)
        {
            if (showDebugInfo)
            {
                Debug.Log("[DoorController] ドアは既に開いています");
            }
            return;
        }
        
        // 鍵が全部集まっているかチェック
        if (collectedKeyCount < requiredKeyCount)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[DoorController] 鍵が不足しています: {collectedKeyCount}/{requiredKeyCount}");
            }
            return;
        }
        
        // プレイヤーかどうかを判定
        bool isPlayer = other.CompareTag(playerTag);
        
        if (showDebugInfo)
        {
            Debug.Log($"[DoorController] プレイヤータグ判定: {isPlayer} (期待タグ: {playerTag}, 実際のタグ: {other.tag})");
        }
        
        if (!isPlayer)
        {
            // プレイヤーの子オブジェクトかチェック
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null && other.transform.IsChildOf(playerObj.transform))
            {
                isPlayer = true;
                if (showDebugInfo)
                {
                    Debug.Log($"[DoorController] プレイヤーの子オブジェクトとして検出: {other.name}");
                }
            }
            
            // CharacterControllerを持つかチェック（VRプレイヤーの場合）
            if (!isPlayer && other.GetComponent<CharacterController>() != null)
            {
                isPlayer = true;
                if (showDebugInfo)
                {
                    Debug.Log($"[DoorController] CharacterControllerとして検出: {other.name}");
                }
            }
        }
        
        if (isPlayer)
        {
            if (showDebugInfo)
            {
                Debug.Log("[DoorController] プレイヤーが接近しました。鍵が全部集まっているのでドアを開きます。");
            }
            OpenDoor();
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"[DoorController] プレイヤーではありません: {other.name}");
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // 左ドアの回転軸を可視化（回転方式の場合）
        if (!useAnimation && leftDoor.doorTransform != null)
        {
            Gizmos.color = Color.blue;
            Vector3 pivot = leftDoor.doorTransform.position + leftDoor.pivotOffset;
            Gizmos.DrawLine(pivot, pivot + leftDoor.rotationAxis * 2f);
            Gizmos.DrawWireSphere(pivot, 0.1f);
        }
        
        // 右ドアの回転軸を可視化（回転方式の場合）
        if (!useAnimation && useBothDoors && rightDoor.doorTransform != null)
        {
            Gizmos.color = Color.red;
            Vector3 pivot = rightDoor.doorTransform.position + rightDoor.pivotOffset;
            Gizmos.DrawLine(pivot, pivot + rightDoor.rotationAxis * 2f);
            Gizmos.DrawWireSphere(pivot, 0.1f);
        }
        
        // 接近検知トリガーを可視化
        if (openOnPlayerApproach && approachTrigger != null)
        {
            Gizmos.color = Color.green;
            if (approachTrigger is BoxCollider boxCollider)
            {
                Gizmos.DrawWireCube(approachTrigger.transform.position + boxCollider.center, boxCollider.size);
            }
            else if (approachTrigger is SphereCollider sphereCollider)
            {
                Gizmos.DrawWireSphere(approachTrigger.transform.position + sphereCollider.center, sphereCollider.radius);
            }
        }
    }
}
