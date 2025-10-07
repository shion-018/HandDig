using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldZoneTrigger : MonoBehaviour
{
    [Tooltip("このゾーンで使用するWorld")]
    public MC_World world;

    [Tooltip("ワールド切り替え先のルーター")]
    public CurrentWorldRouter router;

    [Tooltip("プレイヤーを判定するタグ")] 
    public string playerTag = "Player";

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (router != null)
        {
            router.SetCurrentWorld(world, $"Enter {name}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (router != null)
        {
            // 退室時はデフォルトに戻す（必要に応じて別の挙動に変更可）
            router.SetCurrentWorld(router != null ? router.GetComponent<CurrentWorldRouter>().defaultWorld : null, $"Exit {name}");
        }
    }
}






