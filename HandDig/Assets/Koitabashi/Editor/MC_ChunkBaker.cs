using UnityEngine;
using UnityEditor;

public static class MC_ChunkBaker
{
    [MenuItem("MC/Bake Selected Chunk To Asset")]
    public static void BakeSelectedChunk()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("MC Baker", "選択中のGameObjectがありません", "OK");
            return;
        }

        var chunk = go.GetComponent<MC_Chunk>();
        if (chunk == null || chunk.chunkData == null)
        {
            EditorUtility.DisplayDialog("MC Baker", "選択中のオブジェクトにMC_Chunkが見つからないか、chunkDataがありません", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Chunk Data Asset",
            go.name + "_ChunkData",
            "asset",
            "保存先を選択してください");

        if (string.IsNullOrEmpty(path)) return;

        var asset = ScriptableObject.CreateInstance<MC_ChunkDataAsset>();
        asset.FromRuntimeData(chunk.chunkData);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 自動的にチャンクへ割り当て
        chunk.prebakedData = asset;
        EditorUtility.SetDirty(chunk);

        EditorUtility.DisplayDialog("MC Baker", "ベイク完了し、チャンクへ割り当てました", "OK");
    }
}



