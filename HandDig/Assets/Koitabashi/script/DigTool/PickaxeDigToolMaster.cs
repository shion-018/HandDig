using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeDigToolMaster : MonoBehaviour, IDigToolWithStats
{
    public VoxelDigManager digManager;

    [Tooltip("����I�u�W�F�N�g�����ԂɊi�[�i�ő�3�j")]
    public List<Transform> hitZones = new List<Transform>();

    private DigToolStats stats;
    private int upgradeLevel;

    public float minComboTime = 0.5f;
    public float maxComboTime = 1.5f;

    private float lastDigTime = -10f;
    private int comboStage = 0;

    private bool isSwingReady = false;
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
        Debug.Log($"[PickaxeMaster] SetStats: Level {upgradeLevel}");
    }

    public void SetSwingReady(bool ready)
    {
        isSwingReady = ready;
        Debug.Log($"[PickaxeMaster] SwingReady = {ready}");
    }

    public void IncreaseHitZone()
    {
        activeHitZones = Mathf.Min(activeHitZones + 1, hitZones.Count);
        Debug.Log($"[PickaxeMaster] 判定数が {activeHitZones} になりました");
        
        // 判定エリアの表示/非表示を更新
        UpdateHitZoneVisibility();
    }
    
    private void UpdateHitZoneVisibility()
    {
        for (int i = 0; i < hitZones.Count; i++)
        {
            if (hitZones[i] != null)
            {
                // i < activeHitZones なら表示、そうでなければ非表示
                hitZones[i].gameObject.SetActive(i < activeHitZones);
            }
        }
    }

    public void OnAnyHit()
    {
        bool isTriggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        bool isSpaceHeld = Input.GetKey(KeyCode.Space);

        if (isSwingReady && (isTriggerHeld || isSpaceHeld) && stats != null)
        {
            float currentTime = Time.time;
            float timeSinceLast = currentTime - lastDigTime;

            if (timeSinceLast >= minComboTime && timeSinceLast <= maxComboTime)
                comboStage = Mathf.Min(comboStage + 1, 2);
            else
                comboStage = 0;

            lastDigTime = currentTime;
            isSwingReady = false;

            float radius = stats.GetRadius(comboStage, upgradeLevel);

            for (int i = 0; i < activeHitZones; i++)
            {
                Transform t = hitZones[i];

                Vector3 upwardOffset = t.up * (radius * 0.3f);
                Vector3 digPosition = t.position + upwardOffset;

                digManager.DigAt(digPosition, radius);

                Debug.Log($"[PickaxeMaster] ����{i + 1} Combo {comboStage + 1} / radius: {radius} / Y: {upwardOffset.y:F2}");
            }
        }
    }
    public void OnTriggerEnter(Collider other) { /* 何もせず */ }
    public void OnTriggerExit(Collider other) { /* 何もせず */ }
    public void UpdateDig(Vector3 pos) { /* 何もせず */ }
}
