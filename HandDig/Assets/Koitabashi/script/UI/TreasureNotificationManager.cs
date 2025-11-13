using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// お宝取得時の通知表示を管理するマネージャー
/// </summary>
public class TreasureNotificationManager : MonoBehaviour
{
    [Header("UI要素")]
    [Tooltip("通知表示用のCanvas")]
    public Canvas notificationCanvas;
    
    [Tooltip("通知テキスト")]
    public TextMeshProUGUI notificationText;
    
    [Tooltip("背景パネル")]
    public Image backgroundPanel;
    
    [Header("お宝メッセージ設定")]
    [Tooltip("通常強化お宝のメッセージ")]
    public string normalTreasureMessage = "ツール強化お宝を取得！";
    
    [Tooltip("つるはし判定数増加お宝のメッセージ")]
    public string pickaxeHitZoneMessage = "つるはし判定数増加お宝を取得！";
    
    [Tooltip("ドリル判定数増加お宝のメッセージ")]
    public string drillHitZoneMessage = "ドリル判定数増加お宝を取得！";
    
    [Tooltip("ドリル速度増加お宝のメッセージ")]
    public string drillSpeedMessage = "ドリル速度増加お宝を取得！";
    
    [Tooltip("爆発採掘お宝のメッセージ")]
    public string explosiveMessage = "爆発採掘お宝を取得！";
    
    [Header("表示設定")]
    [Tooltip("表示時間（秒）")]
    public float displayDuration = 3f;
    
    [Tooltip("フェードイン時間（秒）")]
    public float fadeInDuration = 0.5f;
    
    [Tooltip("フェードアウト時間（秒）")]
    public float fadeOutDuration = 0.5f;
    
    [Header("テキスト設定")]
    [Tooltip("テキストの色")]
    public Color textColor = Color.white;
    
    [Tooltip("テキストのサイズ")]
    public float textSize = 48f;
    
    [Header("背景設定")]
    [Tooltip("背景の色")]
    public Color backgroundColor = new Color(0, 0, 0, 0.8f);
    
    [Header("デバッグ")]
    [Tooltip("デバッグログを出力するか")]
    public bool enableDebugLog = true;

    private static TreasureNotificationManager instance;
    private Coroutine currentNotification;
    
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static TreasureNotificationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TreasureNotificationManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("TreasureNotificationManager");
                    instance = go.AddComponent<TreasureNotificationManager>();
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

    /// <summary>
    /// UIを初期化
    /// </summary>
    private void InitializeUI()
    {
        if (notificationCanvas == null)
        {
            CreateNotificationUI();
        }
        else
        {
            SetupUI();
        }
        
        // 初期状態では非表示
        HideNotification(false);
    }

    /// <summary>
    /// 通知UIを自動作成
    /// </summary>
    private void CreateNotificationUI()
    {
        // Canvasを作成
        GameObject canvasObj = new GameObject("TreasureNotificationCanvas");
        canvasObj.transform.SetParent(transform);
        
        notificationCanvas = canvasObj.AddComponent<Canvas>();
        notificationCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        notificationCanvas.sortingOrder = 1000; // 最前面に表示
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 背景パネルを作成
        GameObject panelObj = new GameObject("BackgroundPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        backgroundPanel = panelObj.AddComponent<Image>();
        backgroundPanel.color = backgroundColor;
        
        RectTransform panelRect = backgroundPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.8f);
        panelRect.anchorMax = new Vector2(0.5f, 0.8f);
        panelRect.sizeDelta = new Vector2(600, 100);
        panelRect.anchoredPosition = Vector2.zero;
        
        // テキストを作成
        GameObject textObj = new GameObject("NotificationText");
        textObj.transform.SetParent(panelObj.transform, false);
        
        notificationText = textObj.AddComponent<TextMeshProUGUI>();
        notificationText.text = "お宝を取得！";
        notificationText.color = textColor;
        notificationText.fontSize = textSize;
        notificationText.alignment = TextAlignmentOptions.Center;
        notificationText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = notificationText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        SetupUI();
    }

