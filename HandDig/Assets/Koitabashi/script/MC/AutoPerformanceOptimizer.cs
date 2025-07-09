using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPerformanceOptimizer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnRuntimeMethodLoad()
    {
        // シーンにPerformanceOptimizerが存在しない場合は自動で作成
        if (FindObjectOfType<PerformanceOptimizer>() == null)
        {
            GameObject optimizer = new GameObject("PerformanceOptimizer");
            optimizer.AddComponent<PerformanceOptimizer>();
            DontDestroyOnLoad(optimizer);
            
            Debug.Log("[AutoPerformanceOptimizer] PerformanceOptimizerを自動配置しました");
        }
    }
} 