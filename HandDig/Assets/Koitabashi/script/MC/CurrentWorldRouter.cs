using UnityEngine;

public class CurrentWorldRouter : MonoBehaviour
{
    [Tooltip("初期のデフォルトWorld（未選択時のフォールバック）")]
    public MC_World defaultWorld;

    public MC_World CurrentWorld { get; private set; }

    public void SetCurrentWorld(MC_World world, string reason = "")
    {
        CurrentWorld = world != null ? world : defaultWorld;
#if UNITY_EDITOR
        if (CurrentWorld != null)
        {
            Debug.Log($"[CurrentWorldRouter] CurrentWorld = {CurrentWorld.name} ({reason})");
        }
        else
        {
            Debug.LogWarning("[CurrentWorldRouter] CurrentWorld が null です");
        }
#endif
    }

    void Awake()
    {
        if (CurrentWorld == null)
        {
            CurrentWorld = defaultWorld;
        }
    }
}



