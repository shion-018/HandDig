using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// オブジェクトの輪郭を描画するエフェクト
/// マイクラの発光バフのような効果を実現
/// Emissionマテリアルを使用して輪郭を表現
/// </summary>
[RequireComponent(typeof(Renderer))]
public class OutlineEffect : MonoBehaviour
{
    [Header("輪郭設定")]
    [Tooltip("輪郭の色")]
    public Color outlineColor = new Color(1f, 0.8f, 0f, 1f);
    
    [Tooltip("輪郭の太さ（Emissionの強度として使用）")]
    [Range(0f, 0.2f)]
    public float outlineWidth = 0.05f;
    
    [Tooltip("壁越しでも見えるようにするか")]
    public bool visibleThroughWalls = true;
    
    [Header("アニメーション設定")]
    [Tooltip("パルスアニメーションを有効にするか")]
    public bool enablePulse = false;
    
    [Tooltip("パルスの速度")]
    public float pulseSpeed = 2f;
    
    [Tooltip("パルスの強度")]
    [Range(0f, 1f)]
    public float pulseIntensity = 0.3f;
    
    private Renderer objectRenderer;
    private Material[] originalMaterials;
    private Material[] outlineMaterials;
    private bool isInitialized = false;
    private bool isEnabled = false;
    
    // Emission用のシェーダープロパティ
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissionMapID = Shader.PropertyToID("_EmissionMap");
    
    private void Awake()
    {
        Initialize();
    }
    
    /// <summary>
    /// 初期化
    /// </summary>
    private void Initialize()
    {
        if (isInitialized) return;
        
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError($"[OutlineEffect] Rendererが見つかりません: {gameObject.name}");
            return;
        }
        
        // 元のマテリアルを保存
        originalMaterials = objectRenderer.materials;
        
        // アウトライン用のマテリアルを作成
        CreateOutlineMaterials();
        
        isInitialized = true;
    }
    
    /// <summary>
    /// アウトライン用のマテリアルを作成
    /// </summary>
    private void CreateOutlineMaterials()
    {
        outlineMaterials = new Material[originalMaterials.Length];
        
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            Material originalMat = originalMaterials[i];
            if (originalMat == null) continue;
            
            // 新しいマテリアルを作成（元のマテリアルをコピー）
            Material outlineMat = new Material(originalMat);
            outlineMat.name = originalMat.name + "_Outline";
            
            // Emissionを有効にして輪郭を表現
            if (outlineMat.HasProperty(EmissionColorID))
            {
                outlineMat.EnableKeyword("_EMISSION");
                // 初期状態ではEmissionを無効化
                outlineMat.SetColor(EmissionColorID, Color.black);
            }
            
            // 深度テストを調整（壁越しでも見えるように）
            if (visibleThroughWalls)
            {
                // 深度テストを無効化する代わりに、レンダリング順序を調整
                outlineMat.renderQueue = 3000; // Transparentより前
            }
            
            outlineMaterials[i] = outlineMat;
        }
    }
    
    private void Update()
    {
        if (!isEnabled || !isInitialized) return;
        
        // パルスアニメーション
        float currentWidth = outlineWidth;
        if (enablePulse)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;
            currentWidth = outlineWidth * pulse;
        }
        
        // Emissionの強度を更新
        UpdateEmissionIntensity(currentWidth);
    }
    
    /// <summary>
    /// Emissionの強度を更新
    /// </summary>
    private void UpdateEmissionIntensity(float width)
    {
        if (outlineMaterials == null) return;
        
        // Emissionの強度を計算（widthに基づいて）
        float intensity = width * 20f; // 強度を調整
        Color emissionColor = outlineColor * intensity;
        
        foreach (Material mat in outlineMaterials)
        {
            if (mat == null) continue;
            
            if (mat.HasProperty(EmissionColorID))
            {
                mat.SetColor(EmissionColorID, emissionColor);
            }
        }
    }
    
    /// <summary>
    /// 輪郭の色を設定
    /// </summary>
    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        if (isEnabled)
        {
            UpdateEmissionIntensity(outlineWidth);
        }
    }
    
    /// <summary>
    /// 輪郭の太さを設定
    /// </summary>
    public void SetOutlineWidth(float width)
    {
        outlineWidth = Mathf.Clamp(width, 0f, 0.2f);
        if (isEnabled)
        {
            UpdateEmissionIntensity(outlineWidth);
        }
    }
    
    private void OnEnable()
    {
        if (!isInitialized)
        {
            Initialize();
        }
        
        if (objectRenderer != null && outlineMaterials != null)
        {
            // アウトライン用のマテリアルを適用
            objectRenderer.materials = outlineMaterials;
            isEnabled = true;
            
            // Emissionを有効化
            UpdateEmissionIntensity(outlineWidth);
        }
    }
    
    private void OnDisable()
    {
        isEnabled = false;
        
        if (objectRenderer != null && originalMaterials != null)
        {
            // 元のマテリアルに戻す
            objectRenderer.materials = originalMaterials;
        }
    }
    
    private void OnDestroy()
    {
        // 作成したマテリアルを削除
        if (outlineMaterials != null)
        {
            foreach (Material mat in outlineMaterials)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // エディタ上で輪郭の範囲を可視化
        if (objectRenderer != null)
        {
            Gizmos.color = outlineColor;
            Bounds bounds = objectRenderer.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size + Vector3.one * outlineWidth * 2f);
        }
    }
}

