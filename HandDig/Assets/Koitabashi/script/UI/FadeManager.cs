using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面のフェードイン・フェードアウトを管理するマネージャー
/// </summary>
public class FadeManager : MonoBehaviour
{
    [Header("フェード設定")]
    [Tooltip("フェード用のUI Image")]
    public Image fadeImage;
    
    [Tooltip("フェードの色")]
    public Color fadeColor = Color.black;
    
    [Tooltip("フェードの速度（秒）")]
    public float fadeSpeed = 1.0f;
    
    [Header("デバッグ")]
    [Tooltip("デバッグログを出力するか")]
    public bool enableDebugLog = true;

    private static FadeManager instance;
    
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static FadeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<FadeManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("FadeManager");
                    instance = go.AddComponent<FadeManager>();
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
            InitializeFadeImage();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// フェード用のImageを初期化
    /// </summary>
    private void InitializeFadeImage()
    {
        if (fadeImage == null)
        {
            // Canvasを作成
            GameObject canvasObj = new GameObject("FadeCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // 最前面に表示
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Imageを作成
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvasObj.transform, false);
            
            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            
            RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            if (enableDebugLog)
            {
                Debug.Log("[FadeManager] フェード用UIを自動作成しました");
            }
        }
    }

    /// <summary>
    /// フェードアウト（画面を暗くする）
    /// </summary>
    /// <param name="duration">フェード時間（秒）。0の場合はfadeSpeedを使用</param>
    public Coroutine FadeOut(float duration = 0f)
    {
        if (duration <= 0f) duration = fadeSpeed;
        
        if (enableDebugLog)
        {
            Debug.Log($"[FadeManager] フェードアウト開始: {duration}秒");
        }
        
        return StartCoroutine(FadeCoroutine(0f, 1f, duration));
    }

    /// <summary>
    /// フェードイン（画面を明るくする）
    /// </summary>
    /// <param name="duration">フェード時間（秒）。0の場合はfadeSpeedを使用</param>
    public Coroutine FadeIn(float duration = 0f)
    {
        if (duration <= 0f) duration = fadeSpeed;
        
        if (enableDebugLog)
        {
            Debug.Log($"[FadeManager] フェードイン開始: {duration}秒");
        }
        
        return StartCoroutine(FadeCoroutine(1f, 0f, duration));
    }

    /// <summary>
    /// フェードアウト→フェードインの順で実行
    /// </summary>
    /// <param name="fadeOutDuration">フェードアウト時間</param>
    /// <param name="fadeInDuration">フェードイン時間</param>
    /// <param name="onFadeOutComplete">フェードアウト完了時のコールバック</param>
    public Coroutine FadeOutIn(float fadeOutDuration = 0f, float fadeInDuration = 0f, System.Action onFadeOutComplete = null)
    {
        if (fadeOutDuration <= 0f) fadeOutDuration = fadeSpeed;
        if (fadeInDuration <= 0f) fadeInDuration = fadeSpeed;
        
        if (enableDebugLog)
        {
            Debug.Log($"[FadeManager] フェードアウト→イン開始: アウト{fadeOutDuration}秒 → イン{fadeInDuration}秒");
        }
        
        return StartCoroutine(FadeOutInCoroutine(fadeOutDuration, fadeInDuration, onFadeOutComplete));
    }

    /// <summary>
    /// フェードコルーチン
    /// </summary>
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null)
        {
            Debug.LogError("[FadeManager] fadeImageが設定されていません");
            yield break;
        }

        Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, startAlpha);
        Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, endAlpha);
        
        fadeImage.color = startColor;
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        fadeImage.color = endColor;
        
        if (enableDebugLog)
        {
            Debug.Log($"[FadeManager] フェード完了: Alpha {startAlpha} → {endAlpha}");
        }
    }

    /// <summary>
    /// フェードアウト→インのコルーチン
    /// </summary>
    private IEnumerator FadeOutInCoroutine(float fadeOutDuration, float fadeInDuration, System.Action onFadeOutComplete)
    {
        // フェードアウト
        yield return StartCoroutine(FadeCoroutine(0f, 1f, fadeOutDuration));
        
        // フェードアウト完了時のコールバック実行
        onFadeOutComplete?.Invoke();
        
        // フェードイン
        yield return StartCoroutine(FadeCoroutine(1f, 0f, fadeInDuration));
        
        if (enableDebugLog)
        {
            Debug.Log("[FadeManager] フェードアウト→イン完了");
        }
    }

    /// <summary>
    /// フェードアウト（画面を暗くする）- コールバック付き
    /// </summary>
    /// <param name="duration">フェード時間（秒）。0の場合はfadeSpeedを使用</param>
    /// <param name="onComplete">フェード完了時のコールバック</param>
    public Coroutine FadeOutWithCallback(float duration, System.Action onComplete)
    {
        if (duration <= 0f) duration = fadeSpeed;
        
        if (enableDebugLog)
        {
            Debug.Log($"[FadeManager] フェードアウト開始（コールバック付き）: {duration}秒");
        }
        
        return StartCoroutine(FadeOutCoroutine(duration, onComplete));
    }

    /// <summary>
    /// フェードイン（画面を明るくする）- コールバック付き
    /// </summary>
    /// <param name="duration">フェード時間（秒）。0の場合はfadeSpeedを使用</param>
    /// <param name="onComplete">フェード完了時のコールバック</param>
    public Coroutine FadeInWithCallback(float duration, System.Action onComplete)
    {
        if (duration <= 0f) duration = fadeSpeed;
        
        if (enableDebugLog)
        {
            Debug.Log($"[FadeManager] フェードイン開始（コールバック付き）: {duration}秒");
        }
        
        return StartCoroutine(FadeInCoroutine(duration, onComplete));
    }

    /// <summary>
    /// フェードアウトコルーチン（コールバック付き）
    /// </summary>
    private IEnumerator FadeOutCoroutine(float duration, System.Action onComplete)
    {
        yield return StartCoroutine(FadeCoroutine(0f, 1f, duration));
        onComplete?.Invoke();
    }

    /// <summary>
    /// フェードインコルーチン（コールバック付き）
    /// </summary>
    private IEnumerator FadeInCoroutine(float duration, System.Action onComplete)
    {
        yield return StartCoroutine(FadeCoroutine(1f, 0f, duration));
        onComplete?.Invoke();
    }

    /// <summary>
    /// 即座にフェード状態を設定
    /// </summary>
    /// <param name="alpha">アルファ値（0-1）</param>
    public void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
        }
    }

    /// <summary>
    /// 現在のフェード状態を取得
    /// </summary>
    /// <returns>現在のアルファ値（0-1）</returns>
    public float GetCurrentFadeAlpha()
    {
        if (fadeImage != null)
        {
            return fadeImage.color.a;
        }
        return 0f;
    }
}


