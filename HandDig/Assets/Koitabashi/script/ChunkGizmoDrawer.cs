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

        Gizmos.color = gizmoColor;

        foreach (Transform child in transform)
        {
            Gizmos.DrawWireCube(
                child.position + chunkSize / 2f,  // ���S���`�����N�̒��S�ɕ␳
                chunkSize
            );
        }
    }
}