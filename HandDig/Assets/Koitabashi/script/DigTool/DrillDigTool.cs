using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DrillDigTool : MonoBehaviour, IDigTool
{
    public VoxelDigManager digManager;
    private DrillDigStats stats;
    private int upgradeLevel;
    private int speedUpgradeLevel = 0;

    [SerializeField] private Collider currentCollider; // 地形のコライダー（自動設定、デバッグ用）
    
    [Header("ドリルのコライダー設定")]
    [Tooltip("ドリルのコライダー（OnTriggerEnterを呼ぶコライダー）。未設定の場合は自動検索")]
    public Collider drillCollider;
    
    private Collider toolCollider; // 実際に使用するドリルのコライダー
    private float digTimer = 0f;

    [Tooltip("複数の判定エリア（Transform）を追加（最大3個）")]
    public List<Transform> hitZones = new List<Transform>();
    private int activeHitZones = 1;

    private DigSoundManager soundManager;

    [Header("レイヤー/タグ制御")]
    public LayerMask drillDiggableLayers;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = false;

    [Header("掘削位置オフセット設定")]
    [Tooltip("強化段階に応じた掘削位置の前方向オフセット倍率")]
    public float forwardOffsetMultiplier = 0.2f;
    
    [Tooltip("強化段階に応じた掘削位置の上方向オフセット倍率")]
    public float upwardOffsetMultiplier = 0.15f;

    [Header("ドリル射出設定")]
    public GameObject drillTipPrefab;
    public Transform drillTipOrigin;
    public float shootDuration = 3f;
    private bool isShootMode = false;

    [SerializeField] private float shootCooldown = 2.0f;
    private float cooldownTimer = 0f;

    private bool projectileActive = false;

    [Header("表示用ドリル先端")]
    [SerializeField] private GameObject visibleDrillTip;

    private bool canSwitchMode = false;

    /// <summary>
    /// 射出モードかどうかを取得
    /// </summary>
    public bool IsShootMode()
    {
        return isShootMode;
    }

    /// <summary>
    /// 射出モード切替機能を開放（お宝で呼び出される）
    /// </summary>
    public void UnlockShootMode()
    {
        canSwitchMode = true;
        Debug.Log("[DrillDigTool] 射出モード切替機能が開放されました！");
    }

    private void Start()
    {
        UpdateHitZoneVisibility();
        soundManager = FindObjectOfType<DigSoundManager>();
        if (visibleDrillTip != null)
            visibleDrillTip.SetActive(true);
        
        // ドリルのコライダーを取得
        if (drillCollider != null)
        {
            toolCollider = drillCollider;
        }
        else
        {
            // 自動検索：自分または子オブジェクトから
            toolCollider = GetComponent<Collider>();
            if (toolCollider == null)
            {
                toolCollider = GetComponentInChildren<Collider>();
            }
        }
        
        if (toolCollider == null)
        {
            Debug.LogWarning("[DrillDigTool] ドリルのコライダーが見つかりません。インスペクターでdrillColliderを設定してください。");
        }
        else if (enableDebugLog)
        {
            Debug.Log($"[DrillDigTool] toolCollider設定: {toolCollider.name}, IsTrigger: {toolCollider.isTrigger}");
        }
        
        // OnTriggerEnterが呼ばれるコライダーを確認
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            triggerCollider = GetComponentInChildren<Collider>();
        }
        if (triggerCollider != null && enableDebugLog)
        {
            Debug.Log($"[DrillDigTool] OnTriggerEnterを呼ぶコライダー: {triggerCollider.name}, IsTrigger: {triggerCollider.isTrigger}");
        }
        else if (enableDebugLog)
        {
            Debug.LogWarning("[DrillDigTool] OnTriggerEnterを呼ぶコライダーが見つかりません。このスクリプトがアタッチされているオブジェクト（またはその子）にコライダーが必要です。");
        }
    }

    // ステータス設定関連
    public void SetStats(DigToolStats newStats, int level) { }
    public void SetDrillStats(DrillDigStats newStats, int level)
    {
        stats = newStats;
        upgradeLevel = level;
    }
    public void SetHandStats(HandDigStats newStats, int level) { }
    public void SetPickaxeStats(PickaxeDigStats newStats, int level) { }

    public void IncreaseSpeed()
    {
        if (stats != null && speedUpgradeLevel < stats.GetMaxSpeedUpgradeLevel() - 1)
        {
            speedUpgradeLevel++;
        }
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        // モード切替（Xボタン）
        if (canSwitchMode && (OVRInput.GetDown(OVRInput.RawButton.X) || Input.GetKeyDown(KeyCode.X)))
        {
            isShootMode = !isShootMode;
            Debug.Log($"[DrillDigTool] モード切替: {(isShootMode ? "射出モード" : "通常モード")}");
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        // デバッグログ追加
        if (enableDebugLog)
        {
            Debug.Log($"[DrillDigTool] OnTriggerEnter: {other.name}, Tag: {other.tag}, Layer: {other.gameObject.layer}");
        }
        
        if (drillDiggableLayers.value == 0
            || other.CompareTag("Terrain")
            || ((drillDiggableLayers.value & (1 << other.gameObject.layer)) != 0))
        {
            currentCollider = other;
            if (enableDebugLog)
            {
                Debug.Log($"[DrillDigTool] currentCollider設定: {other.name}");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Terrain"))
        {
            currentCollider = other;
        }
        else if (currentCollider == other)
        {
            currentCollider = null;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (currentCollider == other)
        {
            currentCollider = null;
        }
    }

    public void IncreaseHitZone()
    {
        activeHitZones = Mathf.Min(activeHitZones + 1, hitZones.Count);
        UpdateHitZoneVisibility();
    }

    private void UpdateHitZoneVisibility()
    {
        for (int i = 0; i < hitZones.Count; i++)
        {
            if (hitZones[i] != null)
                hitZones[i].gameObject.SetActive(i < activeHitZones);
        }
    }

    // === 掘削・射出動作 ===
    public void UpdateDig(Vector3 toolPosition)
    {
        bool triggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space);

        if (enableDebugLog && Time.frameCount % 60 == 0) // 60フレームごとにログ出力（負荷軽減）
        {
            Debug.Log($"[DrillDigTool] UpdateDig呼び出し: currentCollider={currentCollider?.name ?? "null"}, triggerHeld={triggerHeld}, stats={stats != null}, isShootMode={isShootMode}, projectileActive={projectileActive}");
        }

        // --- 射出モード ---
        if (isShootMode)
        {
            if (!projectileActive && triggerHeld && cooldownTimer >= shootCooldown)
            {
                ShootDrillTip();
            }
            return;
        }

        // --- 通常掘削モード ---
        if (projectileActive)
        {
            if (enableDebugLog && Time.frameCount % 60 == 0)
            {
                Debug.Log("[DrillDigTool] projectileActiveのため掘削をスキップ");
            }
            return;
        }

        if (currentCollider != null && triggerHeld && stats != null)
        {
            float currentDigInterval = stats.GetDigInterval(speedUpgradeLevel);
            digTimer += Time.deltaTime;

            if (enableDebugLog && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[DrillDigTool] 掘削条件満たす: digTimer={digTimer:F3}, currentDigInterval={currentDigInterval:F3}, toolCollider={toolCollider?.name ?? "null"}");
            }

            if (digTimer >= currentDigInterval)
            {
                digTimer = 0f;
                float radius = stats.GetRadius(upgradeLevel);

                for (int i = 0; i < activeHitZones; i++)
                {
                    if (hitZones[i] == null) continue;
                    
                    // 実際のドリルのコライダー位置を取得（衝突点）
                    Vector3 originalPosition;
                    if (toolCollider != null && currentCollider != null)
                    {
                        // 地形コライダーからドリルコライダーの中心への最も近い点（実際の衝突点）
                        originalPosition = currentCollider.ClosestPoint(toolCollider.bounds.center);
                    }
                    else if (toolCollider != null)
                    {
                        // コライダーの中心位置を使用
                        originalPosition = toolCollider.bounds.center;
                    }
                    else if (toolPosition != Vector3.zero)
                    {
                        // toolPositionパラメータを使用
                        originalPosition = toolPosition;
                    }
                    else
                    {
                        // フォールバック：hitZoneの位置
                        originalPosition = hitZones[i].position;
                    }
                    
                    // 強化段階に応じて掘削位置を前・上にオフセット
                    float forwardOffset = upgradeLevel * forwardOffsetMultiplier;
                    float upwardOffset = upgradeLevel * upwardOffsetMultiplier;
                    
                    // 前方向はhitZoneのforward、上方向はhitZoneのupを使用
                    Vector3 forwardDir = hitZones[i].forward.normalized;
                    Vector3 upDir = hitZones[i].up.normalized;
                    Vector3 digPosition = originalPosition + forwardDir * forwardOffset + upDir * upwardOffset;

                    // 当たり判定は実際のコライダー位置で行う（オフセットなし）
                    // コライダーが地形と実際に接触しているかチェック
                    // ピッケルと同様に、currentColliderが設定されていれば掘削可能とする
                    bool canDrillHere = true;
                    if (drillDiggableLayers.value != 0 && toolCollider != null)
                    {
                        // 実際のコライダー位置で判定
                        canDrillHere = Physics.CheckSphere(
                            toolCollider.bounds.center,
                            radius * 0.35f,
                            drillDiggableLayers,
                            QueryTriggerInteraction.Ignore
                        );
                        
                        if (enableDebugLog && !canDrillHere)
                        {
                            Debug.Log($"[DrillDigTool] Physics.CheckSphere失敗: center={toolCollider.bounds.center}, radius={radius * 0.35f}");
                        }
                    }
                    else if (enableDebugLog)
                    {
                        if (drillDiggableLayers.value == 0)
                        {
                            Debug.Log("[DrillDigTool] drillDiggableLayersが0のため、チェックをスキップ");
                        }
                        if (toolCollider == null)
                        {
                            Debug.LogWarning("[DrillDigTool] toolColliderがnullのため、チェックをスキップ");
                        }
                    }

                    if (!canDrillHere) continue;

                    // 掘削位置はオフセット後の位置を使用
                    if (enableDebugLog)
                    {
                        Debug.Log($"[DrillDigTool] 掘削試行: position={digPosition}, radius={radius}, originalPosition={originalPosition}");
                    }
                    
                    bool digOccurred = digManager.TryDigAt(digPosition, radius);
                    if (digOccurred)
                    {
                        if (enableDebugLog)
                        {
                            Debug.Log($"[DrillDigTool] 掘削成功！");
                        }

                        // チュートリアル：最初の掘削でお宝出現
                        TutorialManager.Instance?.OnFirstDig(digPosition, radius);
                        TutorialManager.Instance?.OnAnyDigSuccess(digPosition, radius);

                        soundManager?.PlayDrillDigSound(digPosition);
                        DigEffectManager.Instance?.CreateDigEffect(digPosition, radius);
                    }
                    else if (enableDebugLog)
                    {
                        Debug.Log($"[DrillDigTool] 掘削失敗: ボクセルが見つからないか、範囲外");
                    }
                }
            }
        }
        else
        {
            if (enableDebugLog && Time.frameCount % 60 == 0)
            {
                string reason = "";
                if (currentCollider == null) reason += "currentCollider=null ";
                if (!triggerHeld) reason += "triggerHeld=false ";
                if (stats == null) reason += "stats=null ";
                Debug.Log($"[DrillDigTool] 掘削条件未満足: {reason}");
            }
            digTimer = 0f;
        }
    }

    // === ドリル射出処理 ===
    private void ShootDrillTip()
    {
        projectileActive = true;
        cooldownTimer = 0f;

        if (visibleDrillTip != null)
            visibleDrillTip.SetActive(false);

        GameObject tip = Instantiate(drillTipPrefab, drillTipOrigin.position, drillTipOrigin.rotation);

        DrillTipProjectile projectile = tip.AddComponent<DrillTipProjectile>();
        projectile.Initialize(
            digManager,
            stats,
            upgradeLevel,
            drillDiggableLayers,
            shootDuration,
            soundManager,
            speedUpgradeLevel
        );

        projectile.onDestroyed = OnProjectileDestroyed;
    }

    private void OnProjectileDestroyed()
    {
        projectileActive = false;

        if (visibleDrillTip != null)
            visibleDrillTip.SetActive(true);
    }
}
