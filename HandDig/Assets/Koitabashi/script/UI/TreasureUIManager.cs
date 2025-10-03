using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// お宝関連のUI表示を管理するマネージャー
/// </summary>
public class TreasureUIManager : MonoBehaviour
{
    [Header("UI要素")]
    [Tooltip("お宝UI表示用のCanvas")]
    public Canvas treasureCanvas;
    
    [Header("通常強化お宝表示")]
    [Tooltip("通常強化お宝のテキスト")]
    public TextMeshProUGUI normalTreasureText;
    
    [Header("ドリル速度お宝表示")]
    [Tooltip("ドリル速度お宝のテキスト")]
    public TextMeshProUGUI drillSpeedText;
    
    [Header("爆発採掘システム表示")]
    [Tooltip("爆発採掘システムの表示パネル")]
    public GameObject explosionSystemPanel;
    
    [Tooltip("爆発モード状態テキスト")]
    public TextMeshProUGUI explosionModeText;
    
    [Tooltip("爆発チャージ数テキスト")]
    public TextMeshProUGUI explosionChargesText;
    
    [Header("総取得お宝数表示")]
    [Tooltip("総取得お宝数のテキスト")]
    public TextMeshProUGUI totalTreasureText;
    
    [Header("表示設定")]
    [Tooltip("UI更新間隔（秒）")]
    public float updateInterval = 0.1f;
    
    [Tooltip("爆発システムを初回取得後に表示するか")]
    public bool showExplosionSystemAfterFirstGet = true;
    
    [Header("テキスト設定")]
    [Tooltip("テキストの色")]
    public Color textColor = Color.white;
    
    [Tooltip("テキストのサイズ")]
    public float textSize = 24f;
    
    [Header("デバッグ")]
    [Tooltip("デバッグログを出力するか")]
    public bool enableDebugLog = false;

    private static TreasureUIManager instance;
    private VRDigToolManager toolManager;
    private Coroutine updateCoroutine;
    
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static TreasureUIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TreasureUIManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("TreasureUIManager");
                    instance = go.AddComponent<TreasureUIManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUI();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // VRDigToolManagerを取得
        toolManager = FindObjectOfType<VRDigToolManager>();
        if (toolManager == null)
        {
            Debug.LogWarning("[TreasureUIManager] VRDigToolManagerが見つかりません。");
        }
        
