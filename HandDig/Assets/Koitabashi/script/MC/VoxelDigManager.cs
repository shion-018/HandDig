using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDigManager : MonoBehaviour
{
    public MC_World world;         // ワールド全体の参照
    public float digRadius = 2f;   // 掘り半径
    
    [Header("エフェクト設定")]
    [Tooltip("掘りエフェクトを有効にするか")]
    public bool enableDigEffects = true;
    
    [Tooltip("エフェクトの色（ツール別）")]
    public Color drillEffectColor = Color.blue;
    public Color pickaxeEffectColor = Color.yellow;
    public Color handEffectColor = Color.green;

    public void DigAt(Vector3 position)
    {
        DigAt(position, 1.0f); // デフォルトの半径で掘り（1.0fは適当）
    }
    
    public void DigAt(Vector3 position, float radius)
    {
        Debug.Log($"半径{radius}で掘り！");

        if (world != null)
        {
            world.Dig(position, radius); // 実際にチャンクを掘り処理
            
            // エフェクトを生成
            if (enableDigEffects)
            {
                CreateDigEffect(position, radius);
            }
        }
        else
        {
            Debug.LogError("MC_World がアタッチされていません！");
        }
    }
    
    /// <summary>
    /// 掘りエフェクトを生成
    /// </summary>
    private void CreateDigEffect(Vector3 position, float radius)
    {
        if (DigEffectManager.Instance != null)
        {
            // 現在のツールに応じて色を設定
            Color effectColor = GetCurrentToolEffectColor();
            DigEffectManager.Instance.SetEffectColor(effectColor);
            
            // エフェクトを生成
            DigEffectManager.Instance.CreateDigEffect(position, radius);
        }
    }
    
    /// <summary>
    /// 現在のツールに応じたエフェクト色を取得
    /// </summary>
    private Color GetCurrentToolEffectColor()
    {
        // VRDigToolManagerから現在のツールを取得
        VRDigToolManager toolManager = FindObjectOfType<VRDigToolManager>();
        if (toolManager != null && toolManager.GetCurrentTool() != null)
        {
            var currentTool = toolManager.GetCurrentTool();
            
            if (currentTool is DrillDigTool)
            {
                return drillEffectColor;
            }
            else if (currentTool is PickaxeDigTool || currentTool is PickaxeDigToolMaster)
            {
                return pickaxeEffectColor;
            }
            else if (currentTool is VRDigTool)
            {
                return handEffectColor;
            }
        }
        
        // デフォルト色
        return Color.white;
    }
}