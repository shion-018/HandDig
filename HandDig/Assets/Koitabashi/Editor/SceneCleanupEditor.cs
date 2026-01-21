#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// エディタモードでシーンが閉じられた際に、DontDestroyOnLoadで保持されているオブジェクトをクリーンアップする
/// </summary>
[InitializeOnLoad]
public class SceneCleanupEditor
{
    static SceneCleanupEditor()
    {
        // シーンが閉じられた際に呼ばれる
        EditorSceneManager.sceneClosed += OnSceneClosed;
    }

    private static void OnSceneClosed(UnityEngine.SceneManagement.Scene scene)
    {
        // エディタモードで、DontDestroyOnLoadで保持されているオブジェクトを検索
        // buildIndex == -1 のオブジェクト = DontDestroyOnLoadで保持されているオブジェクト
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // DontDestroyOnLoadで保持されているオブジェクトを検索
            // シーンに属していないオブジェクト（buildIndex == -1）= DontDestroyOnLoadで保持されているオブジェクト
            if (obj != null && obj.scene.buildIndex == -1)
            {
                // DigSoundManagerやその他のDontDestroyOnLoadオブジェクトを破棄
                if (obj.GetComponent<DigSoundManager>() != null ||
                    obj.GetComponent<SceneLoader>() != null ||
                    obj.GetComponent<TreasureProximitySoundManager>() != null ||
                    obj.GetComponent<TreasureNotificationManager>() != null ||
                    obj.GetComponent<TreasureUIManager>() != null ||
                    obj.GetComponent<GoalUI>() != null ||
                    obj.GetComponent<FadeManager>() != null ||
                    obj.GetComponent<DigEffectManager>() != null ||
                    obj.GetComponent<PerformanceOptimizer>() != null)
                {
                    Debug.Log($"[SceneCleanupEditor] DontDestroyOnLoadオブジェクトをクリーンアップ: {obj.name}");
                    Object.DestroyImmediate(obj);
                }
            }
        }
    }
}
#endif
