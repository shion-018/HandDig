using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillDigTool : MonoBehaviour, IDigToolWithStats
{
    public VoxelDigManager digManager;

    [Tooltip("複数の判定エリア（Transform）を追加（最大3個）")]
    public List<Transform> hitZones = new List<Transform>();

    private DigToolStats stats;
    private int upgradeLevel;

    public float digInterval = 0.1f;
    private Collider currentCollider;
    private float digTimer = 0f;

    private int activeHitZones = 1; // 1個からスタート

    void Start()
    {
        // 初期化時に判定エリアの表示/非表示を設定
        UpdateHitZoneVisibility();
    }

    public void SetStats(DigToolStats newStats, int level)
    {
        stats = newStats;
        upgradeLevel = level;
    }

    public void IncreaseHitZone()
    {
        int before = activeHitZones;
        activeHitZones = Mathf.Min(activeHitZones + 1, hitZones.Count);
        Debug.Log($"[Drill] 判定数が {before} → {activeHitZones} になりました (最大: {hitZones.Count})");
        
        // 判定エリアの表示/非表示を更新
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

    public void UpdateDig(Vector3 toolPosition)
    {
        bool triggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space);

        if (currentCollider != null && triggerHeld)
        {
            digTimer += Time.deltaTime;
            if (digTimer >= digInterval && stats != null)
            {
                digTimer = 0f;

                float radius = stats.GetRadius(0, upgradeLevel); // comboStage = 0（ドリルは段階なし）
                
                // 複数の判定エリアで掘削
                for (int i = 0; i < activeHitZones; i++)
                {
                    if (hitZones[i] != null)
                    {
                        Vector3 digPosition = hitZones[i].position;
                        digManager.DigAt(digPosition, radius);
                        Debug.Log($"[Drill] 判定{i + 1} Dig at radius {radius}");
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