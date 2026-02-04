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

            Debug.Log("[ResultColliderScript] プレイヤーがコライダーに当たりました");
            
            // ワープ地点でコライダーに当たった時に、VRDigToolManagerから直接値を取得
            if (VRDigToolManager.Instance != null)
            {
                _getAllJewels = VRDigToolManager.Instance.GetResultTotalTreasureCount();
                _getUniqueJewels = VRDigToolManager.Instance.GetResultSpecialTreasureCount();
                _digPower = VRDigToolManager.Instance.GetResultDigPower();
                
                Debug.Log($"[ResultColliderScript] お宝データを取得: 総数={_getAllJewels}, 特殊={_getUniqueJewels}, 掘る力={_digPower}");
            }
            else
            {
                Debug.LogWarning("[ResultColliderScript] VRDigToolManager.Instanceが見つかりませんでした");
            }

            Debug.Log($"[ResultColliderScript] ScoreScriptに値を設定: {_getAllJewels}, {_getUniqueJewels}, {_digPower}");
            scoreScript.SetValue(0, _getAllJewels);
            scoreScript.SetValue(1, _getUniqueJewels);
            scoreScript.SetValue(2, _digPower);

            scoreScript.Play();
        }
    }
}
