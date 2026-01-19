using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassScript : MonoBehaviour
{
    [Header("探索設定")]
    [SerializeField] private string currentTag = "Target";
    private List<Transform> targets = new List<Transform>();
    public bool searchCharenge = false;

    [Header("マテリアル切り替え設定")]
    [SerializeField] private Renderer targetRenderer;   // ← インスペクターで指定
    [SerializeField] private Material targetMaterial;   // Target 用
    [SerializeField] private Material keyMaterial;      // Key 用

    [Header("サーチ先の目印オブジェクト")]
    [SerializeField] private GameObject star;
    [SerializeField] private GameObject key;

    void Start()
    {
        RefreshTargetList();

        if (targetRenderer == null)
        {
            Debug.LogWarning("CompassScript: targetRenderer が設定されていません。");
        }
    }

    void Update()
    {
        // YかSpaceで"Target""Key"切り替え
        if (OVRInput.GetDown(OVRInput.Button.Four) || Input.GetKeyDown(KeyCode.Space))
        {
            if (searchCharenge)
            {
                if (currentTag == "Target")
                {
                    SetSearchTag("Key");
                    ChangeMaterial(keyMaterial);
                    star.SetActive(false);
                    key.SetActive(true);

                }
                else
                {
                    SetSearchTag("Target");
                    ChangeMaterial(targetMaterial);
                    star.SetActive(true);
                    key.SetActive(false);
                }
            }
        }

        // 最も近い対象を向かせる
        Transform nearestTarget = FindNearestTarget();
        if (nearestTarget != null)
        {
            Vector3 direction = nearestTarget.position - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    private void ChangeMaterial(Material mat)
    {
        if (targetRenderer != null && mat != null)
        {
            targetRenderer.material = mat;
        }
    }

    private void SetSearchTag(string newTag)
    {
        if (currentTag != newTag)
        {
            currentTag = newTag;
            RefreshTargetList();
            Debug.Log($"探索タグを「{newTag}」に切り替えました。");
        }
    }

    private void RefreshTargetList()
    {
        targets.Clear();
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(currentTag);
        foreach (GameObject obj in targetObjects)
        {
            targets.Add(obj.transform);
        }
    }

    private Transform FindNearestTarget()
    {
        Transform nearest = null;
        float minDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (Transform target in targets)
        {
            if (target == null) continue;

            float distanceSqr = (target.position - currentPosition).sqrMagnitude;
            if (distanceSqr < minDistanceSqr)
            {
                minDistanceSqr = distanceSqr;
                nearest = target;
            }
        }

        return nearest;
    }
}
