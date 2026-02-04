using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstHierarchyGimmickScript : MonoBehaviour
{
    [Header("オブジェクト設定")]
    public GameObject key;
    public GameObject door;

    [Header("ドアアニメーション設定")]
    [Tooltip("ドアのAnimator（自動取得も可能）")]
    public Animator doorAnimator;
    
    [Tooltip("開くアニメーションのトリガー名")]
    public string openAnimationTrigger = "Open";
    
    [Tooltip("開くアニメーションのブールパラメータ名（トリガーの代わりに使用可）")]
    public string isOpenBoolParameter = "IsOpen";

    private Renderer rend;
    private bool isDoorOpen = false;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        
        // ドアにAnimatorがアタッチされている場合は自動取得
        if (door != null && doorAnimator == null)
        {
            doorAnimator = door.GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == key && !isDoorOpen)
        {
            Debug.Log("OnCollision����");
            // Rigidbody�������Ă�����S�Œ�
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
                
                // ドアを開く（Destroyしない）
                OpenDoor();
            }

            Rigidbody myRb = GetComponent<Rigidbody>();
            if (myRb != null)
            {
                myRb.constraints = RigidbodyConstraints.FreezeAll;
            }

            //collision.transform.SetParent(this.transform);
        }
    }

    /// <summary>
    /// ドアを開く
    /// </summary>
    private void OpenDoor()
    {
        if (isDoorOpen || door == null) return;
        
        // ドアを開く音を再生（ドアの位置から）
        if (DigSoundManager.Instance != null)
        {
            Vector3 doorPosition = door.transform.position;
            DigSoundManager.Instance.PlayGoalDoorOpenSound(doorPosition);
        }
        
        // 試練クリア音を再生（ドアの位置から）
        if (DigSoundManager.Instance != null)
        {
            Vector3 doorPosition = door.transform.position;
            DigSoundManager.Instance.PlayTrialRoomClearSound(doorPosition);
        }
        
        // アニメーションでドアを開く
        OpenDoorWithAnimation();
    }

    /// <summary>
    /// アニメーションでドアを開く
    /// </summary>
    private void OpenDoorWithAnimation()
    {
        if (doorAnimator == null)
        {
            Debug.LogWarning("[FirstHierarchyGimmickScript] ドアのAnimatorが設定されていません");
            return;
        }
        
        // ブールパラメータを使用する場合
        if (!string.IsNullOrEmpty(isOpenBoolParameter))
        {
            doorAnimator.SetBool(isOpenBoolParameter, true);
        }
        
        // トリガーを使用する場合
        if (!string.IsNullOrEmpty(openAnimationTrigger))
        {
            doorAnimator.SetTrigger(openAnimationTrigger);
        }
        
        isDoorOpen = true;
        
        Debug.Log("[FirstHierarchyGimmickScript] アニメーションでドアを開きました");
    }
}
