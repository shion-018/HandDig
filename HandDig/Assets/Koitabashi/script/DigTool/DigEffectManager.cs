using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 掘りエフェクトを管理するマネージャークラス
/// </summary>
public class DigEffectManager : MonoBehaviour
{
    [Header("エフェクト設定")]
    [Tooltip("掘りエフェクトのプレハブ")]
    public GameObject digEffectPrefab;
    
    [Tooltip("エフェクトの表示時間（秒）")]
    public float effectDuration = 1.5f;
    
    [Tooltip("エフェクトのスケール（掘り半径に対する倍率）")]
    public float effectScaleMultiplier = 1.0f;
    
    [Tooltip("エフェクトの色")]
    public Color effectColor = Color.yellow;
    
    [Header("パーティクル設定")]
    [Tooltip("パーティクルシステムを使用するか")]
    public bool useParticleSystem = true;
    
    [Tooltip("パーティクルの数")]
    public int particleCount = 20;
    
    [Tooltip("パーティクルの速度")]
    public float particleSpeed = 3f;
    
    [Tooltip("パーティクルのライフタイム")]
    public float particleLifetime = 1.0f;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを出力するか")]
    public bool enableDebugLog = true;

    private static DigEffectManager instance;
    
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static DigEffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DigEffectManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DigEffectManager");
                    instance = go.AddComponent<DigEffectManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 掘りエフェクトを生成
    /// </summary>
    /// <param name="position">掘った位置</param>
    /// <param name="radius">掘り半径</param>
    public void CreateDigEffect(Vector3 position, float radius)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[DigEffectManager] 掘りエフェクト生成: 位置={position}, 半径={radius}");
        }

        try
        {
            if (useParticleSystem)
            {
                CreateParticleEffect(position, radius);
            }
            else if (digEffectPrefab != null)
            {
                CreatePrefabEffect(position, radius);
            }
            else
            {
                CreateSimpleEffect(position, radius);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DigEffectManager] エフェクト生成中にエラーが発生しました: {e.Message}");
            // エラーが発生した場合はシンプルエフェクトにフォールバック
            CreateSimpleEffect(position, radius);
        }
    }

    /// <summary>
    /// パーティクルシステムを使用したエフェクト生成
    /// </summary>
    private void CreateParticleEffect(Vector3 position, float radius)
    {
        GameObject effectObj = new GameObject($"DigEffect_{Time.time}");
        effectObj.transform.position = position;
        
        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = effectDuration;
        main.loop = false;
        main.startLifetime = particleLifetime;
        main.startSpeed = particleSpeed;
        main.startSize = radius * 0.1f;
        main.startColor = effectColor;
        main.maxParticles = particleCount;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, particleCount)
        });
        
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius * 0.5f;
        
        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(particleSpeed);
        
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(effectColor, Color.clear);
        
        // パーティクルレンダラーに適切なマテリアルを設定
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            Material particleMaterial = CreateParticleMaterial();
            renderer.material = particleMaterial;
        }
        
        StartCoroutine(DestroyEffectAfterDelay(effectObj, effectDuration));
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
            Debug.LogWarning("[DigEffectManager] Particles/Standard Unlitシェーダーが見つかりません。Standardシェーダーを使用します。");
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
    /// プレハブを使用したエフェクト生成
    /// </summary>
    private void CreatePrefabEffect(Vector3 position, float radius)
    {
        GameObject effectObj = Instantiate(digEffectPrefab, position, Quaternion.identity);
        effectObj.transform.localScale = Vector3.one * (radius * effectScaleMultiplier);
        
        // エフェクトの色を変更
        Renderer[] renderers = effectObj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (renderer.material != null)
            {
                // マテリアルのコピーを作成して色を変更
                Material newMaterial = new Material(renderer.material);
                newMaterial.color = effectColor;
                renderer.material = newMaterial;
            }
        }
        
        // パーティクルシステムの色も変更
        ParticleSystem[] particleSystems = effectObj.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            main.startColor = effectColor;
            
            // パーティクルレンダラーのマテリアルも修正
            var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null && psRenderer.material != null)
            {
                Material particleMaterial = CreateParticleMaterial();
                psRenderer.material = particleMaterial;
            }
        }
        
        StartCoroutine(DestroyEffectAfterDelay(effectObj, effectDuration));
    }

    /// <summary>
    /// シンプルなエフェクト生成（デフォルト）
    /// </summary>
    private void CreateSimpleEffect(Vector3 position, float radius)
    {
        GameObject effectObj = new GameObject($"DigEffect_{Time.time}");
        effectObj.transform.position = position;
        
        // 球体メッシュを作成
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(effectObj.transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * (radius * effectScaleMultiplier);
        
        // マテリアルを設定
        Renderer renderer = sphere.GetComponent<Renderer>();
        Material material = new Material(Shader.Find("Standard"));
        material.color = effectColor;
        material.SetFloat("_Mode", 3); // Transparent mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        renderer.material = material;
        
        // アニメーション
        StartCoroutine(AnimateSimpleEffect(effectObj, radius));
    }

    /// <summary>
    /// シンプルエフェクトのアニメーション
    /// </summary>
    private IEnumerator AnimateSimpleEffect(GameObject effectObj, float radius)
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * (radius * effectScaleMultiplier * 0.5f);
        Vector3 endScale = Vector3.one * (radius * effectScaleMultiplier * 1.5f);
        
        effectObj.transform.localScale = startScale;
        
        while (elapsed < effectDuration)
        {
            float t = elapsed / effectDuration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // Ease out
            
            effectObj.transform.localScale = Vector3.Lerp(startScale, endScale, easeT);
            
            // 透明度を徐々に下げる
            Renderer renderer = effectObj.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                Color color = renderer.material.color;
                color.a = 1f - t;
                renderer.material.color = color;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(effectObj);
    }

    /// <summary>
    /// 指定時間後にエフェクトを削除
    /// </summary>
    private IEnumerator DestroyEffectAfterDelay(GameObject effectObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effectObj != null)
        {
            Destroy(effectObj);
        }
    }

    /// <summary>
    /// エフェクトの色を設定
    /// </summary>
    public void SetEffectColor(Color color)
    {
        effectColor = color;
    }

    /// <summary>
    /// エフェクトの表示時間を設定
    /// </summary>
    public void SetEffectDuration(float duration)
    {
        effectDuration = duration;
    }

    /// <summary>
    /// パーティクル設定を変更
    /// </summary>
    public void SetParticleSettings(int count, float speed, float lifetime)
    {
        particleCount = count;
        particleSpeed = speed;
        particleLifetime = lifetime;
    }
} 