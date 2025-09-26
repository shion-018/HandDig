using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ゴール時のUI表示を管理するスクリプト
/// </summary>
public class GoalUI : MonoBehaviour
{
    [Header("UI要素")]
    [Tooltip("ゴール表示用のCanvas")]
    public Canvas goalCanvas;
    
    [Tooltip("ゴールテキスト")]
    public TextMeshProUGUI goalText;
    
    [Tooltip("背景パネル")]
    public Image backgroundPanel;
    
    [Header("テキスト設定")]
    [Tooltip("ゴール時の表示テキスト")]
    public string goalMessage = "ゴール！";
    
    [Tooltip("テキストの色")]
    public Color textColor = Color.white;
    
    [Tooltip("テキストのサイズ")]
    public float textSize = 72f;
    
    [Header("アニメーション設定")]
    [Tooltip("表示アニメーション時間（秒）")]
    public float showAnimationDuration = 0.5f;
    
    [Tooltip("非表示アニメーション時間（秒）")]
    public float hideAnimationDuration = 0.5f;
    
    [Tooltip("アニメーションのイージング")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("背景設定")]
    [Tooltip("背景の色")]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    
    [Tooltip("背景を表示するか")]
    public bool showBackground = true;
    
    [Header("デバッグ")]
    [Tooltip("デバッグログを出力するか")]
    public bool enableDebugLog = true;

    private static GoalUI instance;
    private bool isShowing = false;
    private Coroutine currentAnimation;
    
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static GoalUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GoalUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GoalUI");
                    instance = go.AddComponent<GoalUI>();
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
        if (goalCanvas == null)
        {
            CreateGoalUI();
        }
        else
        {
            SetupUI();
        }
        
        // 初期状態では非表示
        HideGoalUI(false);
    }

    /// <summary>
    /// ゴールUIを自動作成
    /// </summary>
    private void CreateGoalUI()
    {
        // Canvasを作成
        GameObject canvasObj = new GameObject("GoalCanvas");
        canvasObj.transform.SetParent(transform);
        
        goalCanvas = canvasObj.AddComponent<Canvas>();
        goalCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        goalCanvas.sortingOrder = 500; // フェードより下、通常UIより上
        
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
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        // テキストを作成
        GameObject textObj = new GameObject("GoalText");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        goalText = textObj.AddComponent<TextMeshProUGUI>();
        goalText.text = goalMessage;
        goalText.color = textColor;
        goalText.fontSize = textSize;
        goalText.alignment = TextAlignmentOptions.Center;
        goalText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = goalText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        if (enableDebugLog)
        {
            Debug.Log("[GoalUI] ゴールUIを自動作成しました");
        }
    }

    /// <summary>
    /// 既存のUIを設定
    /// </summary>
    private void SetupUI()
    {
        if (goalText != null)
        {
            goalText.text = goalMessage;
            goalText.color = textColor;
            goalText.fontSize = textSize;
        }
        
        if (backgroundPanel != null)
        {
            backgroundPanel.color = backgroundColor;
        }
    }

    /// <summary>
    /// ゴールUIを表示
    /// </summary>
    /// <param name="animate">アニメーションするかどうか</param>
    public void ShowGoalUI(bool animate = true)
    {
        if (isShowing) return;
        
        isShowing = true;
        
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        if (animate)
        {
            currentAnimation = StartCoroutine(ShowAnimation());
        }
        else
        {
            SetUIAlpha(1f);
        }
        
        if (enableDebugLog)
        {
            Debug.Log("[GoalUI] ゴールUI表示");
        }
    }

    /// <summary>
    /// ゴールUIを非表示
    /// </summary>
    /// <param name="animate">アニメーションするかどうか</param>
    public void HideGoalUI(bool animate = true)
    {
        if (!isShowing) return;
        
        isShowing = false;
        
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        if (animate)
        {
            currentAnimation = StartCoroutine(HideAnimation());
        }
        else
        {
            SetUIAlpha(0f);
        }
        
        if (enableDebugLog)
        {
            Debug.Log("[GoalUI] ゴールUI非表示");
        }
    }

    /// <summary>
    /// 表示アニメーション
    /// </summary>
    private IEnumerator ShowAnimation()
    {
        float elapsedTime = 0f;
        float startAlpha = 0f;
        float endAlpha = 1f;
        
        SetUIAlpha(startAlpha);
        
        while (elapsedTime < showAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / showAnimationDuration;
            float easedT = animationCurve.Evaluate(t);
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, easedT);
            
            SetUIAlpha(currentAlpha);
            yield return null;
        }
        
        SetUIAlpha(endAlpha);
        currentAnimation = null;
    }

    /// <summary>
    /// 非表示アニメーション
    /// </summary>
    private IEnumerator HideAnimation()
    {
        float elapsedTime = 0f;
        float startAlpha = 1f;
        float endAlpha = 0f;
        
        SetUIAlpha(startAlpha);
        
        while (elapsedTime < hideAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / hideAnimationDuration;
            float easedT = animationCurve.Evaluate(t);
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, easedT);
            
            SetUIAlpha(currentAlpha);
            yield return null;
        }
        
        SetUIAlpha(endAlpha);
        currentAnimation = null;
    }

    /// <summary>
    /// UIのアルファ値を設定
    /// </summary>
    private void SetUIAlpha(float alpha)
    {
        if (goalText != null)
        {
            Color textColor = this.textColor;
            textColor.a = alpha;
            goalText.color = textColor;
        }
        
        if (backgroundPanel != null && showBackground)
        {
            Color bgColor = backgroundColor;
            bgColor.a = alpha * backgroundColor.a;
            backgroundPanel.color = bgColor;
        }
    }

    /// <summary>
    /// ゴールメッセージを変更
    /// </summary>
    /// <param name="message">新しいメッセージ</param>
    public void SetGoalMessage(string message)
    {
        goalMessage = message;
        if (goalText != null)
        {
            goalText.text = message;
        }
    }

    /// <summary>
    /// 現在表示中かどうか
    /// </summary>
    public bool IsShowing()
    {
        return isShowing;
    }

    /// <summary>
    /// テスト用：UI表示
    /// </summary>
    [ContextMenu("テスト: UI表示")]
    public void TestShowUI()
    {
        ShowGoalUI();
    }

    /// <summary>
    /// テスト用：UI非表示
    /// </summary>
    [ContextMenu("テスト: UI非表示")]
    public void TestHideUI()
    {
        HideGoalUI();
    }
}
