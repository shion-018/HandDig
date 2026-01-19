using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultColliderScript : MonoBehaviour
{
    [SerializeField] private ScoreScript scoreScript;
    int _getAllJewels;
    int _getUniqueJewels;
    int _digPower;
    bool result = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && result == false)
        {
            result = true;
            scoreScript.SetValue(0, _getAllJewels);
            scoreScript.SetValue(1, _getUniqueJewels);
            scoreScript.SetValue(2, _digPower);

            scoreScript.Play();
        }
    }
}
