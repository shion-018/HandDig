using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawner : MonoBehaviour
{
    [Tooltip("�z�u���邨��v���n�u�̈ꗗ�i��ޕʁj")]
    public List<GameObject> treasurePrefabs;

    [Tooltip("�����z�u����m���i0�`1�j")]
    [Range(0f, 1f)]
    public float spawnChance = 0.1f;

    [Tooltip("1�`�����N������̍ő�X�|�[�����s��")]
    public int maxAttemptsPerChunk = 3;

    [Tooltip("1�`�����N�̃T�C�Y�i�O���X�N���v�g����n���Ă�OK�j")]
    public int chunkSize = 32;

    // ���O�`�����N
    private HashSet<Vector3Int> excludedChunks = new HashSet<Vector3Int>();

    public void AddExcludedChunk(Vector3Int chunkCoord)
    {
        excludedChunks.Add(chunkCoord);
    }

    public void TrySpawnTreasureAtChunk(Vector3Int chunkCoord, Vector3 worldPos)
    {
        if (excludedChunks.Contains(chunkCoord)) return;

        for (int i = 0; i < maxAttemptsPerChunk; i++)
        {
            if (Random.value < spawnChance && treasurePrefabs.Count > 0)
            {
                Vector3 localOffset = new Vector3(
                    Random.Range(4f, chunkSize - 4f),
                    Random.Range(4f, chunkSize - 4f),
                    Random.Range(4f, chunkSize - 4f)
                );

                Vector3 spawnPos = worldPos + localOffset;

                GameObject prefab = treasurePrefabs[Random.Range(0, treasurePrefabs.Count)];
                Instantiate(prefab, spawnPos, Quaternion.identity, this.transform);
                Debug.Log($"i{prefab.name}j𐶐:`N {chunkCoord}is {i + 1}j");
            }
        }
    }
    public bool IsExcludedChunk(Vector3Int coord)
    {
        return excludedChunks.Contains(coord);
    }
}