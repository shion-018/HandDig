using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Koitabashi.Editor
{
    public static class MC_WorldBaker
    {
        public static void SaveAllChunksToAssets(MC_World world)
        {
            if (world == null)
            {
                Debug.LogError("[MC_WorldBaker] world is null");
                return;
            }

            string folder = "Assets/MCPrebaked";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets", "MCPrebaked");
            }

            int total = world.chunkCountX * world.chunkCountY * world.chunkCountZ;
            if (world.prebakedAssets == null) world.prebakedAssets = new List<MC_ChunkDataAsset>(total);
            if (world.prebakedAssets.Count != total)
            {
                world.prebakedAssets.Clear();
                for (int i = 0; i < total; i++) world.prebakedAssets.Add(null);
            }

            for (int x = 0; x < world.chunkCountX; x++)
            for (int y = 0; y < world.chunkCountY; y++)
            for (int z = 0; z < world.chunkCountZ; z++)
            {
                int shiftedY = -y;
                Vector3Int key = new Vector3Int(x, shiftedY, z);
                var chunkMapField = typeof(MC_World).GetField("chunkMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var map = (Dictionary<Vector3Int, MC_Chunk>)chunkMapField.GetValue(world);
                if (!map.TryGetValue(key, out var chunk) || chunk == null || chunk.chunkData == null)
                    continue;

                string assetPath = System.IO.Path.Combine(folder, $"chunk_x{x}_y{y}_z{z}.asset");
                var existing = AssetDatabase.LoadAssetAtPath<MC_ChunkDataAsset>(assetPath);
                if (existing == null)
                {
                    existing = ScriptableObject.CreateInstance<MC_ChunkDataAsset>();
                    AssetDatabase.CreateAsset(existing, assetPath);
                }

                existing.FromRuntimeData(chunk.chunkData);
                EditorUtility.SetDirty(existing);

                int idx = x + world.chunkCountX * (y + world.chunkCountY * z);
                world.prebakedAssets[idx] = existing;

                chunk.prebakedData = existing;
                EditorUtility.SetDirty(chunk);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            world.usePrebakedData = true;
            EditorUtility.SetDirty(world);
            Debug.Log("[MC_WorldBaker] 全チャンクをアセットに保存しました");
        }
    }
}




