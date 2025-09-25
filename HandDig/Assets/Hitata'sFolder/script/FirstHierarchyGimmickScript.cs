using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstHierarchyGimmickScript : MonoBehaviour
{
    public GameObject key;
    public GameObject door;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == key)
        {

            // RigidbodyÇéùÇ¡ÇƒÇ¢ÇΩÇÁëSå≈íË
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
                Destroy(door);
            }
        }
    }
}
