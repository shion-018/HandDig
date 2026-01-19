using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class TextEntry
{
    [TextArea(2, 5)]
    public string text;   // 例: スコア : {0}
    public int value;     // 文章ごとの数値
}
public class ScoreScript : MonoBehaviour
{
    [Header("表示対象のTextMeshPro")]
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("表示データ（Inspector順に再生）")]
    [SerializeField] private TextEntry[] entries;

    [Header("1文字ごとの表示間隔（秒）")]
    [SerializeField] private float charInterval = 0.05f;

    [Header("文章間の待ち時間（秒）")]
    [SerializeField] private float sentenceInterval = 0.5f;

    private bool isPlaying;

    /// <summary>
    /// 外部から数値を更新（index は Inspector の順）
    /// </summary>
    public void SetValue(int index, int value)
    {
        if (entries == null || index < 0 || index >= entries.Length) return;
        entries[index].value = value;
    }

    /// <summary>
    /// 再生開始
    /// </summary>
    public void Play()
    {
        if (isPlaying) return;
        if (targetText == null || entries == null || entries.Length == 0) return;

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        isPlaying = true;

        for (int i = 0; i < entries.Length; i++)
        {
            if (i > 0)
                targetText.text += "\n";

            string formattedText = string.Format(entries[i].text, entries[i].value);
            yield return StartCoroutine(TypeText(formattedText));

            yield return new WaitForSeconds(sentenceInterval);
        }

        isPlaying = false;
    }

    private IEnumerator TypeText(string text)
    {
        foreach (char c in text)
        {
            targetText.text += c;
            yield return new WaitForSeconds(charInterval);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Play();
        }
    }

}
