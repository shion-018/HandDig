using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// ボタンを押した後にそのまま固定する（戻らないようにする）スクリプト
/// Poke Interactible Visualと連携して動作
/// </summary>
public class LatchButtonController : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("押された後に固定するかどうか")]
    public bool latchOnPress = true;
    
    [Tooltip("固定後に再度押せるようにするか（falseの場合は一度押したら永久に固定）")]
    public bool allowReset = false;
    
    [Tooltip("リセット用のキー（デバッグ用）")]
    public KeyCode resetKey = KeyCode.R;
    
    [Header("カギ処理設定")]
    [Tooltip("ボタン押下時にカギを収集するか")]
    public bool collectKeyOnPress = true;
    
    [Tooltip("収集するカギオブジェクト（指定しない場合は自動検索）")]
    public KeyItem targetKeyItem;
    
    [Tooltip("DoorControllerへの参照（カギ収集を通知するため）")]
    public DoorController doorController;
    
    [System.Serializable]
    public class AnimationTarget
    {
        [Tooltip("アニメーションを再生するオブジェクト（Animatorコンポーネントが必要）")]
        public GameObject targetObject;
        
        [Tooltip("アニメーションのトリガー名（空の場合はブールパラメータを使用）")]
        public string triggerName = "";
        
        [Tooltip("ブールパラメータ名（トリガーが空の場合に使用）")]
        public string boolParameterName = "";
        
        [Tooltip("ブールパラメータの値（true/false）")]
        public bool boolValue = true;
    }
    
    [Header("アニメーション設定")]
    [Tooltip("ボタン押下時にアニメーションを再生するか")]
    public bool playAnimationOnPress = false;
    
    [Tooltip("アニメーションを再生するオブジェクトのリスト")]
    public List<AnimationTarget> animationTargets = new List<AnimationTarget>();
    
    [Header("デバッグ")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = true;
    
    private bool isLatched = false;
    private Vector3 initialPosition;
    private Vector3 latchedPosition;
    private Transform buttonTransform;
    
    // Poke Interactible Visualコンポーネントへの参照
    private MonoBehaviour pokeInteractibleVisual;
    
    private void Start()
    {
        // ボタンのTransformを取得（通常はこのGameObject）
        buttonTransform = transform;
        initialPosition = buttonTransform.localPosition;
        
        // Poke Interactible Visualコンポーネントを探す（このオブジェクトまたは親から）
        FindPokeInteractibleVisual();
        
        // カギオブジェクトを自動検索（指定されていない場合）
        if (collectKeyOnPress && targetKeyItem == null)
        {
            FindKeyItem();
        }
        
        // DoorControllerを自動検索（指定されていない場合）
        if (doorController == null)
        {
            FindDoorController();
        }
        
        // 初期状態では固定されていない
        isLatched = false;
    }
    
    private void Update()
    {
        // デバッグ用：リセットキーで固定を解除
        if (allowReset && Input.GetKeyDown(resetKey))
        {
            ResetLatch();
        }
    }
    
    /// <summary>
    /// Poke Interactible Visualコンポーネントを探す
    /// </summary>
    private void FindPokeInteractibleVisual()
    {
        // まずこのオブジェクトから探す
        Component[] components = GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp != null && comp.GetType().Name.Contains("Poke") && comp.GetType().Name.Contains("Visual"))
            {
                pokeInteractibleVisual = comp as MonoBehaviour;
                Debug.Log($"[LatchButtonController] Poke Interactible Visualを見つけました（このオブジェクト）: {comp.GetType().Name}");
                return;
            }
        }
        
        // 見つからない場合は親オブジェクトから探す
        if (transform.parent != null)
        {
            components = transform.parent.GetComponentsInChildren<Component>();
            foreach (var comp in components)
            {
                if (comp != null && comp.GetType().Name.Contains("Poke") && comp.GetType().Name.Contains("Visual"))
                {
                    pokeInteractibleVisual = comp as MonoBehaviour;
                    Debug.Log($"[LatchButtonController] Poke Interactible Visualを見つけました（親オブジェクト）: {comp.GetType().Name}");
                    return;
                }
            }
        }
        
        Debug.LogWarning("[LatchButtonController] Poke Interactible Visualが見つかりませんでした");
    }
    
    /// <summary>
    /// ボタンが押された時に呼ばれる（内部処理）
    /// </summary>
    private void OnButtonPressed()
    {
        if (enableDebugLog) Debug.Log($"[LatchButtonController] OnButtonPressed()が呼ばれました。latchOnPress={latchOnPress}, isLatched={isLatched}");
        
        if (!latchOnPress)
        {
            if (enableDebugLog) Debug.LogWarning("[LatchButtonController] latchOnPressがfalseのため、固定処理をスキップします");
            return;
        }
        
        if (isLatched)
        {
            if (enableDebugLog) Debug.LogWarning("[LatchButtonController] 既に固定されているため、処理をスキップします");
            return;
        }
        
        // 現在の位置を記録
        latchedPosition = buttonTransform.localPosition;
        isLatched = true;
        
        if (enableDebugLog) Debug.Log($"[LatchButtonController] 位置を記録しました: {latchedPosition}");
        
        // Poke Interactible VisualのReturn to Initial Positionを無効化
        DisableReturnToInitialPosition();
        
        // 位置を固定し続けるコルーチンを開始（確実に位置を固定するため）
        StartCoroutine(KeepPositionFixed());
        
        // カギを収集する処理を実行
        if (collectKeyOnPress)
        {
            CollectKey();
        }
        
        // アニメーションを再生する処理を実行
        if (playAnimationOnPress)
        {
            PlayAnimations();
        }
        
        if (enableDebugLog) Debug.Log("[LatchButtonController] ボタンが固定されました");
    }
    
    /// <summary>
    /// カギオブジェクトを自動検索
    /// </summary>
    private void FindKeyItem()
    {
        targetKeyItem = FindObjectOfType<KeyItem>();
        if (targetKeyItem != null && enableDebugLog)
        {
            Debug.Log($"[LatchButtonController] カギオブジェクトを自動検索しました: {targetKeyItem.name}");
        }
        else if (enableDebugLog)
        {
            Debug.LogWarning("[LatchButtonController] カギオブジェクトが見つかりませんでした");
        }
    }
    
    /// <summary>
    /// DoorControllerを自動検索
    /// </summary>
    private void FindDoorController()
    {
        doorController = FindObjectOfType<DoorController>();
        if (doorController != null && enableDebugLog)
        {
            Debug.Log($"[LatchButtonController] DoorControllerを自動検索しました: {doorController.name}");
        }
        else if (enableDebugLog)
        {
            Debug.LogWarning("[LatchButtonController] DoorControllerが見つかりませんでした");
        }
    }
    
    /// <summary>
    /// カギを収集する（KeyItemのCollectKey()と同じ処理を実行）
    /// </summary>
    private void CollectKey()
    {
        if (targetKeyItem == null)
        {
            if (enableDebugLog) Debug.LogWarning("[LatchButtonController] カギオブジェクトが設定されていません");
            return;
        }
        
        // 既に収集済みの場合はスキップ
        if (targetKeyItem.IsCollected())
        {
            if (enableDebugLog) Debug.LogWarning("[LatchButtonController] カギは既に収集済みです");
            return;
        }
        
        if (enableDebugLog) Debug.Log($"[LatchButtonController] カギを収集します: {targetKeyItem.name}");
        
        // KeyItemのCollectKey()を呼び出す（これでエフェクト、音、ゴールマーク生成などが実行される）
        targetKeyItem.CollectKey();
        
        // DoorControllerに通知（カギ収集を記録するため）
        if (doorController != null)
        {
            doorController.CollectKey(targetKeyItem.gameObject);
            if (enableDebugLog) Debug.Log("[LatchButtonController] DoorControllerにカギ収集を通知しました");
        }
    }
    
    /// <summary>
    /// WhenSelectイベント用（InteractableUnityEventWrapperのWhenSelectに接続）
    /// これがボタンが押された時に呼ばれるメインのメソッドです
    /// </summary>
    public void WhenSelect()
    {
        if (enableDebugLog) Debug.Log("[LatchButtonController] WhenSelect()が呼ばれました");
        OnButtonPressed();
    }
    
    /// <summary>
    /// WhenSelectingInteractorViewAddedイベント用（InteractableUnityEventWrapperのWhenSelectingInteractorViewAddedに接続）
    /// 選択しているインタラクターが追加された時にも呼ばれる
    /// </summary>
    public void WhenSelectingInteractorViewAdded()
    {
        OnButtonPressed();
    }
    
    /// <summary>
    /// Poke Interactible VisualのReturn to Initial Positionを無効化
    /// </summary>
    private void DisableReturnToInitialPosition()
    {
        if (pokeInteractibleVisual == null)
        {
            if (enableDebugLog) Debug.LogWarning("[LatchButtonController] Poke Interactible Visualが見つかりません。位置固定コルーチンで対応します");
            return;
        }
        
        var type = pokeInteractibleVisual.GetType();
        if (enableDebugLog) Debug.Log($"[LatchButtonController] Poke Interactible Visualの型: {type.Name}");
        
        // Return to Initial Positionプロパティを探す
        var returnToInitialProperty = type.GetProperty("ReturnToInitialPosition");
        if (returnToInitialProperty != null)
        {
            returnToInitialProperty.SetValue(pokeInteractibleVisual, false, null);
            if (enableDebugLog) Debug.Log("[LatchButtonController] ReturnToInitialPositionをfalseに設定しました");
            return;
        }
        
        // フィールドを探す
        var returnToInitialField = type.GetField("returnToInitialPosition", 
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (returnToInitialField != null)
        {
            returnToInitialField.SetValue(pokeInteractibleVisual, false);
            if (enableDebugLog) Debug.Log("[LatchButtonController] returnToInitialPositionをfalseに設定しました");
            return;
        }
        
        // Follow Affordanceを無効化する方法を試す
        var followAffordanceProperty = type.GetProperty("FollowAffordance");
        if (followAffordanceProperty != null)
        {
            var followAffordance = followAffordanceProperty.GetValue(pokeInteractibleVisual);
            if (followAffordance != null)
            {
                var followType = followAffordance.GetType();
                var returnToInitialInFollow = followType.GetProperty("ReturnToInitialPosition");
                if (returnToInitialInFollow != null)
                {
                    returnToInitialInFollow.SetValue(followAffordance, false, null);
                    if (enableDebugLog) Debug.Log("[LatchButtonController] FollowAffordanceのReturnToInitialPositionをfalseに設定しました");
                    return;
                }
            }
        }
        
        // コンポーネントを無効化してみる（最後の手段）
        if (pokeInteractibleVisual.enabled)
        {
            if (enableDebugLog) Debug.LogWarning("[LatchButtonController] ReturnToInitialPositionの設定が見つかりませんでした。コンポーネントを無効化します");
            pokeInteractibleVisual.enabled = false;
        }
    }
    
    /// <summary>
    /// 位置を固定し続けるコルーチン
    /// </summary>
    private System.Collections.IEnumerator KeepPositionFixed()
    {
        if (enableDebugLog) Debug.Log($"[LatchButtonController] 位置固定コルーチンを開始しました。固定位置: {latchedPosition}");
        
        while (isLatched)
        {
            // 固定位置に戻す
            Vector3 currentPos = buttonTransform.localPosition;
            if (Vector3.Distance(currentPos, latchedPosition) > 0.001f)
            {
                buttonTransform.localPosition = latchedPosition;
                if (enableDebugLog && Time.frameCount % 60 == 0) // 1秒ごとにログ
                {
                    Debug.Log($"[LatchButtonController] 位置を修正しました: {currentPos} -> {latchedPosition}");
                }
            }
            yield return null;
        }
        
        if (enableDebugLog) Debug.Log("[LatchButtonController] 位置固定コルーチンを終了しました");
    }
    
    /// <summary>
    /// 固定を解除する（リセット）
    /// </summary>
    public void ResetLatch()
    {
        if (!isLatched) return;
        
        isLatched = false;
        buttonTransform.localPosition = initialPosition;
        
        // Poke Interactible VisualのReturn to Initial Positionを再有効化
        if (pokeInteractibleVisual != null)
        {
            var type = pokeInteractibleVisual.GetType();
            var returnToInitialProperty = type.GetProperty("ReturnToInitialPosition");
            if (returnToInitialProperty != null)
            {
                returnToInitialProperty.SetValue(pokeInteractibleVisual, true, null);
            }
        }
        
        Debug.Log("[LatchButtonController] ボタンの固定を解除しました");
    }
    
    /// <summary>
    /// アニメーションを再生する
    /// </summary>
    private void PlayAnimations()
    {
        if (animationTargets == null || animationTargets.Count == 0)
        {
            if (enableDebugLog) Debug.LogWarning("[LatchButtonController] アニメーション対象が設定されていません");
            return;
        }
        
        foreach (var target in animationTargets)
        {
            if (target.targetObject == null)
            {
                if (enableDebugLog) Debug.LogWarning("[LatchButtonController] アニメーション対象オブジェクトがnullです");
                continue;
            }
            
            Animator animator = target.targetObject.GetComponent<Animator>();
            if (animator == null)
            {
                if (enableDebugLog) Debug.LogWarning($"[LatchButtonController] {target.targetObject.name}にAnimatorコンポーネントが見つかりません");
                continue;
            }
            
            // トリガーが設定されている場合はトリガーを使用
            if (!string.IsNullOrEmpty(target.triggerName))
            {
                animator.SetTrigger(target.triggerName);
                if (enableDebugLog) Debug.Log($"[LatchButtonController] {target.targetObject.name}のアニメーショントリガー '{target.triggerName}' を発火しました");
            }
            // ブールパラメータが設定されている場合はブールパラメータを使用
            else if (!string.IsNullOrEmpty(target.boolParameterName))
            {
                animator.SetBool(target.boolParameterName, target.boolValue);
                if (enableDebugLog) Debug.Log($"[LatchButtonController] {target.targetObject.name}のアニメーションブールパラメータ '{target.boolParameterName}' を {target.boolValue} に設定しました");
            }
            else
            {
                if (enableDebugLog) Debug.LogWarning($"[LatchButtonController] {target.targetObject.name}のアニメーションパラメータが設定されていません（トリガー名またはブールパラメータ名を設定してください）");
            }
        }
    }
    
    /// <summary>
    /// 現在固定されているかどうか
    /// </summary>
    public bool IsLatched()
    {
        return isLatched;
    }
}
