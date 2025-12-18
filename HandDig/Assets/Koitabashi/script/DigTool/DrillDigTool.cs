using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DrillDigTool : MonoBehaviour, IDigToolWithStats
{
    public VoxelDigManager digManager;
    private DrillDigStats stats;
    private int upgradeLevel;
    private int speedUpgradeLevel = 0;

    [SerializeField] private Collider currentCollider;
    private float digTimer = 0f;

    [Tooltip("複数の判定エリア（Transform）を追加（最大3個）")]
    public List<Transform> hitZones = new List<Transform>();
    private int activeHitZones = 1;

    private DigSoundManager soundManager;

    [Header("レイヤー/タグ制御")]
    public LayerMask drillDiggableLayers;

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

    private bool canSwitchMode = true;

    private void Start()
    {
        UpdateHitZoneVisibility();
        soundManager = FindObjectOfType<DigSoundManager>();
        if (visibleDrillTip != null)
            visibleDrillTip.SetActive(true);
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
        if (drillDiggableLayers.value == 0
            || other.CompareTag("Terrain")
            || ((drillDiggableLayers.value & (1 << other.gameObject.layer)) != 0))
        {
            currentCollider = other;
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
            return;

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
                    if (hitZones[i] == null) continue;
                    Vector3 digPosition = hitZones[i].position;

                    bool canDrillHere = true;
                    if (drillDiggableLayers.value != 0)
                    {
                        canDrillHere = Physics.CheckSphere(
                            digPosition,
                            radius * 0.35f,
                            drillDiggableLayers,
                            QueryTriggerInteraction.Ignore
                        );
                    }

                    if (!canDrillHere) continue;

                    bool digOccurred = digManager.TryDigAt(digPosition, radius);
                    if (digOccurred)
                    {

                        // チュートリアル：最初の掘削でお宝出現
                        TutorialManager.Instance?.OnFirstDig(digPosition, radius);
                        TutorialManager.Instance?.OnAnyDigSuccess(digPosition, radius);

                        soundManager?.PlayDrillDigSound(digPosition);
                        DigEffectManager.Instance?.CreateDigEffect(digPosition, radius);
                    }
                }
            }
        }
        else
        {
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
