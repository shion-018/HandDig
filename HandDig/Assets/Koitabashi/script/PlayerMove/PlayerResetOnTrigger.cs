using UnityEngine;

public class PlayerResetOnTrigger : MonoBehaviour
{
    [Tooltip("プレイヤーをリセットするSpawnManager（Inspectorでアサイン）")]
    public SpawnManager spawnManager;

    private void OnTriggerEnter(Collider other)
    {
        // "ResetZone"タグが付いたトリガーに触れたら
        if (other.CompareTag("ResetZone"))
        {
            if (spawnManager != null)
            {
                spawnManager.SpawnPlayerAtFinalPosition();
            }
            else
            {
                Debug.LogWarning("[PlayerResetOnTrigger] SpawnManagerがアサインされていません");
            }
        }
    }
} 