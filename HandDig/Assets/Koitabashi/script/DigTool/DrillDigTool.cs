using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillDigTool : MonoBehaviour, IDigToolWithStats
{
    public VoxelDigManager digManager;

    private DrillDigStats stats;
    private int upgradeLevel;
    private int speedUpgradeLevel = 0; // お宝取得で加速するレベル

    private Collider currentCollider;
    private float digTimer = 0f;

    [Tooltip("複数の判定エリア（Transform）を追加（最大3個）")]
    public List<Transform> hitZones = new List<Transform>();
    private int activeHitZones = 1; // 1個からスタート

    public void SetStats(DigToolStats newStats, int level)
    {
        // 後方互換性のため残す
        Debug.LogWarning("[DrillDigTool] SetStats(DigToolStats) is deprecated. Use SetDrillStats instead.");
    }

    public void SetDrillStats(DrillDigStats newStats, int level)
    {
        stats = newStats;
        upgradeLevel = level;
    }

    public void SetHandStats(HandDigStats newStats, int level) { /* Drill用なので未実装 */ }
    public void SetPickaxeStats(PickaxeDigStats newStats, int level) { /* Drill用なので未実装 */ }

    // お宝取得で採掘速度を加速
    public void IncreaseSpeed()
    {
        if (stats != null && speedUpgradeLevel < stats.GetMaxSpeedUpgradeLevel() - 1)
        {
            speedUpgradeLevel++;
            Debug.Log($"[Drill] 採掘速度アップ！ Level {speedUpgradeLevel} / Max {stats.GetMaxSpeedUpgradeLevel()}");
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
            currentCollider = other;
    }

    public void OnTriggerExit(Collider other)
    {
        if (currentCollider == other)
            currentCollider = null;
    }

    public void IncreaseHitZone()
    {
        int before = activeHitZones;
        activeHitZones = Mathf.Min(activeHitZones + 1, hitZones.Count);
        Debug.Log($"[Drill] 判定数が {before} → {activeHitZones} になりました (最大: {hitZones.Count})");
        UpdateHitZoneVisibility();
    }

    private void UpdateHitZoneVisibility()
    {
        Debug.Log($"[Drill] 判定エリアの表示/非表示を更新中... (activeHitZones: {activeHitZones}, hitZones.Count: {hitZones.Count})");
        for (int i = 0; i < hitZones.Count; i++)
        {
            if (hitZones[i] != null)
            {
                bool shouldBeActive = i < activeHitZones;
                hitZones[i].gameObject.SetActive(shouldBeActive);
                Debug.Log($"[Drill] 判定エリア{i + 1}: {(shouldBeActive ? "表示" : "非表示")}");
            }
        }
    }

    void Start()
    {
        // 初期化時に判定エリアの表示/非表示を設定
        UpdateHitZoneVisibility();
    }

    public void UpdateDig(Vector3 toolPosition)
    {
        bool triggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space);

        if (currentCollider != null && triggerHeld && stats != null)
        {
            float currentDigInterval = stats.GetDigInterval(speedUpgradeLevel);
            digTimer += Time.deltaTime;
            
            if (digTimer >= currentDigInterval)
            {
                digTimer = 0f;

                float radius = stats.GetRadius(upgradeLevel);
                // 複数の判定エリアで掘削
                for (int i = 0; i < activeHitZones; i++)
                {
                    if (hitZones[i] != null)
                    {
                        Vector3 digPosition = hitZones[i].position;
                        digManager.DigAt(digPosition, radius);
                        Debug.Log($"[Drill] 判定{i + 1} Dig at radius {radius} / interval {currentDigInterval:F3}s");
                    }
                }
            }
        }
        else
        {
            digTimer = 0f;
        }
    }
}