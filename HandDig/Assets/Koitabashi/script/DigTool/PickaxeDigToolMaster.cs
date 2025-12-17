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
public class PickaxeDigToolMaster : MonoBehaviour, IDigToolWithStats
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
    }

    private void Update()
    {
        // 爆発モードトグル（X / Button3）
        if (OVRInput.GetDown(OVRInput.Button.Three) || Input.GetKeyDown(KeyCode.X))
        {
            if (stats != null &&
                stats.enableExplosionMode &&
                toolManager != null &&
                toolManager.IsPickaxeExplosionUnlocked() &&
                toolManager.GetPickaxeExplosionCharges() > 0)
            {
                isExplosionMode = !isExplosionMode;
                Debug.Log($"[PickaxeMaster] 爆発モード: {isExplosionMode} (残り {toolManager.GetPickaxeExplosionCharges()})");
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
                Debug.Log("[PickaxeMaster] トリガーが離れたため SwingReady をリセット");
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
        Debug.Log($"[PickaxeMaster] Stats set. level={upgradeLevel}");
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
            Debug.Log("[PickaxeMaster] SwingReady = true");
        }
    }

    public void OnSwingZoneExit()
    {
        isSwingReady = false;
        Debug.Log("[PickaxeMaster] SwingReady reset");
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
        Debug.Log($"[PickaxeMaster] Level {currentLevel} active (points: {currentDigPoints.Count})");
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
            Debug.Log("[PickaxeMaster] Hit無効 (swing/input/stats 不足)");
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

            Vector3 digPos = p.position + p.up * (radius * 0.3f);
            bool ok = digManager != null && digManager.TryDigAt(digPos, radius);
            if (!ok)
            {
                Debug.Log($"[PickaxeMaster] Point {i + 1} にボクセルなし");
                continue;
            }

            dug.Add(digPos);

            TutorialManager.Instance?.OnFirstDig(digPos, radius);
            TutorialManager.Instance?.OnAnyDigSuccess();
            DigEffectManager.Instance?.CreateDigEffect(digPos, radius);
            DigSoundManager.Instance?.PlayPickaxeDigSound(comboStage, digPos);
        }

        // 爆発: 掘れた全ポイントに設置
        if (isExplosionMode &&
            dug.Count > 0 &&
            toolManager != null &&
            toolManager.GetPickaxeExplosionCharges() > 0 &&
            stats != null &&
            stats.enableExplosionMode)
        {
            if (toolManager.TryConsumePickaxeExplosionCharge())
            {
                float expRadius = stats.GetExplosionRadius(upgradeLevel);
                foreach (var pos in dug)
                {
                    Vector3 expPos = GetExplosionPosition(pos, radius);
                    SpawnExplosionMarker(expPos, expRadius, stats.explosionDelaySeconds);
                }

                if (toolManager.GetPickaxeExplosionCharges() <= 0)
                    isExplosionMode = false;
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
}
