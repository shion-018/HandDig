using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// パーティクルシステムのマテリアル問題を修正するユーティリティ
/// </summary>
public class ParticleMaterialFixer : MonoBehaviour
{
    [Header("修正設定")]
    [Tooltip("自動的にマテリアルを修正するか")]
    public bool autoFixOnStart = true;
    
    [Tooltip("修正対象のパーティクルシステム")]
    public ParticleSystem targetParticleSystem;
    
    [Tooltip("使用するマテリアル（空の場合は自動生成）")]
    public Material customMaterial;

    void Start()
    {
        if (autoFixOnStart)
        {
            FixParticleMaterials();
        }
    }

    /// <summary>
    /// パーティクルシステムのマテリアルを修正
    /// </summary>
    public void FixParticleMaterials()
    {
        if (targetParticleSystem != null)
        {
            FixSingleParticleSystem(targetParticleSystem);
        }
        else
        {
            // 子オブジェクトのすべてのパーティクルシステムを修正
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                FixSingleParticleSystem(ps);
            }
        }
    }

    /// <summary>
    /// 単一のパーティクルシステムを修正
    /// </summary>
    private void FixSingleParticleSystem(ParticleSystem ps)
    {
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            Material material = customMaterial;
            if (material == null)
            {
                material = CreateParticleMaterial();
            }
            
            renderer.material = material;
            Debug.Log($"[ParticleMaterialFixer] パーティクルシステム '{ps.name}' のマテリアルを修正しました");
        }
    }

    /// <summary>
    /// パーティクル用のマテリアルを作成
    /// </summary>
    private Material CreateParticleMaterial()
    {
        // パーティクル用のシンプルなマテリアルを作成
        Material material = new Material(Shader.Find("Particles/Standard Unlit"));
        
        // シェーダーが見つからない場合はフォールバック
        if (material.shader == null)
        {
            material = new Material(Shader.Find("Standard"));
            Debug.LogWarning("[ParticleMaterialFixer] Particles/Standard Unlitシェーダーが見つかりません。Standardシェーダーを使用します。");
        }
        
        // 透明度設定
        material.SetFloat("_Mode", 3); // Transparent mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        
        return material;
    }

    /// <summary>
    /// プレハブ内のすべてのパーティクルシステムを修正
    /// </summary>
    public static void FixPrefabParticleMaterials(GameObject prefab)
    {
        ParticleSystem[] particleSystems = prefab.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Particles/Standard Unlit"));
                if (material.shader == null)
                {
                    material = new Material(Shader.Find("Standard"));
                }
                
                // 透明度設定
                material.SetFloat("_Mode", 3);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                
                renderer.material = material;
            }
        }
    }

    /// <summary>
    /// エディタでマテリアルを修正（エディタ専用）
    /// </summary>
    [ContextMenu("Fix Particle Materials")]
    public void FixParticleMaterialsEditor()
    {
        FixParticleMaterials();
    }

    /// <summary>
    /// 現在のマテリアル情報をログ出力
    /// </summary>
    [ContextMenu("Log Material Info")]
    public void LogMaterialInfo()
    {
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && renderer.material != null)
            {
                Debug.Log($"[ParticleMaterialFixer] '{ps.name}': シェーダー={renderer.material.shader?.name}, 色={renderer.material.color}");
            }
        }
    }
} 