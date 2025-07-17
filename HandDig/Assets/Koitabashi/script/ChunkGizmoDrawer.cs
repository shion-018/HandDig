using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class ChunkGizmoDrawer : MonoBehaviour
{
    public bool drawChunkGizmos = true;
    public Color gizmoColor = new Color(1f, 1f, 0f, 0.5f); // ���������F
    public Vector3 chunkSize = new Vector3(32f, 32f, 32f);  // �`�����N�̃T�C�Y�ɍ��킹�Đݒ�

    void OnDrawGizmos()
    {
        if (!drawChunkGizmos) return;

        foreach (Transform child in transform)
        {
            var chunk = child.GetComponent<MC_Chunk>();
            if (chunk == null) continue;

            // �F�𕪂���
            if (chunk.isExcluded)
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // ��
            else
                Gizmos.color = gizmoColor;

            Gizmos.DrawWireCube(
                child.position + chunkSize / 2f,
                chunkSize
            );
        }
    }
}