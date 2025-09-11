using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingCubesTriplanarRenderer : MonoBehaviour
{
    [Header("マーチングキューブ設定")]
    public MC_ChunkData chunkData;
    public float surfaceLevel = 0.5f;
    public float uvScale = 0.1f;
    
    [Header("トリプラナーマッピング設定")]
    public TriplanarMaterialSettings materialSettings = new TriplanarMaterialSettings();
    public bool useAdvancedShader = false;
    
    [Header("自動更新設定")]
    public bool autoUpdate = false;
    public float updateInterval = 0.1f;
    
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material triplanarMaterial;
    private float lastUpdateTime;
    
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        
        GenerateMesh();
        CreateTriplanarMaterial();
    }
    
    void Update()
    {
        if (autoUpdate && Time.time - lastUpdateTime > updateInterval)
        {
            GenerateMesh();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// マーチングキューブ法でメッシュを生成
    /// </summary>
    public void GenerateMesh()
    {
        if (chunkData == null)
        {
            Debug.LogWarning("ChunkDataが設定されていません！");
            return;
        }
        
        Mesh mesh = MC_MeshGenerator.GenerateMesh(chunkData, surfaceLevel, uvScale);
        meshFilter.mesh = mesh;
        
        Debug.Log($"マーチングキューブメッシュを生成しました（頂点数: {mesh.vertexCount}）");
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
        
        // マテリアルを適用
        meshRenderer.material = triplanarMaterial;
        
        Debug.Log("トリプラナーマッピングマテリアルを作成・適用しました");
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
    
    /// <summary>
    /// メッシュとマテリアルを再生成
    /// </summary>
    [ContextMenu("Regenerate Mesh and Material")]
    public void RegenerateAll()
    {
        GenerateMesh();
        CreateTriplanarMaterial();
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
