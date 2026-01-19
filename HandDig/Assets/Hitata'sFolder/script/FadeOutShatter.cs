using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutShatter : MonoBehaviour
{
    [Header("順番にフェードアウトさせる親オブジェクト")]
    [SerializeField]
    private GameObject[] targetRoots;

    [Header("フェードアウト時間（秒）")]
    [SerializeField]
    private float fadeDuration = 1.0f;

    private bool isRunning = false;

    void Update()
    {
        // スペースキー1回で開始
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartFadeSequence();
        }
    }

    public void StartFadeSequence()
    {
        if (isRunning) return;
        if (targetRoots == null || targetRoots.Length == 0) return;

        StartCoroutine(FadeSequenceCoroutine());
    }

    private IEnumerator FadeSequenceCoroutine()
    {
        isRunning = true;

        foreach (GameObject root in targetRoots)
        {
            if (root == null) continue;

            yield return StartCoroutine(FadeOutHierarchy(root));
        }

        isRunning = false;
    }

    private IEnumerator FadeOutHierarchy(GameObject root)
    {
        // 非アクティブ含む Renderer を取得
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

        List<Material> materials = new List<Material>();

        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat != null && mat.HasProperty("_Color"))
                {
                    materials.Add(mat);
                }
            }
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            foreach (Material mat in materials)
            {
                Color color = mat.color;
                color.a = alpha;
                mat.color = color;
            }

            yield return null;
        }

        // 完全透明
        foreach (Material mat in materials)
        {
            Color color = mat.color;
            color.a = 0f;
            mat.color = color;
        }

        root.SetActive(false);
    }
}
