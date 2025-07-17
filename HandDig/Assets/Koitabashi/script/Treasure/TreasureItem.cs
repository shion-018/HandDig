using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureItem : MonoBehaviour
{
    [Tooltip("���̂���ŋ��������c�[���̃C���f�b�N�X�i�����j")]
    public List<int> targetToolIndices = new List<int>(); // �����I��

    [Tooltip("1��ŋ�������i�K��")]
    public int upgradeAmount = 1;
}