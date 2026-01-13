using UnityEngine;

/// <summary>
/// 試練の部屋のドアが破壊されるときにクリア音を再生するスクリプト
/// このスクリプトをドアのGameObjectにアタッチしてください
/// </summary>
public class TrialRoomDoorSound : MonoBehaviour
{
    private void OnDestroy()
    {
        // ドアが破壊されるときにクリア音を再生
        if (DigSoundManager.Instance != null)
        {
            DigSoundManager.Instance.PlayTrialRoomClearSound(transform.position);
        }
    }
}
