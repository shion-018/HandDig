using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstHierarchyGimmickScript : MonoBehaviour
{
    [Header("衝突して反応する対象オブジェクト")]
    public GameObject key;
    public GameObject door;

    [Header("ドアアニメーション設定")]
    public string openAnimationTrigger = "Open";

    private Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == key)
        {
            Debug.Log("OnCollision検出");
            
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
                
                if (door != null)
                {
                    Animator doorAnimator = door.GetComponent<Animator>();
                    if (doorAnimator != null && !string.IsNullOrEmpty(openAnimationTrigger))
                    {
                        doorAnimator.SetTrigger(openAnimationTrigger);
                    }
                    
                    if (DigSoundManager.Instance != null)
                    {
                        DigSoundManager.Instance.PlayGoalDoorOpenSound(door.transform.position);
                    }
                }
            }

            Rigidbody myRb = GetComponent<Rigidbody>();
            if (myRb != null)
            {
                myRb.constraints = RigidbodyConstraints.FreezeAll;
            }

            //collision.transform.SetParent(this.transform);
        }
    }
}
