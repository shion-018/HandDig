using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillDigTool : MonoBehaviour, IDigToolWithStats
{
    public VoxelDigManager digManager;
    private DrillDigStats stats;
    private int upgradeLevel;
    private int speedUpgradeLevel = 0;
    [SerializeField]
    private Collider currentCollider;
    private float digTimer = 0f;

    [Tooltip("複数の判定エリア（Transform）を追加（最大3個）")]
    public List<Transform> hitZones = new List<Transform>();
    private int activeHitZones = 1;

    private DigSoundManager soundManager;

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

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
        {
            currentCollider = other;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Terrainタグのオブジェクトに触れている間だけ更新
        if (other.CompareTag("Terrain"))
        {
            currentCollider = other;
        }
        // Terrainでないものに触れていた場合は解除
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
            {
                hitZones[i].gameObject.SetActive(i < activeHitZones);
            }
        }
    }

    void Start()
    {
        UpdateHitZoneVisibility();
        soundManager = FindObjectOfType<DigSoundManager>();
    }

    public void UpdateDig(Vector3 toolPosition)
    {
        bool triggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space);

        // 万が一currentColliderがTerrain以外になっていたらリセット
        if (currentCollider != null && !currentCollider.CompareTag("Terrain"))
        {
            currentCollider = null;
        }

        if (currentCollider != null && triggerHeld && stats != null)
        {
            float currentDigInterval = stats.GetDigInterval(speedUpgradeLevel);
            digTimer += Time.deltaTime;

            if (digTimer >= currentDigInterval)
            {
                digTimer = 0f;
                float radius = stats.GetRadius(upgradeLevel);

                for (int i = 0; i < activeHitZones; i++)
                {
                    if (hitZones[i] != null)
                    {
                        Vector3 digPosition = hitZones[i].position;

                        bool digOccurred = digManager.TryDigAt(digPosition, radius);

                        if (digOccurred)
                        {
                            if (soundManager != null)
                                soundManager.PlayDrillDigSound(digPosition);

                            if (DigEffectManager.Instance != null)
                                DigEffectManager.Instance.CreateDigEffect(digPosition, radius);
                        }
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
