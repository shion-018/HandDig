using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeDigToolMaster : MonoBehaviour, IDigToolWithStats
{
    [Header("Core")]
    public VoxelDigManager digManager;

    // --- 従来（互換）方式：Transform の配列（各 Transform が掘りポイント） ---
    [Tooltip("従来互換：複数の判定エリアを追加（最大3個など）")]
    public List<Transform> hitZones = new List<Transform>();
    private int activeHitZones = 1; // 従来の段階的増加方式向け

    // --- 新方式：強化段階ごとのヒットゾーンオブジェクト（丸ごとON/OFF） ---
    [Header("強化段階ごとのヒットゾーン（丸ごとオブジェクト）")]
    [Tooltip("各要素が強化レベルごとのヒットゾーン（子に複数コライダーを持てる）")]
    public List<GameObject> hitZoneObjects = new List<GameObject>();
    [Tooltip("true の場合は hitZoneObjects を使う（レベルごとに丸ごと切り替え）")]
    public bool useLevelHitZoneObjects = false;
    private int currentHitZoneLevel = 0;
    private GameObject currentHitZoneObject = null;
    private List<Transform> currentHitPoints = new List<Transform>(); // 現在使っている掘りポイント（コライダーの transform など）

    // Stats & upgrade
    private PickaxeDigStats stats;
    private int upgradeLevel = 0;

    // Combo timing
    public float minComboTime = 0.5f;
    public float maxComboTime = 1.5f;
    private float lastDigTime = -10f;
    private int comboStage = 0;

    // Swing ready
    private bool isSwingReady = false;

    // SwingReadyZone の設定（従来互換）
    [Header("振りかぶりゾーン設定（従来互換）")]
    [Tooltip("振りかぶりゾーンのコライダー")]
    public Collider swingZoneCollider;
    [Tooltip("振りかぶりゾーンに入った時のタグ")]
    public string swingZoneTag = "SwingZone";

    // 爆発モード関連（元のロジック）
    private VRDigToolManager toolManager;
    private bool isExplosionMode = false;
    [Tooltip("地形レイヤー（必要ならRaycastで使用）")]
    public LayerMask terrainLayer;

    void Awake()
    {
        toolManager = FindObjectOfType<VRDigToolManager>();
        // 初期設定
        if (useLevelHitZoneObjects && hitZoneObjects != null && hitZoneObjects.Count > 0)
        {
            SetHitZoneLevel(0);
        }
        else
        {
            // 従来互換モード：活性ヒット数に応じて表示を更新
            UpdateHitZoneVisibility();
            CollectTransformsFromHitZones(); // currentHitPoints を補助的に作る（必要なら）
        }
    }

    void Start()
    {
        // Start 時点でも更新（エディタでの切り替え反映等）
        if (useLevelHitZoneObjects)
            SetHitZoneLevel(currentHitZoneLevel);
        else
            UpdateHitZoneVisibility();
    }

    void Update()
    {
        // ---------- PCテスト用キー ----------
        // Space: 振りかぶり準備をOn（VRではトリガーと組み合わせて OnTriggerEnter でも有効）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetSwingReady(true);
            Debug.Log("[PickaxeMaster][TEST] Space: SwingReady = true");
        }

        // G: SwingReady を解除（デバッグ用）
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetSwingReady(false);
            Debug.Log("[PickaxeMaster][TEST] G: SwingReady = false");
        }

        // F: 強制掘り（実際に掘りを試みる。振りかぶりが必要。）
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("[PickaxeMaster][TEST] F: Force OnAnyHit()");
            OnAnyHit();
        }
        // ---------------------------------

        // 爆発モード切り替え（元のキー X / VRボタン）
        if (OVRInput.GetDown(OVRInput.Button.Three) || Input.GetKeyDown(KeyCode.X))
        {
            if (toolManager != null && stats != null && stats.enableExplosionMode && toolManager.IsPickaxeExplosionUnlocked() && toolManager.GetPickaxeExplosionCharges() > 0)
            {
                isExplosionMode = !isExplosionMode;
                Debug.Log($"[PickaxeMaster] 爆発モード: {isExplosionMode} (残り {toolManager.GetPickaxeExplosionCharges()} 回)");
            }
        }
    }

    // -------------------- Stats セット（互換） --------------------
    public void SetStats(DigToolStats newStats, int level)
    {
        Debug.LogWarning("[PickaxeDigToolMaster] SetStats(DigToolStats) is deprecated. Use SetPickaxeStats instead.");
    }

    public void SetPickaxeStats(PickaxeDigStats newStats, int level)
    {
        stats = newStats;
        upgradeLevel = level;
        Debug.Log($"[PickaxeMaster] SetStats: Level {upgradeLevel}");
    }

    public void SetHandStats(HandDigStats newStats, int level) { /* 未実装 */ }
    public void SetDrillStats(DrillDigStats newStats, int level) { /* 未実装 */ }

    // -------------------- Swing Ready --------------------
    public void SetSwingReady(bool ready)
    {
        isSwingReady = ready;
        Debug.Log($"[PickaxeMaster] SwingReady = {ready}");
    }

    public bool IsSwingReady()
    {
        return isSwingReady;
    }

    // -------------------- ヒットゾーン管理 --------------------
    /// <summary>
    /// IncreaseHitZone はモードに応じて動作：
    /// - useLevelHitZoneObjects == true の場合は「レベルを +1 にしてそのオブジェクトを有効化」
    /// - そうでない場合は activeHitZones を増加（従来の挙動）
    /// </summary>
    public void IncreaseHitZone()
    {
        if (useLevelHitZoneObjects && hitZoneObjects != null && hitZoneObjects.Count > 0)
        {
            int next = Mathf.Clamp(currentHitZoneLevel + 1, 0, hitZoneObjects.Count - 1);
            SetHitZoneLevel(next);
            Debug.Log($"[PickaxeMaster] レベルヒットゾーンへ切替: {next}");
        }
        else
        {
            activeHitZones = Mathf.Min(activeHitZones + 1, hitZones.Count);
            Debug.Log($"[PickaxeMaster] 判定数が {activeHitZones} になりました (従来互換)");
            UpdateHitZoneVisibility();
            CollectTransformsFromHitZones();
        }
    }

    /// <summary>
    /// 従来互換：hitZones の可視化を更新
    /// </summary>
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

    /// <summary>
    /// レベルヒットゾーン（丸ごとオブジェクト）を切り替える
    /// </summary>
    public void SetHitZoneLevel(int level)
    {
        if (hitZoneObjects == null || hitZoneObjects.Count == 0)
        {
            Debug.LogWarning("[PickaxeMaster] hitZoneObjects が設定されていません");
            return;
        }

        level = Mathf.Clamp(level, 0, hitZoneObjects.Count - 1);
        currentHitZoneLevel = level;

        for (int i = 0; i < hitZoneObjects.Count; i++)
        {
            if (hitZoneObjects[i] != null)
            {
                hitZoneObjects[i].SetActive(i == level);
            }
        }

        currentHitZoneObject = hitZoneObjects[level];
        CollectHitPointsFromCurrentObject();

        Debug.Log($"[PickaxeMaster] 強化レベル {level} のヒットゾーンを使用中");
    }

    /// <summary>
    /// 現在の HitZoneObject の子にある Collider.transform を掘りポイントとして収集
    /// </summary>
    private void CollectHitPointsFromCurrentObject()
    {
        currentHitPoints.Clear();

        if (currentHitZoneObject == null) return;

        // GetComponentsInChildren<Collider>() を使って、コライダーの transform を収集
        Collider[] cols = currentHitZoneObject.GetComponentsInChildren<Collider>(includeInactive: true);
        foreach (var c in cols)
        {
            currentHitPoints.Add(c.transform);
        }

        Debug.Log($"[PickaxeMaster] {currentHitPoints.Count} 個の掘りポイントを登録（レベル {currentHitZoneLevel}）");
    }

    /// <summary>
    /// hitZones (Transform list) から currentHitPoints を作る（互換サポート）
    /// </summary>
    private void CollectTransformsFromHitZones()
    {
        currentHitPoints.Clear();
        for (int i = 0; i < Mathf.Min(activeHitZones, hitZones.Count); i++)
        {
            if (hitZones[i] != null)
                currentHitPoints.Add(hitZones[i]);
        }
        Debug.Log($"[PickaxeMaster] (互換) {currentHitPoints.Count} 個の掘りポイントを登録しました");
    }

    // -------------------- 振りかぶりゾーンのトリガー（従来互換） --------------------
    // 注意：この OnTriggerEnter はこのコンポーネントがアタッチされた GameObject の Collider が
    //       trigger になっている、または外部から呼ばれている前提です。
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(swingZoneTag))
        {
            bool isTriggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
            bool isSpaceHeld = Input.GetKey(KeyCode.Space);

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

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(swingZoneTag))
        {
            // 従来の設計ではゾーンから出ても SwingReady は維持する（掘り判定に触れるまで）
            Debug.Log("[PickaxeMaster] 振りかぶりゾーンから出ましたが、振りかぶり状態は維持中...");
        }
    }

    // SwingReady zone から明示的に出たときに呼ぶ想定のメソッド
    public void OnSwingZoneExit()
    {
        isSwingReady = false;
        Debug.Log("[PickaxeMaster] SwingReady = false (ゾーンから出ました)");
    }

    // -------------------- 掘り処理（コア） --------------------
    public void OnAnyHit()
    {
        // 入力取得（VR と PC 両対応のため）
        bool isTriggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        bool isSpaceHeld = Input.GetKey(KeyCode.Space);

        // 振りかぶりかつトリガー/スペース押下が必要（PCテスト時は Space を用いる）
        if (isSwingReady && (isTriggerHeld || isSpaceHeld) && stats != null)
        {
            float currentTime = Time.time;
            float timeSinceLast = currentTime - lastDigTime;

            if (timeSinceLast >= minComboTime && timeSinceLast <= maxComboTime)
                comboStage = Mathf.Min(comboStage + 1, 2);
            else
                comboStage = 0;

            lastDigTime = currentTime;

            // 掘った後はSwingReadyをリセット（次の振りかぶりを待つ）
            isSwingReady = false;

            float radius = stats.GetRadius(comboStage, upgradeLevel);

            // currentHitPoints に登録されたすべてのポイントで掘る
            for (int i = 0; i < currentHitPoints.Count; i++)
            {
                Transform t = currentHitPoints[i];
                if (t == null) continue;

                Vector3 upwardOffset = t.up * (radius * 0.3f);
                Vector3 digPosition = t.position + upwardOffset;

                bool digOccurred = false;
                if (digManager != null)
                {
                    digOccurred = digManager.TryDigAt(digPosition, radius);
                }

                if (digOccurred)
                {

                    // --- チュートリアル通知（最初の1回用） ---
                    TutorialManager.Instance?.OnFirstDig(digPosition, radius);

                    // --- 「つるはしで1回掘る」ステップ用 ---
                    TutorialManager.Instance?.OnAnyDigSuccess();

                    if (DigEffectManager.Instance != null)
                    {
                        DigEffectManager.Instance.CreateDigEffect(digPosition, radius);
                    }
                    var soundManager = DigSoundManager.Instance;
                    if (soundManager != null)
                    {
                        soundManager.PlayPickaxeDigSound(comboStage, digPosition);
                    }

                    Debug.Log($"[PickaxeMaster] 掘削発生: ポイント {i + 1} / Combo {comboStage + 1} / radius {radius:F2} / Y {upwardOffset.y:F2}");
                }
                else
                {
                    Debug.Log($"[PickaxeMaster] ポイント {i + 1} にボクセルなし（エフェクト/音スキップ）");
                }
            }

            // ---------- 爆発モード処理（元のロジックを復元） ----------
            if (isExplosionMode && toolManager != null && toolManager.GetPickaxeExplosionCharges() > 0)
            {
                if (toolManager.TryConsumePickaxeExplosionCharge())
                {
                    float explosionRadius = stats.GetExplosionRadius(upgradeLevel);
                    Vector3 pos = GetFarthestExplosionPosition(radius);
                    SpawnExplosionMarker(pos, explosionRadius, stats.explosionDelaySeconds);
                    Debug.Log($"[PickaxeMaster] 爆発マーカー設置（残りチャージ: {toolManager.GetPickaxeExplosionCharges()}）");

                    if (toolManager.GetPickaxeExplosionCharges() <= 0)
                    {
                        isExplosionMode = false;
                    }
                }
            }

            Debug.Log("[PickaxeMaster] 掘り完了（OnAnyHit）。次の振りかぶりを待機中...");
        }
        else
        {
            // デバッグ向け詳細ログ（原因を特定しやすくする）
            if (!isSwingReady)
            {
                Debug.Log("[PickaxeMaster] 振りかぶり準備ができていません。後ろに振りかぶってゾーンに入ってください。");
            }
            else if (!isTriggerHeld && !isSpaceHeld)
            {
                Debug.Log("[PickaxeMaster] トリガー/スペースが押されていません。入力してください。");
            }
            else if (stats == null)
            {
                Debug.LogWarning("[PickaxeMaster] stats が設定されていません");
            }
        }
    }

    public void UpdateDig(Vector3 pos) { /* 何もしない（必要なら拡張） */ }

    // -------------------- 爆発のための位置決め --------------------
    private Vector3 GetExplosionPosition(Transform t)
    {
        if (t == null || stats == null)
            return transform.position;

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

    /// <summary>
    /// 現在のヒットポイント群の中で最も外側（forward 方向に最も遠い）位置を爆発中心候補として返す
    /// </summary>
    private Vector3 GetFarthestExplosionPosition(float digRadius)
    {
        Vector3 bestPos = transform.position;
        float bestDist = -Mathf.Infinity;

        // currentHitPoints を基に評価（レベル切替 or 互換どちらでも currentHitPoints が埋まっている）
        for (int i = 0; i < currentHitPoints.Count; i++)
        {
            Transform t = currentHitPoints[i];
            if (t == null) continue;

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
        if (stats == null)
        {
            Debug.LogWarning("[PickaxeMaster] stats が未設定のため爆発マーカーを生成できません");
            return;
        }

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

    // 爆発モード取得
    public bool IsExplosionMode()
    {
        return isExplosionMode;
    }
}
