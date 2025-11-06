using System.Collections;
using System.Collections.Generic;
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
    [Tooltip("ドリルで掘ってよい地形のレイヤーを設定（例: Terrain のみ）")]
    public LayerMask drillDiggableLayers;

    [Header("ドリル射出設定")]
    public GameObject drillTipPrefab;     // 飛ばすドリル先端のプレハブ
    public Transform drillTipOrigin;      // 射出の起点（ドリルの先端位置）
    public float shootDuration = 3f;      // 存続時間
    private bool isShootMode = false;     // モード切替（通常 or 射出）

    [SerializeField] private float shootCooldown = 1.0f;
    private float shootTimer = 0f;

    [Header("表示用ドリル先端")]
    [SerializeField] private GameObject visibleDrillTip;
    private bool canShowTip = true;

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

    // === 射出モードの切替 ===
    private void Update()
    {
        shootTimer += Time.deltaTime;

        // 半分経過したら再表示
        if (!canShowTip && shootTimer >= shootCooldown * 0.5f)
        {
            if (visibleDrillTip != null)
                visibleDrillTip.SetActive(true);
            canShowTip = true;
        }

        if (OVRInput.GetDown(OVRInput.RawButton.X) || Input.GetKeyDown(KeyCode.X))
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
            {
                hitZones[i].gameObject.SetActive(i < activeHitZones);
            }
        }
    }

    // === 掘削＆射出動作 ===
    public void UpdateDig(Vector3 toolPosition)
    {
        bool triggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space);

        if (isShootMode)
        {
            if (triggerHeld)
            {
                ShootDrillTip();
            }
            return;
        }

        // 通常掘削モード
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
                        if (!canDrillHere)
                            continue;

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

    // === ドリル射出処理 ===
    private void ShootDrillTip()
    {
        bool triggerPressed = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) || Input.GetKeyDown(KeyCode.Space);
        if (!triggerPressed || shootTimer < shootCooldown) return;
        shootTimer = 0f;

        // 先端を非表示
        if (visibleDrillTip != null)
            visibleDrillTip.SetActive(false);
        canShowTip = false;

        // プレハブを生成して飛ばす
        GameObject tip = Instantiate(drillTipPrefab, drillTipOrigin.position, drillTipOrigin.rotation);
        DrillTipProjectile projectile = tip.AddComponent<DrillTipProjectile>();
        projectile.Initialize(digManager, stats, upgradeLevel, drillDiggableLayers, shootDuration, soundManager, speedUpgradeLevel);
    }


}
