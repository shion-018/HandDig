using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureCollector : MonoBehaviour
{
    [Tooltip("����Ƃ��ĔF�������^�O��")]
    public string treasureTag = "Treasure"; // �C���X�y�N�^�[�Őݒ�\

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(treasureTag))
        {
            Debug.Log($"���� [{other.name}] �𔭌��I");

            // ������\���Ɂi�ォ��G�t�F�N�g�Ȃǒǉ��\��j
            other.gameObject.SetActive(false);
        }
    }
}
