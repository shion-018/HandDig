using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 掘りエフェクトをテストするためのスクリプト
/// </summary>
public class DigEffectTester : MonoBehaviour
{
    [Header("テスト設定")]
    [Tooltip("テスト用の掘り半径")]
    public float testRadius = 2f;
    
    [Tooltip("テスト用のエフェクト色")]
    public Color testEffectColor = Color.red;
    
    [Tooltip("テスト用のエフェクト表示時間")]
    public float testEffectDuration = 2f;
    
    [Header("キー設定")]
    [Tooltip("エフェクト生成キー")]
    public KeyCode spawnEffectKey = KeyCode.E;
    
    [Tooltip("エフェクト色変更キー")]
    public KeyCode changeColorKey = KeyCode.C;
    
    [Tooltip("エフェクト時間変更キー")]
    public KeyCode changeDurationKey = KeyCode.D;

    private Camera playerCamera;
    private int colorIndex = 0;
    private Color[] testColors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        Color.white
    };

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        Debug.Log("[DigEffectTester] 掘りエフェクトテスト開始");
        Debug.Log($"[DigEffectTester] {spawnEffectKey}キー: エフェクト生成");
        Debug.Log($"[DigEffectTester] {changeColorKey}キー: 色変更");
        Debug.Log($"[DigEffectTester] {changeDurationKey}キー: 表示時間変更");
    }

    void Update()
    {
        // エフェクト生成
        if (Input.GetKeyDown(spawnEffectKey))
        {
            SpawnTestEffect();
        }
        
        // 色変更
        if (Input.GetKeyDown(changeColorKey))
        {
            ChangeTestColor();
        }
        
        // 表示時間変更
        if (Input.GetKeyDown(changeDurationKey))
        {
            ChangeTestDuration();
        }
    }

    /// <summary>
    /// テスト用エフェクトを生成
    /// </summary>
    private void SpawnTestEffect()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("[DigEffectTester] カメラが見つかりません");
            return;
        }

        // カメラの前方にエフェクトを生成
        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * 5f;
        
        if (DigEffectManager.Instance != null)
        {
            DigEffectManager.Instance.SetEffectColor(testEffectColor);
            DigEffectManager.Instance.SetEffectDuration(testEffectDuration);
            DigEffectManager.Instance.CreateDigEffect(spawnPosition, testRadius);
            
            Debug.Log($"[DigEffectTester] テストエフェクト生成: 位置={spawnPosition}, 半径={testRadius}, 色={testEffectColor}");
        }
        else
        {
            Debug.LogWarning("[DigEffectTester] DigEffectManagerが見つかりません");
        }
    }

    /// <summary>
    /// テスト用の色を変更
    /// </summary>
    private void ChangeTestColor()
    {
        colorIndex = (colorIndex + 1) % testColors.Length;
        testEffectColor = testColors[colorIndex];
        
        Debug.Log($"[DigEffectTester] テスト色変更: {testEffectColor}");
    }

    /// <summary>
    /// テスト用の表示時間を変更
    /// </summary>
    private void ChangeTestDuration()
    {
        testEffectDuration = Mathf.Clamp(testEffectDuration + 0.5f, 0.5f, 5f);
        
        Debug.Log($"[DigEffectTester] テスト表示時間変更: {testEffectDuration}秒");
    }

    /// <summary>
    /// 指定位置にエフェクトを生成（外部から呼び出し可能）
    /// </summary>
    public void SpawnEffectAtPosition(Vector3 position, float radius)
    {
        if (DigEffectManager.Instance != null)
        {
            DigEffectManager.Instance.SetEffectColor(testEffectColor);
            DigEffectManager.Instance.SetEffectDuration(testEffectDuration);
            DigEffectManager.Instance.CreateDigEffect(position, radius);
            
            Debug.Log($"[DigEffectTester] 指定位置にエフェクト生成: 位置={position}, 半径={radius}");
        }
    }

    /// <summary>
    /// マウス位置にエフェクトを生成
    /// </summary>
    public void SpawnEffectAtMousePosition()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            SpawnEffectAtPosition(hit.point, testRadius);
        }
    }

    void OnGUI()
    {
        // デバッグ情報を表示
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("掘りエフェクトテスト", GUI.skin.box);
        GUILayout.Label($"現在の色: {testEffectColor}");
        GUILayout.Label($"表示時間: {testEffectDuration}秒");
        GUILayout.Label($"掘り半径: {testRadius}");
        GUILayout.Label($"{spawnEffectKey}キー: エフェクト生成");
        GUILayout.Label($"{changeColorKey}キー: 色変更");
        GUILayout.Label($"{changeDurationKey}キー: 時間変更");
        GUILayout.EndArea();
    }
} 