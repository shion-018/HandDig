using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public enum TutorialStep
    {
        Move,
        Look,
        Jump,
        FirstDig,
        GetCompass,
        ToolChange,
        SecondDig,
        CompassUpgrade,
        CompassButtonPress,
        Complete
    }

    [Header("UI")]
    public Canvas tutorialCanvas;
    public TextMeshProUGUI tutorialText;

    [Header("コンパス")]
    public GameObject tutorialCompassPrefab;

    [Header("二回目の掘りで出現するお宝")]
    public GameObject secondDigTreasurePrefab;

    [Header("コンパス設定")]
    public GameObject compassObject;
    [Tooltip("CompassAbilityTreasureItemに設定するCompassScript（compassObjectから自動取得も可能）")]
    public CompassScript compassScript;
    private bool compassUnlocked = false;

    [Header("コンパスメッシュ表示設定")]
    [Tooltip("コンパス本体のメッシュ（チュートリアル時にコンパス本体を取った時に表示）")]
    public List<GameObject> compassMainMeshes = new List<GameObject>();
    [Tooltip("コンパス強化パーツのメッシュ（チュートリアル時にコンパス強化お宝を取った時に表示）")]
    public List<GameObject> compassUpgradeMeshes = new List<GameObject>();


    [Header("プレイヤー")]
    [SerializeField] private Transform playerRoot;
    private TutorialStep currentStep = TutorialStep.Move;
    private bool firstDigDone = false;
    private bool compassSpawned = false;
    private bool secondDigTreasureSpawned = false;
    private int firstDigToolIndex = -1; // 最初に掘ったときのツールインデックス

    private VRDigToolManager toolManager;
    [SerializeField] private float requiredMoveTime = 1.0f;
    [SerializeField] private float moveInputThreshold = 0.3f;
    private bool moveInputDetected = false;
    private float moveInputTimer = 0f;

    [SerializeField] private int requiredSnapTurnCount = 3; // スナップターンの必要回数
    [SerializeField] private float snapInputThreshold = 0.8f; // スナップターンの閾値（VRPlayerMovementと同じ）
    private int snapTurnCount = 0; // スナップターンの回数
    private bool canSnapTurn = true; // スナップターン可能かどうか（VRPlayerMovementと同じロジック）

    [SerializeField] private float requiredJumpTime = 0.5f; // ジャンプ/ジェットパック検知時間
    private float jumpTimer = 0f;
    private bool jumpDetected = false;

    private int compassButtonPressCount = 0; // Yボタンを押した回数

    [Header("モード切り替えテキスト表示")]
    private bool showModeSwitchText = false; // モード切り替えテキストを表示するか
    private string modeSwitchTextMessage = ""; // 表示するメッセージ

    [Header("コントローラー設定")]
    [Tooltip("左コントローラーのプレハブ（Inspectorから手動で設定）")]
    public GameObject leftControllerPrefab;
    [Tooltip("右コントローラーのプレハブ（Inspectorから手動で設定）")]
    public GameObject rightControllerPrefab;
    [Tooltip("チュートリアル中にコントローラーを最前面に表示するか")]
    public bool enableControllerFrontmost = true;
    private Renderer[] leftControllerRenderers;
    private Renderer[] rightControllerRenderers;
    private Material[] leftOriginalControllerMaterials;
    private Material[] rightOriginalControllerMaterials;
    private Shader frontmostShader;
    private bool isControllerFrontmost = false;
    private bool leftOriginalControllerActiveState = true; // 左コントローラーの元の表示状態を保存
    private bool rightOriginalControllerActiveState = true; // 右コントローラーの元の表示状態を保存

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        toolManager = FindObjectOfType<VRDigToolManager>();

        SetupTutorialUI();
        UpdateText();
        
        // コントローラーのRendererを取得
        SetupControllerRenderers();
        
        // 最前面表示シェーダーを読み込む
        if (enableControllerFrontmost)
        {
            frontmostShader = Shader.Find("Custom/ControllerFrontmost");
            if (frontmostShader == null)
            {
                Debug.LogWarning("[TutorialManager] ControllerFrontmostシェーダーが見つかりません。");
            }
        }
        
        // チュートリアル開始時にコントローラーを表示し、最前面表示を有効化
        ShowController();

        // 初期状態ではコンパスのメッシュを非表示
        foreach (var mesh in compassMainMeshes)
        {
            if (mesh != null)
            {
                mesh.SetActive(false);
            }
        }
        foreach (var mesh in compassUpgradeMeshes)
        {
            if (mesh != null)
            {
                mesh.SetActive(false);
            }
        }
    }


    private void Update()
    {
        switch (currentStep)
        {
            case TutorialStep.Move:
                CheckMove();
                break;

            case TutorialStep.Look:
                CheckLook();
                break;

            case TutorialStep.Jump:
                CheckJump();
                break;

            case TutorialStep.CompassUpgrade:
            case TutorialStep.CompassButtonPress:
                CheckCompassButton();
                break;
        }

        // モード切り替えテキスト表示中はXボタンの入力を監視
        if (showModeSwitchText)
        {
            CheckModeSwitchButton();
        }
    }

    // ==========================
    // 進行処理
    // ==========================

    private void CheckMove()
    {
        // スティック入力（Meta XR）
        Vector2 move = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // 閾値以上動かしているか
        if (move.magnitude >= moveInputThreshold)
        {
            moveInputTimer += Time.deltaTime;

            if (moveInputTimer >= requiredMoveTime)
            {
                Debug.Log("[Tutorial] Move Completed");
                SetStep(TutorialStep.Look);
            }
        }
        else
        {
            // 入力していない場合はタイマー0にリセット
            moveInputTimer = 0f;
        }
    }




    public void OnFirstDig(Vector3 digPos, float radius)
    {
        if (currentStep != TutorialStep.FirstDig || firstDigDone) return;

        firstDigDone = true;
        firstDigToolIndex = toolManager != null ? toolManager.GetCurrentToolIndex() : -1;

        SpawnCompass(digPos, radius);
        SetStep(TutorialStep.GetCompass);
    }

    public void OnGetCompass()
    {
        if (currentStep != TutorialStep.GetCompass) return;

        SetStep(TutorialStep.ToolChange);
    }

    public void OnToolChanged(IDigTool tool, int toolIndex)
    {
        // ToolChangeステップ: 前回と違うツールに切り替えたらSecondDigに進む
        if (currentStep == TutorialStep.ToolChange)
        {
            if (firstDigToolIndex >= 0 && toolIndex != firstDigToolIndex)
            {
                SetStep(TutorialStep.SecondDig);
            }
            else
            {
                // 同じツールに切り替えた場合はメッセージを更新
                UpdateText();
            }
            return;
        }

        // SecondDigステップ: 前回と同じツールに戻したらToolChangeに戻る
        if (currentStep == TutorialStep.SecondDig)
        {
            if (firstDigToolIndex >= 0 && toolIndex == firstDigToolIndex)
            {
                SetStep(TutorialStep.ToolChange);
            }
            else
            {
                // 別のツールの場合はメッセージを更新
                UpdateText();
            }
            return;
        }

        // FirstDigステップ: ツール変更は無視（掘る動作のみ）
        if (currentStep == TutorialStep.FirstDig)
        {
            UpdateText();
            return;
        }
    }




    [Header("コンパス強化フェーズ")]
    [Tooltip("trueの場合、7の掘り後にコンパス強化フェーズ（8,9）に進む。falseの場合、7の掘りでチュートリアル終了")]
    public bool enableCompassUpgradePhase = false;

    public void OnAnyDigSuccess(Vector3 digPos, float radius)
    {
        // SecondDigステップで掘った場合
        if (currentStep == TutorialStep.SecondDig)
        {
            // 二回目の掘りでお宝を生成
            SpawnSecondDigTreasure(digPos, radius);
            
            // コンパス強化フェーズが有効な場合はお宝を取るまで待つ
            // 無効な場合は直接完了
            if (!enableCompassUpgradePhase)
            {
                SetStep(TutorialStep.Complete);
            }
        }
    }

    public void OnCompassUpgradeTreasureCollected()
    {
        // コンパス強化フェーズが有効で、SecondDigステップの後、コンパス強化お宝を取った場合
        if (enableCompassUpgradePhase && (currentStep == TutorialStep.SecondDig || currentStep == TutorialStep.CompassUpgrade))
        {
            SetStep(TutorialStep.CompassUpgrade);
        }
    }

    // 後方互換性のため、位置情報なしのメソッドも残す
    public void OnAnyDigSuccess()
    {
        // 位置情報が不明な場合は何もしない（SecondDigでは位置情報が必要）
        if (currentStep == TutorialStep.SecondDig)
        {
            Debug.LogWarning("[TutorialManager] OnAnyDigSuccess called without position info in SecondDig step");
        }
    }

    // ==========================
    // 内部処理
    // ==========================

    private void SetStep(TutorialStep next)
    {
        Debug.Log($"[Tutorial] Step Changed: {currentStep} → {next}");
        currentStep = next;
        UpdateText();
    }

    private void UpdateText()
    {
        switch (currentStep)
        {
            case TutorialStep.Move:
                tutorialText.text = "左スティックで移動";
                break;

            case TutorialStep.Look:
                tutorialText.text = "右スティックで視点移動";
                break;

            case TutorialStep.Jump:
                tutorialText.text = "Aボタンでジェットパック起動\n押している間飛ぶことができる";
                break;

            case TutorialStep.FirstDig:
                UpdateFirstDigTextByTool();
                break;

            case TutorialStep.GetCompass:
                tutorialText.text = "コンパスが出てきた\nお宝を指してくれる";
                break;

            case TutorialStep.ToolChange:
                tutorialText.text = "Bボタンで 道具を持ち替えることができる";
                break;

            case TutorialStep.SecondDig:
                UpdateSecondDigTextByTool();
                break;

            case TutorialStep.CompassUpgrade:
                tutorialText.text = "コンパスが強化された\nYボタンで扉の解除キーを指す";
                break;

            case TutorialStep.CompassButtonPress:
                tutorialText.text = "もう一度押すとまたお宝を指す";
                break;

            case TutorialStep.Complete:
                tutorialText.text = "チュートリアル完了、おめでとう！";
                Invoke(nameof(HideUI), 2f);
                break;
        }
    }


    private void UpdateFirstDigTextByTool()
    {
        var tool = toolManager.GetCurrentTool();

        if (tool is PickaxeDigToolMaster)
        {
            tutorialText.text = "掘るにはトリガーを押しながら\n振りかぶって 振り下ろす";
        }
        else if (tool is DrillDigTool)
        {
            tutorialText.text = "掘るにはトリガーを押しながら\nドリルを壁に近づける";
        }
    }

    private void UpdateSecondDigTextByTool()
    {
        var tool = toolManager.GetCurrentTool();

        if (tool is PickaxeDigToolMaster)
        {
            tutorialText.text = "掘るにはトリガーを押しながら\n振りかぶって 振り下ろす";
        }
        else if (tool is DrillDigTool)
        {
            tutorialText.text = "掘るにはトリガーを押しながら\nドリルを壁に近づける";
        }
    }

    private void SpawnCompass(Vector3 digPos, float radius)
    {
        if (compassSpawned) return;
        compassSpawned = true;

        if (tutorialCompassPrefab == null)
        {
            Debug.LogWarning("[TutorialManager] tutorialCompassPrefab is not set");
            return;
        }

        Vector3 spawnPos = digPos;
        spawnPos += toolManager.transform.forward * 0.3f;

        Instantiate(tutorialCompassPrefab, spawnPos, Quaternion.identity);
    }

    private void SpawnSecondDigTreasure(Vector3 digPos, float radius)
    {
        if (secondDigTreasureSpawned) return;
        secondDigTreasureSpawned = true;

        if (secondDigTreasurePrefab == null)
        {
            Debug.LogWarning("[TutorialManager] secondDigTreasurePrefab is not set");
            return;
        }

        Vector3 spawnPos = digPos;
        spawnPos += toolManager.transform.forward * 0.3f;

        GameObject treasureInstance = Instantiate(secondDigTreasurePrefab, spawnPos, Quaternion.identity);
        
        // CompassAbilityTreasureItemコンポーネントがある場合、compassScriptを設定
        CompassAbilityTreasureItem compassTreasure = treasureInstance.GetComponent<CompassAbilityTreasureItem>();
        if (compassTreasure != null)
        {
            // compassScriptが直接設定されていない場合、compassObjectから取得を試みる
            if (compassScript == null && compassObject != null)
            {
                compassScript = compassObject.GetComponent<CompassScript>();
            }
            
            if (compassScript != null)
            {
                compassTreasure.compassScript = compassScript;
                Debug.Log($"[TutorialManager] CompassAbilityTreasureItemにcompassScriptを設定しました");
            }
            else
            {
                Debug.LogWarning("[TutorialManager] compassScriptが設定されていません。CompassAbilityTreasureItemのcompassScriptは空のままです。");
            }
        }
        
        Debug.Log($"[TutorialManager] 二回目の掘りでお宝を生成: {spawnPos}");
    }

    private void SetupTutorialUI()
    {
        if (tutorialCanvas == null)
        {
            GameObject canvasObj = new GameObject("TutorialCanvas");
            tutorialCanvas = canvasObj.AddComponent<Canvas>();
            tutorialCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            tutorialCanvas.sortingOrder = 999;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject textObj = new GameObject("TutorialText");
            textObj.transform.SetParent(canvasObj.transform);
            tutorialText = textObj.AddComponent<TextMeshProUGUI>();

            RectTransform rect = tutorialText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.15f);
            rect.anchorMax = new Vector2(0.5f, 0.15f);
            rect.sizeDelta = new Vector2(900, 120);
            rect.anchoredPosition = Vector2.zero;

            tutorialText.fontSize = 46;
            tutorialText.alignment = TextAlignmentOptions.Center;
        }
    }

    private void HideUI()
    {
        tutorialCanvas.gameObject.SetActive(false);
        
        // チュートリアル終了時にコントローラーを非表示にし、最前面表示を無効化
        HideController();
    }

    private void ShowText(string message)
    {
        if (tutorialText == null) return;
        tutorialText.text = message;
    }

    public void OnPlayerSpawned()
    {
        if (playerRoot == null)
        {
            Debug.LogError("[Tutorial] playerRoot is null on spawn");
            return;
        }

        SetStep(TutorialStep.Move);
    }

    private void CheckLook()
    {
        // 右スティック（VRPlayerMovementと同じスナップターン方式）
        float rightX = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;

        // VRPlayerMovementと同じロジック
        if (canSnapTurn)
        {
            // 右にスナップターン
            if (rightX > snapInputThreshold)
            {
                snapTurnCount++;
                canSnapTurn = false;
                Debug.Log($"[Tutorial] スナップターン（右）: {snapTurnCount}/{requiredSnapTurnCount}回");
            }
            // 左にスナップターン
            else if (rightX < -snapInputThreshold)
            {
                snapTurnCount++;
                canSnapTurn = false;
                Debug.Log($"[Tutorial] スナップターン（左）: {snapTurnCount}/{requiredSnapTurnCount}回");
            }
        }
        
        // スティックが戻ったら次のスナップターンを許可（VRPlayerMovementと同じ）
        if (Mathf.Abs(rightX) < 0.2f)
        {
            canSnapTurn = true;
        }

        // 必要な回数に達したら次へ
        if (snapTurnCount >= requiredSnapTurnCount)
        {
            Debug.Log("[Tutorial] Look Completed");
            SetStep(TutorialStep.Jump);
            snapTurnCount = 0;
            canSnapTurn = true;
        }
    }

    private void CheckJump()
    {
        // Aボタン（右コントローラーのButton.One）
        bool isAButtonHeld = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);
        bool isAButtonDown = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);

        // ジャンプ（短押し）またはジェットパック（長押し）を検知
        if (isAButtonDown || isAButtonHeld)
        {
            if (isAButtonDown)
            {
                jumpDetected = true;
                jumpTimer = 0f;
            }

            if (isAButtonHeld)
            {
                jumpTimer += Time.deltaTime;

                // 短押し（ジャンプ）または長押し（ジェットパック）を検知
                if (jumpTimer >= requiredJumpTime)
                {
                    Debug.Log("[Tutorial] Jump/Jetpack Completed");
                    SetStep(TutorialStep.FirstDig);
                    jumpDetected = false;
                    jumpTimer = 0f;
                }
            }
        }
        else if (jumpDetected && jumpTimer > 0.1f)
        {
            // 短押しでジャンプした場合も検知（少し待ってから判定）
            Debug.Log("[Tutorial] Jump Completed (short press)");
            SetStep(TutorialStep.FirstDig);
            jumpDetected = false;
            jumpTimer = 0f;
        }
    }

    public void UnlockCompass()
    {
        if (compassUnlocked) return;

        compassUnlocked = true;
        if (compassObject != null)
            compassObject.SetActive(true);

        Debug.Log("[TutorialManager] コンパスが解放されました！");

        // コンパス本体のメッシュを表示
        foreach (var mesh in compassMainMeshes)
        {
            if (mesh != null)
            {
                mesh.SetActive(true);
            }
        }
        if (compassMainMeshes.Count > 0)
        {
            Debug.Log($"[TutorialManager] コンパス本体のメッシュを{compassMainMeshes.Count}個表示しました");
        }

        // チュートリアルの進行も処理
        OnGetCompass();
    }

    /// <summary>
    /// コンパス強化パーツのメッシュを表示する（チュートリアル時にコンパス強化お宝を取った時に呼ばれる）
    /// </summary>
    public void ShowCompassUpgradeMesh()
    {
        foreach (var mesh in compassUpgradeMeshes)
        {
            if (mesh != null)
            {
                mesh.SetActive(true);
            }
        }
        if (compassUpgradeMeshes.Count > 0)
        {
            Debug.Log($"[TutorialManager] コンパス強化パーツのメッシュを{compassUpgradeMeshes.Count}個表示しました");
        }
    }

    private void CheckCompassButton()
    {
        // Yボタン（左コントローラーのButton.Two）を検知
        // CompassScriptと同じようにButton.Fourも試す（後方互換性のため）
        bool yButtonPressed = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch) || 
                              OVRInput.GetDown(OVRInput.Button.Four) || 
                              Input.GetKeyDown(KeyCode.Space);

        if (yButtonPressed)
        {
            compassButtonPressCount++;
            Debug.Log($"[Tutorial] Yボタンが押されました: {compassButtonPressCount}回目 (現在のステップ: {currentStep})");

            // CompassUpgradeステップ: 1回目でCompassButtonPressに進む
            if (currentStep == TutorialStep.CompassUpgrade)
            {
                SetStep(TutorialStep.CompassButtonPress);
            }
            // CompassButtonPressステップ: 2回目で完了
            else if (currentStep == TutorialStep.CompassButtonPress)
            {
                SetStep(TutorialStep.Complete);
                compassButtonPressCount = 0;
            }
        }
    }

    /// <summary>
    /// Xボタンが押されたかチェック（モード切り替えテキストを消すため）
    /// </summary>
    private void CheckModeSwitchButton()
    {
        // Xボタン（Button.ThreeまたはRawButton.X）を検知
        bool xButtonPressed = OVRInput.GetDown(OVRInput.Button.Three) || 
                              OVRInput.GetDown(OVRInput.RawButton.X) || 
                              Input.GetKeyDown(KeyCode.X);

        if (xButtonPressed)
        {
            HideModeSwitchText();
        }
    }

    /// <summary>
    /// モード切り替えテキストを表示（ピッケル爆発モードまたはドリル射出モード取得時）
    /// </summary>
    public void ShowModeSwitchText(string message)
    {
        showModeSwitchText = true;
        modeSwitchTextMessage = message;
        
        // チュートリアルUIを表示（非表示の場合は表示）
        if (tutorialCanvas != null && !tutorialCanvas.gameObject.activeSelf)
        {
            tutorialCanvas.gameObject.SetActive(true);
        }
        
        // テキストを更新
        if (tutorialText != null)
        {
            tutorialText.text = message;
        }
        
        Debug.Log($"[TutorialManager] モード切り替えテキストを表示: {message}");
    }

    /// <summary>
    /// モード切り替えテキストを非表示
    /// </summary>
    public void HideModeSwitchText()
    {
        if (!showModeSwitchText) return;
        
        showModeSwitchText = false;
        modeSwitchTextMessage = "";
        
        // チュートリアルが完了していない場合は、現在のステップのテキストに戻す
        if (currentStep != TutorialStep.Complete)
        {
            UpdateText();
        }
        else
        {
            // チュートリアル完了済みの場合はUIを非表示
            if (tutorialCanvas != null)
            {
                tutorialCanvas.gameObject.SetActive(false);
            }
        }
        
        Debug.Log("[TutorialManager] モード切り替えテキストを非表示にしました");
    }

    /// <summary>
    /// Inspectorで設定されたコントローラーのRendererを取得
    /// </summary>
    private void SetupControllerRenderers()
    {
        // 左コントローラー
        if (leftControllerPrefab != null)
        {
            leftOriginalControllerActiveState = leftControllerPrefab.activeSelf;
            leftControllerRenderers = leftControllerPrefab.GetComponentsInChildren<Renderer>(true);
            
            if (leftControllerRenderers != null && leftControllerRenderers.Length > 0)
            {
                leftOriginalControllerMaterials = new Material[leftControllerRenderers.Length];
                for (int i = 0; i < leftControllerRenderers.Length; i++)
                {
                    if (leftControllerRenderers[i] != null && leftControllerRenderers[i].sharedMaterial != null)
                    {
                        leftOriginalControllerMaterials[i] = leftControllerRenderers[i].sharedMaterial;
                    }
                }
                Debug.Log($"[TutorialManager] 左コントローラーのRendererを{leftControllerRenderers.Length}個見つけました");
            }
        }
        else
        {
            Debug.LogWarning("[TutorialManager] leftControllerPrefabがInspectorで設定されていません。");
        }

        // 右コントローラー
        if (rightControllerPrefab != null)
        {
            rightOriginalControllerActiveState = rightControllerPrefab.activeSelf;
            rightControllerRenderers = rightControllerPrefab.GetComponentsInChildren<Renderer>(true);
            
            if (rightControllerRenderers != null && rightControllerRenderers.Length > 0)
            {
                rightOriginalControllerMaterials = new Material[rightControllerRenderers.Length];
                for (int i = 0; i < rightControllerRenderers.Length; i++)
                {
                    if (rightControllerRenderers[i] != null && rightControllerRenderers[i].sharedMaterial != null)
                    {
                        rightOriginalControllerMaterials[i] = rightControllerRenderers[i].sharedMaterial;
                    }
                }
                Debug.Log($"[TutorialManager] 右コントローラーのRendererを{rightControllerRenderers.Length}個見つけました");
            }
        }
        else
        {
            Debug.LogWarning("[TutorialManager] rightControllerPrefabがInspectorで設定されていません。");
        }
    }

    /// <summary>
    /// チュートリアル開始時にコントローラーを表示
    /// </summary>
    private void ShowController()
    {
        // 左コントローラーを表示
        if (leftControllerPrefab != null)
        {
            leftControllerPrefab.SetActive(true);
            Debug.Log("[TutorialManager] 左コントローラーを表示しました");
        }

        // 右コントローラーを表示
        if (rightControllerPrefab != null)
        {
            rightControllerPrefab.SetActive(true);
            Debug.Log("[TutorialManager] 右コントローラーを表示しました");
        }

        // 最前面表示を有効化
        if (enableControllerFrontmost && frontmostShader != null)
        {
            SetControllerFrontmost(true);
        }
    }

    /// <summary>
    /// チュートリアル終了時にコントローラーを非表示
    /// </summary>
    private void HideController()
    {
        // 最前面表示を無効化
        if (isControllerFrontmost)
        {
            SetControllerFrontmost(false);
        }

        // 左コントローラーを非表示
        if (leftControllerPrefab != null)
        {
            leftControllerPrefab.SetActive(false);
            Debug.Log("[TutorialManager] 左コントローラーを非表示にしました");
        }

        // 右コントローラーを非表示
        if (rightControllerPrefab != null)
        {
            rightControllerPrefab.SetActive(false);
            Debug.Log("[TutorialManager] 右コントローラーを非表示にしました");
        }
    }

    /// <summary>
    /// コントローラーを最前面表示にする/戻す
    /// </summary>
    /// <param name="enable">trueで最前面表示、falseで通常表示に戻す</param>
    private void SetControllerFrontmost(bool enable)
    {
        if (frontmostShader == null && enable)
        {
            Debug.LogWarning("[TutorialManager] 最前面表示シェーダーが読み込まれていません。");
            return;
        }

        isControllerFrontmost = enable;

        // 左コントローラーの処理
        SetControllerFrontmostForRenderers(leftControllerRenderers, leftOriginalControllerMaterials, enable, "左");

        // 右コントローラーの処理
        SetControllerFrontmostForRenderers(rightControllerRenderers, rightOriginalControllerMaterials, enable, "右");
    }

    /// <summary>
    /// 指定されたRenderer配列に対して最前面表示を適用/解除
    /// </summary>
    private void SetControllerFrontmostForRenderers(Renderer[] renderers, Material[] originalMaterials, bool enable, string sideName)
    {
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            if (enable)
            {
                // 最前面表示シェーダーに切り替え
                Material originalMat = originalMaterials[i];
                if (originalMat != null)
                {
                    // 新しいマテリアルを作成（元のマテリアルのプロパティをコピー）
                    Material frontmostMat = new Material(frontmostShader);
                    
                    // テクスチャとプロパティをコピー
                    if (originalMat.HasProperty("_MainTex"))
                        frontmostMat.SetTexture("_MainTex", originalMat.GetTexture("_MainTex"));
                    if (originalMat.HasProperty("_Color"))
                        frontmostMat.SetColor("_Color", originalMat.GetColor("_Color"));
                    if (originalMat.HasProperty("_BumpMap") && frontmostMat.HasProperty("_BumpMap"))
                        frontmostMat.SetTexture("_BumpMap", originalMat.GetTexture("_BumpMap"));
                    if (originalMat.HasProperty("_BumpScale") && frontmostMat.HasProperty("_BumpScale"))
                        frontmostMat.SetFloat("_BumpScale", originalMat.GetFloat("_BumpScale"));
                    if (originalMat.HasProperty("_Glossiness") && frontmostMat.HasProperty("_Glossiness"))
                        frontmostMat.SetFloat("_Glossiness", originalMat.GetFloat("_Glossiness"));
                    if (originalMat.HasProperty("_Metallic") && frontmostMat.HasProperty("_Metallic"))
                        frontmostMat.SetFloat("_Metallic", originalMat.GetFloat("_Metallic"));
                    if (originalMat.HasProperty("_EmissionMap") && frontmostMat.HasProperty("_EmissionMap"))
                        frontmostMat.SetTexture("_EmissionMap", originalMat.GetTexture("_EmissionMap"));
                    if (originalMat.HasProperty("_EmissionColor") && frontmostMat.HasProperty("_EmissionColor"))
                        frontmostMat.SetColor("_EmissionColor", originalMat.GetColor("_EmissionColor"));
                    
                    renderers[i].sharedMaterial = frontmostMat;
                }
            }
            else
            {
                // 元のマテリアルに戻す
                if (originalMaterials[i] != null)
                {
                    renderers[i].sharedMaterial = originalMaterials[i];
                }
            }
        }

        Debug.Log($"[TutorialManager] {sideName}コントローラーの最前面表示を{(enable ? "有効" : "無効")}にしました");
    }
}
