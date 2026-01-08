using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassScript : MonoBehaviour
{
    [SerializeField] private string currentTag = "Target"; // 現在探索しているタグ
    private List<Transform> targets = new List<Transform>();
    public bool searchCharenge = false;

    void Start()
    {
        RefreshTargetList();
    }

    void Update()
    {
        // Yで "Target" ⇄ "Key" 切り替え
        //デバッグ用としてスペースでも可
        if (OVRInput.GetDown(OVRInput.Button.Four) || Input.GetKeyDown(KeyCode.Space))
        {
            if(searchCharenge == true)
            {
                if (currentTag == "Target")
                {
                    SetSearchTag("Key");
                    GetComponent<Renderer>().material.color = Color.red;
                }
                else
                {
                    SetSearchTag("Target");
                    GetComponent<Renderer>().material.color = Color.blue;
                }
            }
        }

        // 最も近い対象を探して向く
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

    // タグを変更して再探索
    private void SetSearchTag(string newTag)
    {
        if (currentTag != newTag)
        {
            currentTag = newTag;
            RefreshTargetList();
            Debug.Log($"探索タグを「{newTag}」に切り替えました。");
        }
    }

    // タグに基づいてターゲットリストを更新
    private void RefreshTargetList()
    {
        targets.Clear();
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(currentTag);
        foreach (GameObject obj in targetObjects)
        {
            targets.Add(obj.transform);
        }
    }

    // 最も近いオブジェクトを探す
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
