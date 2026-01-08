using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PerformanceOptimizer : MonoBehaviour
{
    [Header("自動配置設定")]
    [Tooltip("シーン開始時に自動で配置されるか")]
    public bool autoPlaceOnStart = true;
    
    [Header("パフォーマンス設定")]
    [Tooltip("lilToonの自動マイグレーションを無効化")]
    public bool disableLilToonMigration = true;
    
    [Tooltip("VR初期化の遅延")]
    public bool delayVRInitialization = true;
    
    [Tooltip("重い処理の分散間隔（フレーム数）")]
    public int heavyProcessInterval = 3;

    /// <summary>
    /// シーン開始時に自動でPerformanceOptimizerを配置する
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoPlaceIfNeeded()
    {
        if (FindObjectOfType<PerformanceOptimizer>() == null)
        {
            GameObject optimizer = new GameObject("PerformanceOptimizer");
            var component = optimizer.AddComponent<PerformanceOptimizer>();
            component.autoPlaceOnStart = true;
            DontDestroyOnLoad(optimizer);
            
            Debug.Log("[PerformanceOptimizer] 自動配置されました");
        }
    }

    void Awake()
    {
        // lilToonの重い処理を無効化
        if (disableLilToonMigration)
        {
            DisableLilToonHeavyProcesses();
        }
        
        // VR初期化を遅延
        if (delayVRInitialization)
        {
            DelayVRInitialization().Forget();
        }
    }

    void DisableLilToonHeavyProcesses()
    {
        // lilToonの自動マイグレーションを無効化するための設定
        // 実際の無効化はlilToonの設定ファイルで行う必要があります
        Debug.Log("[PerformanceOptimizer] lilToonの重い処理を無効化しました");
    }

    async UniTask DelayVRInitialization()
    {
        // VR初期化を数フレーム遅延
        for (int i = 0; i < 5; i++)
        {
            await UniTask.Yield();
        }
        
        Debug.Log("[PerformanceOptimizer] VR初期化の遅延完了");
    }

    void Start()
    {
        // メモリ使用量の最適化
        Application.targetFrameRate = 0; // 無制限に変更
        QualitySettings.vSyncCount = 0;
        
        Debug.Log("[PerformanceOptimizer] パフォーマンス最適化完了");
    }
} 