        // UI更新を開始
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
        updateCoroutine = StartCoroutine(UpdateUICoroutine());
    }

    private void OnDestroy()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
    }

    /// <summary>
    /// UIを初期化
    /// </summary>
    private void InitializeUI()
    {
        if (treasureCanvas == null)
        {
            CreateTreasureUI();
        }
        else
        {
            SetupUI();
        }
    }

    /// <summary>
    /// お宝UIを自動作成
    /// </summary>
    private void CreateTreasureUI()
    {
        // Canvasを作成
        GameObject canvasObj = new GameObject("TreasureCanvas");
        canvasObj.transform.SetParent(transform);
        
        treasureCanvas = canvasObj.AddComponent<Canvas>();
        treasureCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        treasureCanvas.sortingOrder = 100; // 通常UIより上に表示
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 通常強化お宝表示を作成
        CreateNormalTreasureDisplay(canvasObj);
        
        // ドリル速度お宝表示を作成
        CreateDrillSpeedDisplay(canvasObj);
        
        // 爆発採掘システム表示を作成
        CreateExplosionSystemDisplay(canvasObj);
        
        // 総取得お宝数表示を作成
        CreateTotalTreasureDisplay(canvasObj);
        
        SetupUI();
    }

    /// <summary>
    /// 通常強化お宝表示を作成
    /// </summary>
    private void CreateNormalTreasureDisplay(GameObject parent)
    {
        GameObject normalObj = new GameObject("NormalTreasureDisplay");
        normalObj.transform.SetParent(parent.transform, false);
        
        normalTreasureText = normalObj.AddComponent<TextMeshProUGUI>();
        normalTreasureText.text = "通常強化: 0/5";
        normalTreasureText.color = textColor;
        normalTreasureText.fontSize = textSize;
        normalTreasureText.alignment = TextAlignmentOptions.TopLeft;
        
        RectTransform normalRect = normalTreasureText.GetComponent<RectTransform>();
        normalRect.anchorMin = new Vector2(0.02f, 0.9f);
        normalRect.anchorMax = new Vector2(0.3f, 0.95f);
        normalRect.sizeDelta = Vector2.zero;
        normalRect.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// ドリル速度お宝表示を作成
    /// </summary>
    private void CreateDrillSpeedDisplay(GameObject parent)
    {
        GameObject drillSpeedObj = new GameObject("DrillSpeedDisplay");
        drillSpeedObj.transform.SetParent(parent.transform, false);
        
        drillSpeedText = drillSpeedObj.AddComponent<TextMeshProUGUI>();
        drillSpeedText.text = "ドリル速度: 0/5";
        drillSpeedText.color = textColor;
        drillSpeedText.fontSize = textSize;
        drillSpeedText.alignment = TextAlignmentOptions.TopLeft;
        
        RectTransform drillSpeedRect = drillSpeedText.GetComponent<RectTransform>();
        drillSpeedRect.anchorMin = new Vector2(0.02f, 0.85f);
        drillSpeedRect.anchorMax = new Vector2(0.3f, 0.9f);
        drillSpeedRect.sizeDelta = Vector2.zero;
        drillSpeedRect.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// 爆発採掘システム表示を作成
    /// </summary>
    private void CreateExplosionSystemDisplay(GameObject parent)
    {
        // パネルを作成
        GameObject panelObj = new GameObject("ExplosionSystemPanel");
        panelObj.transform.SetParent(parent.transform, false);
        
        explosionSystemPanel = panelObj;
        
        // 背景画像を追加
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.02f, 0.75f);
        panelRect.anchorMax = new Vector2(0.3f, 0.82f);
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        // 爆発モード状態テキスト
        GameObject modeObj = new GameObject("ExplosionModeText");
        modeObj.transform.SetParent(panelObj.transform, false);
        
        explosionModeText = modeObj.AddComponent<TextMeshProUGUI>();
        explosionModeText.text = "爆発モード: OFF";
        explosionModeText.color = textColor;
        explosionModeText.fontSize = textSize;
        explosionModeText.alignment = TextAlignmentOptions.TopLeft;
        
        RectTransform modeRect = explosionModeText.GetComponent<RectTransform>();
        modeRect.anchorMin = new Vector2(0.05f, 0.5f);
        modeRect.anchorMax = new Vector2(0.95f, 1f);
        modeRect.sizeDelta = Vector2.zero;
        modeRect.anchoredPosition = Vector2.zero;
        
        // 爆発チャージ数テキスト
        GameObject chargesObj = new GameObject("ExplosionChargesText");
        chargesObj.transform.SetParent(panelObj.transform, false);
        
        explosionChargesText = chargesObj.AddComponent<TextMeshProUGUI>();
        explosionChargesText.text = "残りチャージ: 0";
        explosionChargesText.color = textColor;
        explosionChargesText.fontSize = textSize;
        explosionChargesText.alignment = TextAlignmentOptions.TopLeft;
        
        RectTransform chargesRect = explosionChargesText.GetComponent<RectTransform>();
        chargesRect.anchorMin = new Vector2(0.05f, 0f);
        chargesRect.anchorMax = new Vector2(0.95f, 0.5f);
        chargesRect.sizeDelta = Vector2.zero;
        chargesRect.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// 総取得お宝数表示を作成
    /// </summary>
    private void CreateTotalTreasureDisplay(GameObject parent)
    {
        GameObject totalObj = new GameObject("TotalTreasureDisplay");
        totalObj.transform.SetParent(parent.transform, false);
        
        totalTreasureText = totalObj.AddComponent<TextMeshProUGUI>();
        totalTreasureText.text = "総取得お宝数: 0";
        totalTreasureText.color = textColor;
        totalTreasureText.fontSize = textSize;
        totalTreasureText.alignment = TextAlignmentOptions.TopLeft;
        
        RectTransform totalRect = totalTreasureText.GetComponent<RectTransform>();
        totalRect.anchorMin = new Vector2(0.02f, 0.7f);
        totalRect.anchorMax = new Vector2(0.3f, 0.75f);
        totalRect.sizeDelta = Vector2.zero;
        totalRect.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// UI設定を適用
    /// </summary>
    private void SetupUI()
    {
        if (normalTreasureText != null)
        {
            normalTreasureText.color = textColor;
            normalTreasureText.fontSize = textSize;
        }
        
        if (drillSpeedText != null)
        {
            drillSpeedText.color = textColor;
            drillSpeedText.fontSize = textSize;
        }
        
        if (explosionModeText != null)
        {
            explosionModeText.color = textColor;
            explosionModeText.fontSize = textSize;
        }
        
        if (explosionChargesText != null)
        {
            explosionChargesText.color = textColor;
            explosionChargesText.fontSize = textSize;
        }
        
        if (totalTreasureText != null)
        {
            totalTreasureText.color = textColor;
            totalTreasureText.fontSize = textSize;
        }
    }

    /// <summary>
    /// UI更新のコルーチン
    /// </summary>
    private IEnumerator UpdateUICoroutine()
    {
        while (true)
        {
            UpdateUI();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    /// <summary>
    /// UIを更新
    /// </summary>
    private void UpdateUI()
    {
        if (toolManager == null) return;
        
        // 通常強化お宝の表示更新
        UpdateNormalTreasureDisplay();
        
        // ドリル速度お宝の表示更新
        UpdateDrillSpeedDisplay();
        
        // 爆発採掘システムの表示更新
        UpdateExplosionSystemDisplay();
        
        // 総取得お宝数の表示更新
        UpdateTotalTreasureDisplay();
    }

    /// <summary>
    /// 通常強化お宝の表示を更新
    /// </summary>
    private void UpdateNormalTreasureDisplay()
    {
        if (normalTreasureText == null || toolManager == null) return;
        
        // 現在のツールの強化レベルを取得
        int currentIndex = toolManager.GetCurrentToolIndex();
        var toolData = toolManager.toolDataList;
        
        if (currentIndex >= 0 && currentIndex < toolData.Count)
        {
            var data = toolData[currentIndex];
            int currentLevel = data.currentUpgradeLevel;
            int maxLevel = 5; // デフォルト値
            
            // 各ツールの最大レベルを取得
            if (data.handStats != null)
                maxLevel = data.handStats.GetMaxUpgradeLevel();
            else if (data.pickaxeStats != null)
                maxLevel = data.pickaxeStats.GetMaxUpgradeLevel();
            else if (data.drillStats != null)
                maxLevel = data.drillStats.GetMaxUpgradeLevel();
            
            normalTreasureText.text = $"通常強化: {currentLevel}/{maxLevel}";
        }
    }

    /// <summary>
    /// ドリル速度お宝の表示を更新
    /// </summary>
    private void UpdateDrillSpeedDisplay()
    {
        if (drillSpeedText == null || toolManager == null) return;
        
        int currentLevel = toolManager.GetDrillSpeedLevel();
        int maxLevel = toolManager.GetDrillMaxSpeedLevel();
        
        drillSpeedText.text = $"ドリル速度: {currentLevel}/{maxLevel}";
    }

    /// <summary>
    /// 爆発採掘システムの表示を更新
    /// </summary>
    private void UpdateExplosionSystemDisplay()
    {
        if (explosionSystemPanel == null || explosionModeText == null || explosionChargesText == null || toolManager == null) return;
        
        bool isUnlocked = toolManager.IsPickaxeExplosionUnlocked();
        int charges = toolManager.GetPickaxeExplosionCharges();
        
        // 初回取得後に表示する設定の場合
        if (showExplosionSystemAfterFirstGet && !isUnlocked)
        {
            explosionSystemPanel.SetActive(false);
            return;
        }
        
        explosionSystemPanel.SetActive(true);
        
        // 爆発モード状態を取得（PickaxeDigToolMasterから）
        bool isExplosionMode = false;
        var currentTool = toolManager.GetCurrentTool();
        if (currentTool is PickaxeDigToolMaster pickaxeMaster)
        {
            isExplosionMode = pickaxeMaster.IsExplosionMode();
        }
        
        explosionModeText.text = $"爆発モード: {(isExplosionMode ? "ON" : "OFF")}";
        explosionChargesText.text = $"残りチャージ: {charges}";
        
        // チャージ数に応じて色を変更
        if (charges > 0)
        {
            explosionChargesText.color = Color.green;
        }
        else
        {
            explosionChargesText.color = Color.red;
        }
    }

    /// <summary>
    /// 総取得お宝数の表示を更新
    /// </summary>
    private void UpdateTotalTreasureDisplay()
    {
        if (totalTreasureText == null || toolManager == null) return;
        
        int totalCount = toolManager.GetTotalTreasureCount();
        totalTreasureText.text = $"総取得お宝数: {totalCount}";
    }

    /// <summary>
    /// UIの表示/非表示を切り替え
    /// </summary>
    public void SetUIVisible(bool visible)
    {
        if (treasureCanvas != null)
        {
            treasureCanvas.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// 爆発システムパネルの表示/非表示を切り替え
    /// </summary>
    public void SetExplosionSystemVisible(bool visible)
    {
        if (explosionSystemPanel != null)
        {
            explosionSystemPanel.SetActive(visible);
        }
    }
}
