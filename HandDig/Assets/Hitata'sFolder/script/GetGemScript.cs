using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetGemScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision other)
    {
        Destroy(other.gameObject);
        if (other.gameObject.tag == "speed")
        {

        }
        if (other.gameObject.tag == "Power")
        {

        }
        if (other.gameObject.tag == "AoE")
        {

        }
    }
}