    /// <summary>
    /// UI設定を適用
    /// </summary>
    private void SetupUI()
    {
        if (notificationText != null)
        {
            notificationText.color = textColor;
            notificationText.fontSize = textSize;
        }
        
        if (backgroundPanel != null)
        {
            backgroundPanel.color = backgroundColor;
        }
    }

    /// <summary>
    /// お宝取得通知を表示
    /// </summary>
    /// <param name="treasureType">お宝の種類</param>
    public void ShowTreasureNotification(string treasureType)
    {
        string message = GetTreasureMessage(treasureType);
        ShowNotification(message);
    }

    /// <summary>
    /// カスタム通知を表示
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    public void ShowNotification(string message)
    {
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }
        
        currentNotification = StartCoroutine(ShowNotificationCoroutine(message));
        
        if (enableDebugLog)
        {
            Debug.Log($"[TreasureNotification] 通知表示: {message}");
        }
    }

    /// <summary>
    /// 通知表示のコルーチン
    /// </summary>
    private IEnumerator ShowNotificationCoroutine(string message)
    {
        // テキストを設定
        if (notificationText != null)
        {
            notificationText.text = message;
        }
        
        // フェードイン
        yield return StartCoroutine(FadeIn());
        
        // 表示時間待機
        yield return new WaitForSeconds(displayDuration);
        
        // フェードアウト
        yield return StartCoroutine(FadeOut());
        
        currentNotification = null;
    }

    /// <summary>
    /// フェードイン
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color startColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0f);
        Color endColor = backgroundColor;
        Color startTextColor = new Color(textColor.r, textColor.g, textColor.b, 0f);
        Color endTextColor = textColor;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            
            if (backgroundPanel != null)
            {
                backgroundPanel.color = Color.Lerp(startColor, endColor, t);
            }
            
            if (notificationText != null)
            {
                notificationText.color = Color.Lerp(startTextColor, endTextColor, t);
            }
            
            yield return null;
        }
        
        // 最終値を設定
        if (backgroundPanel != null) backgroundPanel.color = endColor;
        if (notificationText != null) notificationText.color = endTextColor;
    }

    /// <summary>
    /// フェードアウト
    /// </summary>
    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color startColor = backgroundColor;
        Color endColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0f);
        Color startTextColor = textColor;
        Color endTextColor = new Color(textColor.r, textColor.g, textColor.b, 0f);
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            if (backgroundPanel != null)
            {
                backgroundPanel.color = Color.Lerp(startColor, endColor, t);
            }
            
            if (notificationText != null)
            {
                notificationText.color = Color.Lerp(startTextColor, endTextColor, t);
            }
            
            yield return null;
        }
        
        // 最終値を設定
        if (backgroundPanel != null) backgroundPanel.color = endColor;
        if (notificationText != null) notificationText.color = endTextColor;
    }

    /// <summary>
    /// 通知を即座に非表示
    /// </summary>
    /// <param name="withAnimation">アニメーション付きで非表示にするか</param>
    public void HideNotification(bool withAnimation = true)
    {
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
            currentNotification = null;
        }
        
        if (withAnimation)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            // 即座に非表示
            if (backgroundPanel != null)
            {
                Color transparent = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0f);
                backgroundPanel.color = transparent;
            }
            
            if (notificationText != null)
            {
                Color transparent = new Color(textColor.r, textColor.g, textColor.b, 0f);
                notificationText.color = transparent;
            }
        }
    }

    /// <summary>
    /// お宝の種類に応じたメッセージを取得
    /// </summary>
    private string GetTreasureMessage(string treasureType)
    {
        return treasureType switch
        {
            "Normal" => normalTreasureMessage,
            "PickaxeHitZone" => pickaxeHitZoneMessage,
            "DrillHitZone" => drillHitZoneMessage,
            "DrillSpeed" => drillSpeedMessage,
            "Explosive" => explosiveMessage,
            _ => "お宝を取得！"
        };
    }
}



