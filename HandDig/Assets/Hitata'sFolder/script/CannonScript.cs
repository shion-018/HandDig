using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonScript : MonoBehaviour
{
    public GameObject CannonBall;
    Vector3 vector = new Vector3(5, 5, 5);
    public float bulletSpeed = 200f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)){
            Debug.Log("pushed enter");
            ShotBall();
        }
    }

    void ShotBall()
    {
        GameObject bullet = Instantiate(CannonBall, transform.position, transform.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.forward * bulletSpeed;
            HalfGravity halfGravity = bullet.AddComponent<HalfGravity>();
            halfGravity.rb = rb;
        }
    }
    public class HalfGravity : MonoBehaviour
    {
        public Rigidbody rb;

        void FixedUpdate()
        {
            if (rb == null) return;

            // Unity‚Ì’Êíd—Í‚ğ”¼•ª‘Å‚¿Á‚·iã•ûŒü‚É”¼•ª‚Ì—Í‚ğ‰Á‚¦‚éj
            rb.AddForce(-0.2f * Physics.gravity, ForceMode.Acceleration);
        }
    }
}
