using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeDigToolMaster : MonoBehaviour, IDigToolWithStats
{
    public VoxelDigManager digManager;

    [Tooltip("複数の判定エリアを追加（最大3個）")]
    public List<Transform> hitZones = new List<Transform>();

    private PickaxeDigStats stats;
    private int upgradeLevel;

    public float minComboTime = 0.5f;
    public float maxComboTime = 1.5f;

    private float lastDigTime = -10f;
    private int comboStage = 0;

    private bool isSwingReady = false;
    private int activeHitZones = 1; // 1個からスタート

    // SwingReadyZoneの機能を統合
    [Header("振りかぶりゾーン設定")]
    [Tooltip("振りかぶりゾーンのコライダー")]
    public Collider swingZoneCollider;
    [Tooltip("振りかぶりゾーンに入った時のタグ")]
    public string swingZoneTag = "SwingZone";

    // 爆発モード関連
    private VRDigToolManager toolManager;
    private bool isExplosionMode = false;
    [Tooltip("地形レイヤー（必要ならRaycastで使用）")]
    public LayerMask terrainLayer;

    // 音声管理
    private DigSoundManager soundManager;

    void Start()
    {
        // 初期化時に判定エリアの表示/非表示を設定
        UpdateHitZoneVisibility();
    }

    void Awake()
    {
        toolManager = FindObjectOfType<VRDigToolManager>();
        soundManager = FindObjectOfType<DigSoundManager>();
    }

    public void SetStats(DigToolStats newStats, int level)
    {
        // 後方互換性のため残す
        Debug.LogWarning("[PickaxeDigToolMaster] SetStats(DigToolStats) is deprecated. Use SetPickaxeStats instead.");
    }

    public void SetPickaxeStats(PickaxeDigStats newStats, int level)
    {
        stats = newStats;
        upgradeLevel = level;
        Debug.Log($"[PickaxeMaster] SetStats: Level {upgradeLevel}");
    }

    public void SetHandStats(HandDigStats newStats, int level) { /* Pickaxe用なので未実装 */ }
    public void SetDrillStats(DrillDigStats newStats, int level) { /* Pickaxe用なので未実装 */ }

    public void SetSwingReady(bool ready)
    {
        isSwingReady = ready;
        Debug.Log($"[PickaxeMaster] SwingReady = {ready}");
    }

    // isSwingReadyを確認するためのパブリックメソッド
    public bool IsSwingReady()
    {
        return isSwingReady;
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

    // 振りかぶりゾーンに入った時の処理
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(swingZoneTag))
        {
            bool isTriggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
            bool isSpaceHeld = Input.GetKey(KeyCode.Space);
            
            // トリガーを押している時のみ振りかぶり準備を有効にする
            if (isTriggerHeld || isSpaceHeld)
            {
                SetSwingReady(true);
                Debug.Log("[PickaxeMaster] 振りかぶりゾーンに入りました（トリガー押下中）");
            }
            else
            {
                Debug.Log("[PickaxeMaster] 振りかぶりゾーンに入りましたが、トリガーを押していません");
            }
        }
    }

    // 振りかぶりゾーンから出た時の処理
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(swingZoneTag))
        {
            // ゾーンから出ても振りかぶり状態は維持（掘り判定に触れるまで）
            Debug.Log("[PickaxeMaster] 振りかぶりゾーンから出ましたが、振りかぶり状態は維持中...");
        }
    }

    public void OnAnyHit()
    {
        bool isTriggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        bool isSpaceHeld = Input.GetKey(KeyCode.Space);

        // 振りかぶり準備ができていて、トリガーを押している場合のみ掘る
        if (isSwingReady && (isTriggerHeld || isSpaceHeld) && stats != null)
        {
            float currentTime = Time.time;
            float timeSinceLast = currentTime - lastDigTime;

            if (timeSinceLast >= minComboTime && timeSinceLast <= maxComboTime)
                comboStage = Mathf.Min(comboStage + 1, 2);
            else
                comboStage = 0;

            lastDigTime = currentTime;
            
            // 掘った後は振りかぶり状態をリセット（次の振りかぶりを待つ）
            isSwingReady = false;

            float radius = stats.GetRadius(comboStage, upgradeLevel);

            for (int i = 0; i < activeHitZones; i++)
            {
                Transform t = hitZones[i];

                Vector3 upwardOffset = t.up * (radius * 0.3f);
                Vector3 digPosition = t.position + upwardOffset;

                digManager.DigAt(digPosition, radius);

                // 掘削音を再生
                if (soundManager != null)
                {
                    soundManager.PlayPickaxeDigSound(comboStage, digPosition);
                }

                Debug.Log($"[PickaxeMaster] 判定{i + 1} Combo {comboStage + 1} / radius: {radius} / Y: {upwardOffset.y:F2}");
            }
            
            // 爆発モード: 通常掘削に加えてマーカーを1つ設置（チャージ消費）
            if (isExplosionMode && toolManager != null && toolManager.GetPickaxeExplosionCharges() > 0)
            {
                // チャージ消費に成功したらマーカー設置
                if (toolManager.TryConsumePickaxeExplosionCharge())
                {
                    float explosionRadius = stats.GetExplosionRadius(upgradeLevel);
                    Vector3 pos = GetFarthestExplosionPosition(radius);
                    SpawnExplosionMarker(pos, explosionRadius, stats.explosionDelaySeconds);
                    
                    // 爆発マーカー設置音を再生
                    if (soundManager != null)
                    {
                        soundManager.PlayPickaxeExplosionMarkerSound(pos);
                    }
                    
                    Debug.Log($"[PickaxeMaster] 爆発マーカー設置（残りチャージ: {toolManager.GetPickaxeExplosionCharges()}）");

                    if (toolManager.GetPickaxeExplosionCharges() <= 0)
                    {
                        isExplosionMode = false;
                    }
                }
            }

            Debug.Log("[PickaxeMaster] 掘り完了！次の振りかぶりを待機中...");
        }
        else if (!isSwingReady)
        {
            // 振りかぶり準備ができていない場合は何もしない
            Debug.Log("[PickaxeMaster] 振りかぶり準備ができていません。後ろに振りかぶってゾーンに入ってください。");
        }
        else if (!isTriggerHeld && !isSpaceHeld)
        {
            // トリガーを押していない場合
            Debug.Log("[PickaxeMaster] トリガーを押してください。");
        }
    }
    
    public void UpdateDig(Vector3 pos) { /* 何もせず */ }
    
    // SwingReadyZoneから出た時に呼ばれる
    public void OnSwingZoneExit()
    {
        isSwingReady = false;
        Debug.Log("[PickaxeMaster] SwingReady = false (ゾーンから出ました)");
    }

    void Update()
    {
        // 爆発モード切り替え（左コントローラX / キーボードX）
        if (OVRInput.GetDown(OVRInput.Button.Three) || Input.GetKeyDown(KeyCode.X))
        {
            if (toolManager != null && stats != null && stats.enableExplosionMode && toolManager.IsPickaxeExplosionUnlocked() && toolManager.GetPickaxeExplosionCharges() > 0)
            {
                isExplosionMode = !isExplosionMode;
                Debug.Log($"[PickaxeMaster] 爆発モード: {isExplosionMode} (残り {toolManager.GetPickaxeExplosionCharges()} 回)");
            }
        }
    }

    private Vector3 GetExplosionPosition(Transform t)
    {
        // 既存のdigPositionロジックと同等の位置を利用
        float baseRadius = stats.GetRadius(comboStage, upgradeLevel);
        Vector3 upwardOffset = t.up * (baseRadius * 0.3f);
        Vector3 center = t.position + upwardOffset;
        Vector3 dir = t.forward.normalized;

        float epsilon = 0.05f;
        Ray ray = new Ray(center, dir);
        if (Physics.Raycast(ray, out RaycastHit hit, baseRadius, terrainLayer))
        {
            return hit.point - dir * epsilon;
        }
        else
        {
            return center + dir * (baseRadius - epsilon);
        }
    }

    private Vector3 GetFarthestExplosionPosition(float digRadius)
    {
        Vector3 bestPos = transform.position;
        float bestDist = -Mathf.Infinity;

        int count = Mathf.Min(activeHitZones, hitZones.Count);
        for (int i = 0; i < count; i++)
        {
            Transform t = hitZones[i];
            // 掘削中心（通常掘削と同等）
            Vector3 center = t.position + t.up * (digRadius * 0.3f);
            Vector3 dir = t.forward.normalized;
            float epsilon = 0.05f;

            Vector3 candidate;
            Ray ray = new Ray(center, dir);
            if (Physics.Raycast(ray, out RaycastHit hit, digRadius, terrainLayer))
            {
                candidate = hit.point - dir * epsilon;
            }
            else
            {
                candidate = center + dir * (digRadius - epsilon);
            }

            float proj = Vector3.Dot(candidate - center, dir);
            if (proj > bestDist)
            {
                bestDist = proj;
                bestPos = candidate;
            }
        }

        return bestPos;
    }

    private void SpawnExplosionMarker(Vector3 position, float radius, float delaySeconds)
    {
        if (stats.explosionMarkerPrefab == null)
        {
            Debug.LogWarning("[PickaxeMaster] explosionMarkerPrefab が未設定です");
            return;
        }

        GameObject go = Instantiate(stats.explosionMarkerPrefab, position, Quaternion.identity);
        var marker = go.GetComponent<ExplosiveMarker>();
        if (marker != null)
        {
            marker.Initialize(digManager, radius, delaySeconds);
        }
        else
        {
            Debug.LogWarning("[PickaxeMaster] ExplosiveMarker コンポーネントが見つかりません");
        }
    }
}
