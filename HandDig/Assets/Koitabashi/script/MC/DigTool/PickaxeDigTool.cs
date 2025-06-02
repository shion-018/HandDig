using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeDigTool : MonoBehaviour, IDigTool
{
    public VoxelDigManager digManager;
    public float baseRadius = 3.0f;
    public float stage2Radius = 4.0f;
    public float stage3Radius = 5.0f;

    public float minComboTime = 0.5f;
    public float maxComboTime = 1.5f;

    private Collider currentCollider;
    private float lastDigTime = -10f;
    private int comboStage = 0;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
        {
            currentCollider = other;
            Debug.Log("�n�`�ɐG�ꂽ: " + other.name);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (currentCollider == other)
            currentCollider = null;
    }

    public void UpdateDig(Vector3 toolPosition)
    {
        //Debug.Log("UpdateDig() �Ă΂ꂽ��");
        if (currentCollider == null)
        {
            //Debug.Log("�n�`�ɐG��Ă��Ȃ�");
            return;
        }
        bool triggerPressed = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) || Input.GetKeyDown(KeyCode.Space);
        if (triggerPressed)
        {

            //Debug.Log("�g���K�[�����ꂽ");

            float currentTime = Time.time;
            float timeSinceLast = currentTime - lastDigTime;

            if (timeSinceLast >= minComboTime && timeSinceLast <= maxComboTime)
            {
                comboStage = (comboStage + 1) % 3; // 0 �� 1 �� 2 �� 0�c
            }
            else
            {
                comboStage = 0;
            }

            lastDigTime = currentTime;

            float radius = comboStage switch
            {
                1 => stage2Radius,
                2 => stage3Radius,
                _ => baseRadius
            };
            Debug.Log($"��͂��i�K {comboStage + 1}�i���a{radius}�j�Ō@��I");

            digManager.DigAt(toolPosition, radius);
        }
    }
} 
