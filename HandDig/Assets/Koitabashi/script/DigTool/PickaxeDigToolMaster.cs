using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// つるはし（Master版）- シンプル設計
/// ヒエラルキー前提:
/// - 頭に SwingZone コライダー
/// - つるはし本体 (本スクリプト)
///   - 地形感知用コライダー (PickaxeMainHit を付与)
///   - レベル1/2/3... (各子に掘りポイント)
/// 仕様:
/// - トリガー入力しながら SwingZone に入ると SwingReady
/// - SwingReady 中にトリガーを維持して地形に当てると掘削
/// - IncreaseHitZone() でレベルを1段階アップし、そのレベルの掘りポイントで掘削
/// - 爆発モード時は掘れた全ポイントに爆発マーカーを設置
/// </summary>
public class PickaxeDigToolMaster : MonoBehaviour, IDigTool
{
    [Header("Core")]
    public VoxelDigManager digManager;

    [Header("レベルごとの掘りポイントグループ")]
    [Tooltip("レベル1,2,3... の順に配置された子オブジェクト")]
    public List<GameObject> digPointGroups = new List<GameObject>();

    [Header("タグ設定")]
    public string swingZoneTag = "SwingZone";
    public string terrainTag = "Terrain";
    public string pickaxeOnlyTag = "PickaxeOnly";

    [Header("コンボ & 振りかぶり")]
    public float minComboTime = 0.5f;
    public float maxComboTime = 1.5f;

    [Header("爆発設定")]
    [Tooltip("爆発位置計算で使用する地形レイヤー")]
    public LayerMask terrainLayer;

    [Header("掘削形状設定")]
    [Tooltip("段階に応じた中心位置の奥方向オフセット倍率")]
    public float depthOffsetMultiplier = 0.3f;
    
    [Tooltip("段階に応じた中心位置の上方向オフセット倍率")]
    public float upwardOffsetMultiplier = 0.2f;

    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = false;

    [Header("マテリアル設定")]
    [Tooltip("マテリアルを変更するMeshRenderer（未設定の場合は自動検索）")]
    public MeshRenderer[] targetRenderers;
    
    [Tooltip("通常モード用のマテリアル（1つ目）")]
    public Material normalModeMaterial1;
    
    [Tooltip("通常モード用のマテリアル（2つ目）")]
    public Material normalModeMaterial2;
    
    [Tooltip("爆発モード用のマテリアル（1つ目）")]
    public Material explosionModeMaterial1;
    
    [Tooltip("爆発モード用のマテリアル（2つ目）")]
    public Material explosionModeMaterial2;
    
    // 後方互換性のための古いフィールド（非推奨）
    [System.Obsolete("normalModeMaterialは非推奨です。normalModeMaterial1とnormalModeMaterial2を使用してください。")]
    [Tooltip("通常モード用のマテリアル（非推奨：normalModeMaterial1とnormalModeMaterial2を使用）")]
    public Material normalModeMaterial;
    
    [System.Obsolete("explosionModeMaterialは非推奨です。explosionModeMaterial1とexplosionModeMaterial2を使用してください。")]
    [Tooltip("爆発モード用のマテリアル（非推奨：explosionModeMaterial1とexplosionModeMaterial2を使用）")]
    public Material explosionModeMaterial;

    // 内部状態
    private VRDigToolManager toolManager;
    private PickaxeDigStats stats;
    private int upgradeLevel;

    private int currentLevel = 0;
    private readonly List<Transform> currentDigPoints = new List<Transform>();

    private bool isSwingReady = false;
    private bool isExplosionMode = false;
    private float lastDigTime = -10f;
    private int comboStage = 0;

    private void Awake()
    {
        toolManager = FindObjectOfType<VRDigToolManager>();
        SetLevel(0);
    }

    private void Start()
    {
        SetLevel(currentLevel);
        
        // マテリアル変更対象のRendererを自動検索（未設定の場合）
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<MeshRenderer>();
        }
        
