using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonitorCameraScript : MonoBehaviour
{
    [Header("追従対象（OVRCameraRig または PlayerRoot）")]
    public Transform target;

    [Header("俯瞰カメラ設定")]
    public float distance = 6f;   // プレイヤーからの距離
    public float height = 4f;     // 上方向の高さ
    public float pitchAngle = 45f; // 俯瞰角度

    [Header("追従の滑らかさ")]
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (!target) return;

        // プレイヤーの向き（Y軸）のみ取得
        float yaw = target.eulerAngles.y;

        Quaternion rotation = Quaternion.Euler(pitchAngle, yaw, 0f);
        Vector3 offset = rotation * Vector3.back * distance;
        offset.y += height;

        Vector3 desiredPos = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );

        // プレイヤー中心を見る
        Vector3 lookTarget = target.position;
        lookTarget.y += 1.5f; // 見やすさ調整
        transform.LookAt(lookTarget);
    }
}
