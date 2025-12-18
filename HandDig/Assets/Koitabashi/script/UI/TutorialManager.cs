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

    [SerializeField] private float requiredLookTime = 0.6f;
    [SerializeField] private float lookThreshold = 0.3f;
    private float lookTimer = 0f;

    [SerializeField] private float requiredJumpTime = 0.5f; // ジャンプ/ジェットパック検知時間
    private float jumpTimer = 0f;
    private bool jumpDetected = false;

    private int compassButtonPressCount = 0; // Yボタンを押した回数


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        toolManager = FindObjectOfType<VRDigToolManager>();

        SetupTutorialUI();
        UpdateText();
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




    public void OnAnyDigSuccess(Vector3 digPos, float radius)
    {
        // SecondDigステップで掘った場合
        if (currentStep == TutorialStep.SecondDig)
        {
            // 二回目の掘りでお宝を生成
            SpawnSecondDigTreasure(digPos, radius);
            // お宝を取るまで待つ（お宝取得時にCompassUpgradeに進む）
        }
    }

    public void OnCompassUpgradeTreasureCollected()
    {
        // SecondDigステップの後、コンパス強化お宝を取った場合
        if (currentStep == TutorialStep.SecondDig || currentStep == TutorialStep.CompassUpgrade)
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
                tutorialText.text = "Aボタンでジャンプ、長押しでジェットパック";
                break;

            case TutorialStep.FirstDig:
                UpdateFirstDigTextByTool();
                break;

            case TutorialStep.GetCompass:
                tutorialText.text = "お宝は 取ると 強化を得られる";
                break;

            case TutorialStep.ToolChange:
                tutorialText.text = "Bボタンで 道具を持ち替えることができる";
                break;

            case TutorialStep.SecondDig:
                UpdateSecondDigTextByTool();
                break;

            case TutorialStep.CompassUpgrade:
                tutorialText.text = "コンパスが強化されたようだ\nYボタンを押す";
                break;

            case TutorialStep.CompassButtonPress:
                tutorialText.text = "コンパスが指すものが変わったようだ\nもう一度押すとまたお宝を指す";
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
            tutorialText.text = "トリガーを押しながら\nピッケルを振りかぶって掘ってみよう";
        }
        else if (tool is DrillDigTool)
        {
            tutorialText.text = "トリガーを押しながら\nドリルを壁に近づける";
        }
    }

    private void UpdateSecondDigTextByTool()
    {
        var tool = toolManager.GetCurrentTool();

        if (tool is PickaxeDigToolMaster)
        {
            tutorialText.text = "トリガーを押しながら\n振りかぶって 振り下ろす";
        }
        else if (tool is DrillDigTool)
        {
            tutorialText.text = "トリガーを押しながら\n振りかぶって 振り下ろす";
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
        // 右スティック
        float rightX = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;

        if (Mathf.Abs(rightX) >= lookThreshold)
        {
            lookTimer += Time.deltaTime;

            if (lookTimer >= requiredLookTime)
            {
                Debug.Log("[Tutorial] Look Completed");
                SetStep(TutorialStep.Jump);
            }
        }
        else
        {
            // 入力が無い場合はリセット
            lookTimer = 0f;
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

        // チュートリアルの進行も処理
        OnGetCompass();
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
}
