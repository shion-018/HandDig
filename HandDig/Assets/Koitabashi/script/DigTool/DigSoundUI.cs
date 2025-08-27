using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DigSoundUI : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("音量調整用のスライダー")]
    public Slider volumeSlider;
    [Tooltip("音量表示用のテキスト")]
    public Text volumeText;
    [Tooltip("音声ON/OFF用のトグル")]
    public Toggle soundToggle;

    [Header("キー設定")]
    [Tooltip("音量調整を開くキー")]
    public KeyCode toggleUIKey = KeyCode.M;

    private DigSoundManager soundManager;
    private CanvasGroup canvasGroup;
    private bool isUIVisible = false;

    void Start()
    {
        // 音声マネージャーを取得
        soundManager = FindObjectOfType<DigSoundManager>();
        if (soundManager == null)
        {
            Debug.LogWarning("[DigSoundUI] DigSoundManagerが見つかりません");
            return;
        }

        // CanvasGroupを取得または追加
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // UIの初期設定
        SetupUI();
        
        // 初期状態では非表示
        SetUIVisibility(false);
    }

    void Update()
    {
        // キー入力でUIの表示/非表示を切り替え
        if (Input.GetKeyDown(toggleUIKey))
        {
            ToggleUIVisibility();
        }
    }

    void SetupUI()
    {
        // スライダーの設定
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = soundManager.GetVolume();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // トグルの設定
        if (soundToggle != null)
        {
            soundToggle.isOn = soundManager.GetVolume() > 0f;
            soundToggle.onValueChanged.AddListener(OnSoundToggled);
        }

        // テキストの更新
        UpdateVolumeText();
    }

    void OnVolumeChanged(float newVolume)
    {
        if (soundManager != null)
        {
            soundManager.SetVolume(newVolume);
            UpdateVolumeText();
            
            // トグルの状態も更新
            if (soundToggle != null)
            {
                soundToggle.isOn = newVolume > 0f;
            }
        }
    }

    void OnSoundToggled(bool isOn)
    {
        if (soundManager != null)
        {
            float newVolume = isOn ? 0.7f : 0f; // デフォルト音量または0
            soundManager.SetVolume(newVolume);
            
            // スライダーの値も更新
            if (volumeSlider != null)
            {
                volumeSlider.value = newVolume;
            }
            
            UpdateVolumeText();
        }
    }

    void UpdateVolumeText()
    {
        if (volumeText != null && soundManager != null)
        {
            float volume = soundManager.GetVolume();
            volumeText.text = $"音量: {(volume * 100):F0}%";
        }
    }

    void ToggleUIVisibility()
    {
        SetUIVisibility(!isUIVisible);
    }

    void SetUIVisibility(bool visible)
    {
        isUIVisible = visible;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }

    /// <summary>
    /// 音量を設定（外部から呼び出し可能）
    /// </summary>
    /// <param name="volume">音量（0-1）</param>
    public void SetVolume(float volume)
    {
        if (soundManager != null)
        {
            soundManager.SetVolume(volume);
            
            if (volumeSlider != null)
            {
                volumeSlider.value = volume;
            }
            
            if (soundToggle != null)
            {
                soundToggle.isOn = volume > 0f;
            }
            
            UpdateVolumeText();
        }
    }

    /// <summary>
    /// 現在の音量を取得
    /// </summary>
    /// <returns>現在の音量</returns>
    public float GetVolume()
    {
        return soundManager != null ? soundManager.GetVolume() : 0f;
    }
}
