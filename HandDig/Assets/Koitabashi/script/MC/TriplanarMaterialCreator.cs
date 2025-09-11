using UnityEngine;

[System.Serializable]
public class TriplanarMaterialSettings
{
    [Header("テクスチャ設定")]
    public Texture2D albedoTexture;
    public Texture2D normalMap;
    
    [Header("トリプラナーマッピング設定")]
    [Range(0.01f, 10f)]
    public float tiling = 1.0f;
    
    [Range(1f, 10f)]
    public float blendSharpness = 2.0f;
    
    [Range(0f, 2f)]
    public float normalStrength = 1.0f;
    
    [Header("マテリアル設定")]
    public Color color = Color.white;
    
    [Range(0f, 1f)]
    public float metallic = 0.0f;
    
    [Range(0f, 1f)]
    public float smoothness = 0.5f;
}

public class TriplanarMaterialCreator : MonoBehaviour
{
    [Header("トリプラナーマッピング設定")]
    public TriplanarMaterialSettings materialSettings = new TriplanarMaterialSettings();
    
    [Header("シェーダー選択")]
    public bool useAdvancedShader = false;
    
    private Material triplanarMaterial;
    
    void Start()
    {
        CreateTriplanarMaterial();
        ApplyMaterial();
    }
    
    /// <summary>
    /// トリプラナーマッピング用のマテリアルを作成
    /// </summary>
    public void CreateTriplanarMaterial()
    {
        // シェーダーを選択
        Shader shader = useAdvancedShader ? 
            Shader.Find("Custom/TriplanarMappingAdvanced") : 
            Shader.Find("Custom/TriplanarMapping");
        
        if (shader == null)
        {
            Debug.LogError("トリプラナーマッピングシェーダーが見つかりません！");
            return;
        }
        
        // マテリアルを作成
        triplanarMaterial = new Material(shader);
        
        // プロパティを設定
        if (materialSettings.albedoTexture != null)
        {
            triplanarMaterial.SetTexture("_MainTex", materialSettings.albedoTexture);
        }
        
        if (useAdvancedShader && materialSettings.normalMap != null)
        {
            triplanarMaterial.SetTexture("_NormalMap", materialSettings.normalMap);
        }
        
        triplanarMaterial.SetFloat("_Tiling", materialSettings.tiling);
        triplanarMaterial.SetFloat("_BlendSharpness", materialSettings.blendSharpness);
        triplanarMaterial.SetColor("_Color", materialSettings.color);
        
        if (useAdvancedShader)
        {
            triplanarMaterial.SetFloat("_NormalStrength", materialSettings.normalStrength);
            triplanarMaterial.SetFloat("_Metallic", materialSettings.metallic);
            triplanarMaterial.SetFloat("_Smoothness", materialSettings.smoothness);
        }
        
        Debug.Log("トリプラナーマッピングマテリアルを作成しました");
    }
    
    /// <summary>
    /// マテリアルを適用
    /// </summary>
    public void ApplyMaterial()
    {
        if (triplanarMaterial == null)
        {
            Debug.LogWarning("マテリアルが作成されていません！");
            return;
        }
        
        // このオブジェクトのRendererにマテリアルを適用
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = triplanarMaterial;
            Debug.Log("トリプラナーマッピングマテリアルを適用しました");
        }
        else
        {
            Debug.LogWarning("Rendererコンポーネントが見つかりません！");
        }
    }
    
    /// <summary>
    /// マテリアル設定を更新
    /// </summary>
    public void UpdateMaterialSettings()
    {
        if (triplanarMaterial == null) return;
        
        if (materialSettings.albedoTexture != null)
        {
            triplanarMaterial.SetTexture("_MainTex", materialSettings.albedoTexture);
        }
        
        if (useAdvancedShader && materialSettings.normalMap != null)
        {
            triplanarMaterial.SetTexture("_NormalMap", materialSettings.normalMap);
        }
        
        triplanarMaterial.SetFloat("_Tiling", materialSettings.tiling);
        triplanarMaterial.SetFloat("_BlendSharpness", materialSettings.blendSharpness);
        triplanarMaterial.SetColor("_Color", materialSettings.color);
        
        if (useAdvancedShader)
        {
            triplanarMaterial.SetFloat("_NormalStrength", materialSettings.normalStrength);
            triplanarMaterial.SetFloat("_Metallic", materialSettings.metallic);
            triplanarMaterial.SetFloat("_Smoothness", materialSettings.smoothness);
        }
    }
    
    void OnValidate()
    {
        // インスペクターで値が変更されたときにマテリアルを更新
        if (Application.isPlaying && triplanarMaterial != null)
        {
            UpdateMaterialSettings();
        }
    }
}
