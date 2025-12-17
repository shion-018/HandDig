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
        ToolChange,
        AnyDig,
        GetCompass,
        Complete
    }

    [Header("UI")]
    public Canvas tutorialCanvas;
    public TextMeshProUGUI tutorialText;

    [Header("コンパス")]
    public GameObject tutorialCompassPrefab;

    public GameObject compassObject;
    private bool compassUnlocked = false;


    [Header("プレイヤー")]
    [SerializeField] private Transform playerRoot;
    private TutorialStep currentStep = TutorialStep.Move;
    private bool firstDigDone = false;
    private bool compassSpawned = false;

    private VRDigToolManager toolManager;
    [SerializeField] private float requiredMoveTime = 1.0f;
    [SerializeField] private float moveInputThreshold = 0.3f;
    private bool moveInputDetected = false;
    private float moveInputTimer = 0f;

    [SerializeField] private float requiredLookTime = 0.6f;
    [SerializeField] private float lookThreshold = 0.3f;
    private float lookTimer = 0f;


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
        if (currentStep != TutorialStep.AnyDig || firstDigDone) return;

        firstDigDone = true;

        SpawnCompass(digPos, radius);
        SetStep(TutorialStep.GetCompass);
    }

    public void OnGetCompass()
    {
        if (currentStep != TutorialStep.GetCompass) return;

        SetStep(TutorialStep.Complete);
    }

    public void OnToolChanged(IDigTool tool, int toolIndex)
    {
        
        if (currentStep != TutorialStep.AnyDig &&
            currentStep != TutorialStep.ToolChange)
            return;

        // ToolChangeステップの時は、ツール変更を検知したらAnyDigステップに進む
        if (currentStep == TutorialStep.ToolChange)
        {
            SetStep(TutorialStep.AnyDig);
        }

        if (tool is PickaxeDigToolMaster)
        {
            ShowText("ピッケルを振りかぶって掘ってみよう！");
        }
        else if (tool is DrillDigTool)
        {
            ShowText("ピッケルを振りかぶって\nドリルを近づけよう！");
        }
    }




    public void OnAnyDigSuccess()
    {
        if (currentStep == TutorialStep.AnyDig)
        {
            SetStep(TutorialStep.Complete);
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
                tutorialText.text = "スティックで移動しよう";
                break;

            case TutorialStep.Look:
                tutorialText.text = "右スティックで視点を回転させてみよう";
                break;

            case TutorialStep.ToolChange:
                tutorialText.text = "Bボタンで道具を切り替えよう";
                break;

            case TutorialStep.AnyDig:
                UpdateDigTextByTool();
                break;

            case TutorialStep.GetCompass:
                tutorialText.text = "出てきたコンパスを拾おう";
                break;

            case TutorialStep.Complete:
                tutorialText.text = "チュートリアル完了、おめでとう！";
                Invoke(nameof(HideUI), 2f);
                break;
        }
    }


    private void UpdateDigTextByTool()
    {
        var tool = toolManager.GetCurrentTool();

        if (tool is PickaxeDigToolMaster)
        {
            tutorialText.text = "トリガーを押しながら\nピッケルを振りかぶって掘ってみよう";
        }
        else
        {
            tutorialText.text = "トリガーを押しながら\nドリルを近づけよう";
        }
    }

    private void SpawnCompass(Vector3 digPos, float radius)
    {
        if (compassSpawned) return;
        compassSpawned = true;

        Vector3 spawnPos = digPos;
        spawnPos += toolManager.transform.forward * 0.3f;

        Instantiate(tutorialCompassPrefab, spawnPos, Quaternion.identity);
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
                SetStep(TutorialStep.ToolChange);
            }
        }
        else
        {
            // 入力が無い場合はリセット
            lookTimer = 0f;
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
}
