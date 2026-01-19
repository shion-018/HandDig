using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstHierarchyGimmickScript : MonoBehaviour
{
    [Header("衝突を検出する対象オブジェクト")]
    public GameObject key;
    public GameObject door;


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
            Debug.Log("OnCollision反応");
            // Rigidbodyを持っていたら全固定
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
                Destroy(door);
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
