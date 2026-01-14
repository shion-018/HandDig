using UnityEngine;
using Cysharp.Threading.Tasks; 
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// 背景用のOverlay
    /// </summary>
    [SerializeField]
    private OVROverlay _overlay_Background;

    /// <summary>
    /// LOADING表示用のOverlay
    /// </summary>
    [SerializeField]
    private OVROverlay _overlay_LoadingText;

    /// <summary>
    /// 起動時に自動的に初期化を開始するか
    /// </summary>
    [SerializeField]
    private bool _autoLoadOnStart = true;

    /// <summary>
    /// ローディング表示の遅延時間（ミリ秒）
    /// </summary>
    [SerializeField]
    private int _loadingDelayMillisecond = 3000;

    private static readonly string CENTER_EYE_ANCHOR = "CenterEyeAnchor";

    private static SceneLoader _instance;
    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 起動時に自動的に初期化を開始
        if (_autoLoadOnStart)
        {
            InitializeScene();
        }
    }

    /// <summary>
    /// シーンの初期化を開始（自動呼び出し用）
    /// </summary>
    public async void InitializeScene()
    {
        await ShowOverlayAndInitialize();
    }

    /// <summary>
    /// オーバーレイを表示して、シーン内のオブジェクトを初期化する
    /// </summary>
    private async UniTask ShowOverlayAndInitialize()
    {
        // Overlayを有効化
        ActivateOverlay(true);

        // ローディングテキストの位置を設定
        GameObject centerEyeAnchor = GameObject.Find(CENTER_EYE_ANCHOR);
        if (centerEyeAnchor != null && _overlay_LoadingText != null)
        {
            _overlay_LoadingText.gameObject.transform.position = centerEyeAnchor.transform.position + new Vector3(0f, 0f, 3f);
        }

        // ローディング表示の遅延（フレーム単位で待機してVRカメラが動き続けるようにする）
        int delayFrames = Mathf.CeilToInt(_loadingDelayMillisecond / (1000f / 60f)); // 60FPS想定
        for (int i = 0; i < delayFrames; i++)
        {
            await UniTask.Yield();
        }

        // シーン内のオブジェクトの初期化を待つ
        await WaitForSceneInitialization();

        // MonitorCameraが正しく初期化されるまで待機
        await EnsureMonitorCameraInitialized();

        // さらに少し待機してからOverlayを無効化（フレーム単位で待機）
        for (int i = 0; i < 12; i++) // 約200ms（60FPS想定）
        {
            await UniTask.Yield();
        }

        // Overlayを無効化（GameObjectも非アクティブにする）
        ActivateOverlay(false);
        DeactivateOverlayGameObjects();
        
        // 念のため、もう一度フレーム待機してから最終確認
        await UniTask.Yield();
        DeactivateOverlayGameObjects();
    }

    /// <summary>
    /// シーン内のオブジェクトの初期化を待つ
    /// </summary>
    private async UniTask WaitForSceneInitialization()
    {
        Debug.Log("[SceneLoader] シーン初期化の待機開始");

        // MC_Worldを探す
        MC_World mcWorld = FindObjectOfType<MC_World>();
        
        if (mcWorld != null)
        {
            Debug.Log("[SceneLoader] MC_Worldが見つかりました。初期化を待機します。");
            
            // MC_WorldのStart()が呼ばれてInitializeWorldAsyncが実行されるのを待つ
            // リフレクションを使ってInitializeWorldAsync()のUniTaskを取得して待機
            await WaitForMCWorldInitialization(mcWorld);
        }
        else
        {
            Debug.LogWarning("[SceneLoader] MC_Worldが見つかりません。初期化待機をスキップします。");
            // MC_Worldがない場合でも、少し待機してから進む
            for (int i = 0; i < 30; i++) // 約0.5秒（60FPS想定）
            {
                await UniTask.Yield();
            }
        }

        Debug.Log("[SceneLoader] シーン初期化の待機完了");
    }

    /// <summary>
    /// MC_Worldの初期化が完了するまで待機
    /// </summary>
    private async UniTask WaitForMCWorldInitialization(MC_World mcWorld)
    {
        // MC_WorldのStart()が呼ばれるのを少し待つ
        for (int i = 0; i < 5; i++)
        {
            await UniTask.Yield();
        }

        Debug.Log("[SceneLoader] MC_Worldの初期化完了を待機中...");
        
        // 初期化完了フラグがtrueになるまで待機
        float timeout = 120f; // 最大120秒
        float elapsed = 0f;
        
        while (!mcWorld.IsInitialized && elapsed < timeout)
        {
            await UniTask.Yield();
            elapsed += Time.deltaTime;
        }
        
        if (mcWorld.IsInitialized)
        {
            Debug.Log("[SceneLoader] MC_Worldの初期化が完了しました");
            // さらに少し待機して、完全に初期化が完了するのを待つ
            for (int i = 0; i < 10; i++)
            {
                await UniTask.Yield();
            }
        }
        else
        {
            Debug.LogWarning("[SceneLoader] MC_Worldの初期化がタイムアウトしました");
        }
    }



    /// <summary>
    /// MonitorCameraが正しく初期化されるまで待機
    /// </summary>
    private async UniTask EnsureMonitorCameraInitialized()
    {
        // MonitorCameraを探す
        MonitorCameraScript monitorCamera = null;
        Camera monitorCam = null;
        
        // 数フレーム待機してから探す（オブジェクトが完全に初期化されるまで）
        for (int i = 0; i < 5; i++)
        {
            await UniTask.Yield();
        }

        // MonitorCameraを探す
        monitorCamera = FindObjectOfType<MonitorCameraScript>();
        if (monitorCamera != null)
        {
            monitorCam = monitorCamera.GetComponent<Camera>();
        }

        if (monitorCam != null)
        {
            // カメラの設定を確認・修正
            monitorCam.enabled = true;
            
            // カメラの明るさに関連する設定を確認
            // ClearFlagsが正しく設定されているか確認
            if (monitorCam.clearFlags == CameraClearFlags.SolidColor)
            {
                // 背景色が暗すぎる場合は調整
                if (monitorCam.backgroundColor.r < 0.1f && 
                    monitorCam.backgroundColor.g < 0.1f && 
                    monitorCam.backgroundColor.b < 0.1f)
                {
                    // 背景色を少し明るくする
                    monitorCam.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0f);
                }
            }

            // さらに数フレーム待機して、カメラが完全に初期化されるのを待つ
            for (int i = 0; i < 3; i++)
            {
                await UniTask.Yield();
            }

            // カメラを再度有効化（念のため）
            monitorCam.enabled = false;
            await UniTask.Yield();
            monitorCam.enabled = true;
        }
    }

    private void ActivateOverlay(bool isActive)
    {
        if (_overlay_Background != null)
        {
            _overlay_Background.enabled = isActive;
            if (_overlay_Background.gameObject != null)
            {
                _overlay_Background.gameObject.SetActive(isActive);
            }
        }

        if (_overlay_LoadingText != null)
        {
            _overlay_LoadingText.enabled = isActive;
            if (_overlay_LoadingText.gameObject != null)
            {
                _overlay_LoadingText.gameObject.SetActive(isActive);
            }
        }
    }

    /// <summary>
    /// OverlayのGameObjectを非アクティブにする（確実に無効化するため）
    /// </summary>
    private void DeactivateOverlayGameObjects()
    {
        if (_overlay_Background != null && _overlay_Background.gameObject != null)
        {
            _overlay_Background.enabled = false;
            // hiddenプロパティを設定して完全に非表示にする
            SetOverlayHidden(_overlay_Background, true);
            _overlay_Background.gameObject.SetActive(false);
        }

        if (_overlay_LoadingText != null && _overlay_LoadingText.gameObject != null)
        {
            _overlay_LoadingText.enabled = false;
            // hiddenプロパティを設定して完全に非表示にする
            SetOverlayHidden(_overlay_LoadingText, true);
            _overlay_LoadingText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// OVROverlayのhiddenプロパティを設定する（リフレクション使用）
    /// </summary>
    private void SetOverlayHidden(OVROverlay overlay, bool hidden)
    {
        if (overlay == null) return;

        try
        {
            // OVROverlayのhiddenプロパティをリフレクションで設定
            var hiddenProperty = typeof(OVROverlay).GetProperty("hidden");
            if (hiddenProperty != null)
            {
                hiddenProperty.SetValue(overlay, hidden);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SceneLoader] Overlayのhiddenプロパティ設定に失敗: {e.Message}");
        }
    }
}