        // 初期マテリアルを設定
        UpdateMaterial();
    }

    private void Update()
    {
        // 爆発モードトグル（X / Button3）
        if (OVRInput.GetDown(OVRInput.Button.Three) || Input.GetKeyDown(KeyCode.X))
        {
            if (stats != null &&
                stats.enableExplosionMode &&
                toolManager != null &&
                toolManager.IsPickaxeExplosionUnlocked())
            {
                // 無限モードでない場合のみチャージをチェック
                bool canToggle = toolManager.infiniteExplosionMode || toolManager.GetPickaxeExplosionCharges() > 0;
                if (canToggle)
                {
                    isExplosionMode = !isExplosionMode;
                    string chargeInfo = toolManager.infiniteExplosionMode ? "無限" : $"残り {toolManager.GetPickaxeExplosionCharges()}";
                    if (enableDebugLog) Debug.Log($"[PickaxeMaster] 爆発モード: {isExplosionMode} ({chargeInfo})");
                    
                    // マテリアルを更新
                    UpdateMaterial();
                    
                    // モード切り替え音を再生
                    if (DigSoundManager.Instance != null)
                    {
                        DigSoundManager.Instance.PlayPickaxeModeSwitchSound(transform.position);
                    }
                    
                    // モード切り替えテキストを消す
                    if (TutorialManager.Instance != null)
                    {
                        TutorialManager.Instance.HideModeSwitchText();
                    }
                }
            }
        }

        // トリガーが離れた時に SwingReady をリセット
        // （ゾーンから出た後、トリガーを離した時にも反応するため）
        if (isSwingReady)
        {
            bool triggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space);
            if (!triggerHeld)
            {
                isSwingReady = false;
                if (enableDebugLog) Debug.Log("[PickaxeMaster] トリガーが離れたため SwingReady をリセット");
            }
        }
    }

    // ===== Stats =====
    public void SetStats(DigToolStats newStats, int level)
    {
        Debug.LogWarning("[PickaxeDigToolMaster] SetStats(DigToolStats) is deprecated. Use SetPickaxeStats instead.");
    }

    public void SetPickaxeStats(PickaxeDigStats newStats, int level)
    {
        stats = newStats;
        upgradeLevel = level;
        if (enableDebugLog) Debug.Log($"[PickaxeMaster] Stats set. level={upgradeLevel}");
    }

    public void SetHandStats(HandDigStats newStats, int level) { }
    public void SetDrillStats(DrillDigStats newStats, int level) { }

    // ===== Swing Ready =====
    public void OnSwingZoneEntered()
    {
        bool triggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space);
        if (triggerHeld)
        {
            isSwingReady = true;
            if (enableDebugLog) Debug.Log("[PickaxeMaster] SwingReady = true");
        }
    }

    public void OnSwingZoneExit()
    {
        isSwingReady = false;
        if (enableDebugLog) Debug.Log("[PickaxeMaster] SwingReady reset");
    }

    public bool IsSwingReady() => isSwingReady;

    // ===== レベル切替 =====
    public void IncreaseHitZone()
    {
        SetLevel(currentLevel + 1);
    }

    private void SetLevel(int level)
    {
        if (digPointGroups == null || digPointGroups.Count == 0)
        {
            Debug.LogWarning("[PickaxeMaster] digPointGroups が未設定です");
            return;
        }

        level = Mathf.Clamp(level, 0, digPointGroups.Count - 1);
        currentLevel = level;

        for (int i = 0; i < digPointGroups.Count; i++)
        {
            if (digPointGroups[i] != null)
                digPointGroups[i].SetActive(i == currentLevel);
        }

        CollectCurrentDigPoints();
        if (enableDebugLog) Debug.Log($"[PickaxeMaster] Level {currentLevel} active (points: {currentDigPoints.Count})");
    }

    private void CollectCurrentDigPoints()
    {
        currentDigPoints.Clear();
        if (currentLevel < 0 || currentLevel >= digPointGroups.Count) return;
        var group = digPointGroups[currentLevel];
        if (group == null) return;

        foreach (Transform child in group.transform)
        {
            if (child != null) currentDigPoints.Add(child);
        }
    }

    // ===== メインヒット（PickaxeMainHit から呼ばれる） =====
    public void OnMainHit(Collider other)
    {
        if (!other.CompareTag(terrainTag) && !other.CompareTag(pickaxeOnlyTag))
            return;

        bool triggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space);
        if (!(isSwingReady && triggerHeld && stats != null))
        {
            if (enableDebugLog) Debug.Log("[PickaxeMaster] Hit無効 (swing/input/stats 不足)");
            return;
        }

        // コンボ計算
        float now = Time.time;
        float dt = now - lastDigTime;
        comboStage = (dt >= minComboTime && dt <= maxComboTime) ? Mathf.Min(comboStage + 1, 2) : 0;
        lastDigTime = now;
        isSwingReady = false;

        float radius = stats.GetRadius(comboStage, upgradeLevel);
        List<Vector3> dug = new List<Vector3>();

        for (int i = 0; i < currentDigPoints.Count; i++)
        {
            Transform p = currentDigPoints[i];
            if (p == null) continue;

            // 段階に応じて中心位置を奥の方に斜め上にオフセット
            float depthOffset = upgradeLevel * depthOffsetMultiplier;
            float upwardOffset = upgradeLevel * upwardOffsetMultiplier;

            // 掘削方向はdigPointのforward方向を使用
            Vector3 digDirection = p.forward.normalized;
            Vector3 digPos = p.position + p.up * (radius * 0.3f);
            
            // オフセットを適用（前方向と上方向）
            digPos += digDirection * depthOffset;
            digPos += p.up * upwardOffset;

            // 掘削実行
            bool ok = digManager != null && digManager.TryDigAt(digPos, radius);
            if (!ok)
            {
                if (enableDebugLog) Debug.Log($"[PickaxeMaster] Point {i + 1} にボクセルなし");
                continue;
            }

            dug.Add(digPos);

            TutorialManager.Instance?.OnFirstDig(digPos, radius);
            TutorialManager.Instance?.OnAnyDigSuccess(digPos, radius);
            DigEffectManager.Instance?.CreateDigEffect(digPos, radius);
            DigSoundManager.Instance?.PlayPickaxeDigSound(comboStage, digPos);
        }

        // 爆発: 掘れた全ポイントに設置
        if (isExplosionMode &&
            dug.Count > 0 &&
            toolManager != null &&
            stats != null &&
            stats.enableExplosionMode)
        {
            // 無限モードでない場合のみチャージをチェック
            bool canUseExplosion = toolManager.infiniteExplosionMode || toolManager.GetPickaxeExplosionCharges() > 0;
            if (canUseExplosion && toolManager.TryConsumePickaxeExplosionCharge())
            {
                float expRadius = stats.GetExplosionRadius(upgradeLevel);
                foreach (var pos in dug)
                {
                    Vector3 expPos = GetExplosionPosition(pos, radius);
                    SpawnExplosionMarker(expPos, expRadius, stats.explosionDelaySeconds);
                }

                // 無限モードでない場合のみ、チャージがなくなったらモードをオフ
                if (!toolManager.infiniteExplosionMode && toolManager.GetPickaxeExplosionCharges() <= 0)
                {
                    isExplosionMode = false;
                    UpdateMaterial();
                }
            }
        }
    }

    public void UpdateDig(Vector3 toolPosition) { }

    // ===== IDigTool インターフェース用トリガー実装 =====
    // 現状の設計では SwingReadyZone / PickaxeMainHit 側から直接呼んでいるが、
    // IDigTool を満たすために最低限の実装を入れておく。
    public void OnTriggerEnter(Collider other)
    {
        // 頭のSwingZoneコライダーをこのオブジェクトに付けた場合のフォールバック
        if (other.CompareTag(swingZoneTag))
        {
            OnSwingZoneEntered();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(swingZoneTag))
        {
            OnSwingZoneExit();
        }
    }

    // ===== 爆発位置 =====
    private Vector3 GetExplosionPosition(Vector3 digPos, float digRadius)
        {
        Vector3 dir = transform.forward.normalized;
        float epsilon = 0.15f;
        float maxDist = digRadius * 1.5f;

        Ray ray = new Ray(digPos, dir);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDist, terrainLayer))
            {
            return hit.point - dir * epsilon;
            }

        return digPos + dir * (digRadius * 0.8f);
    }

    private void SpawnExplosionMarker(Vector3 position, float radius, float delaySeconds)
    {
        if (stats == null || stats.explosionMarkerPrefab == null)
        {
            Debug.LogWarning("[PickaxeMaster] explosionMarkerPrefab 未設定");
            return;
        }

        GameObject go = Instantiate(stats.explosionMarkerPrefab, position, Quaternion.identity);
        var marker = go.GetComponent<ExplosiveMarker>();
        if (marker != null)
        {
            marker.Initialize(digManager, radius, delaySeconds);
        }
    }

    // ===== 状態参照 =====
    public bool IsExplosionMode() => isExplosionMode;

    /// <summary>
    /// マテリアルを更新（モードに応じて）
    /// </summary>
    private void UpdateMaterial()
    {
        if (targetRenderers == null || targetRenderers.Length == 0) return;

        Material mat1, mat2;
        
        if (isExplosionMode)
        {
            // 爆発モード用のマテリアルを取得（後方互換性対応）
            mat1 = explosionModeMaterial1 != null ? explosionModeMaterial1 : explosionModeMaterial;
            mat2 = explosionModeMaterial2;
        }
        else
        {
            // 通常モード用のマテリアルを取得（後方互換性対応）
            mat1 = normalModeMaterial1 != null ? normalModeMaterial1 : normalModeMaterial;
            mat2 = normalModeMaterial2;
        }

        // nullチェック
        if (mat1 == null && mat2 == null)
        {
            if (enableDebugLog) Debug.LogWarning("[PickaxeMaster] マテリアルが設定されていません");
            return;
        }

        // マテリアル配列を作成（nullでないもののみ）
        List<Material> materialsList = new List<Material>();
        if (mat1 != null) materialsList.Add(mat1);
        if (mat2 != null) materialsList.Add(mat2);
        
        Material[] targetMaterials = materialsList.ToArray();

        foreach (var renderer in targetRenderers)
        {
            if (renderer != null)
            {
                // マテリアル配列を設定（2つのスロットに対応）
                renderer.materials = targetMaterials;
            }
        }

        if (enableDebugLog) Debug.Log($"[PickaxeMaster] マテリアルを更新: {(isExplosionMode ? "爆発モード" : "通常モード")}");
    }
}
