using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRDigToolManager : MonoBehaviour
{
    public IDigTool currentTool;
    public Transform toolTransform; // �E��̈ʒu���Q�Ɓi���c�[���̈ʒu�j


    public MonoBehaviour defaultToolObject; // PickaxeDigTool ���A�^�b�`���� GameObject
    public Transform defaultToolTransform;  // �E��̈ʒu

    void Start()
    {
        if (defaultToolObject is IDigTool digTool)
        {
            SetTool(digTool, defaultToolTransform);
            Debug.Log("�����c�[�����Z�b�g������I");
        }
        else
        {
            Debug.LogError("defaultToolObject �� IDigTool �����������I�u�W�F�N�g��ݒ肵�Ă��������I");
        }
    }
    void Update()
    {
        if (currentTool != null && toolTransform != null)
        {
            Debug.Log("VRDigToolManager �� UpdateDig �Ă�ł��");
            currentTool.UpdateDig(toolTransform.position);
        }
        else
        {
            Debug.LogWarning("currentTool �� toolTransform �� null ����I");
        }
        if(currentTool == null)
        {
            Debug.LogWarning("���݂̌@��c�[�����ݒ肳��Ă��܂���BSetTool���\�b�h�Őݒ肵�Ă��������B");
        }
    }
    public void SetTool(IDigTool tool, Transform toolTransform)
    {
        currentTool = tool;
        this.toolTransform = toolTransform;
    }
}