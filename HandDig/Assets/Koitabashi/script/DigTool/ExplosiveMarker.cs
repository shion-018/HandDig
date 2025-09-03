using System.Collections;
using UnityEngine;

public class ExplosiveMarker : MonoBehaviour
{
    private VoxelDigManager digManager;
    private float radius;
    private float delaySeconds;

    public void Initialize(VoxelDigManager manager, float explosionRadius, float delay)
    {
        digManager = manager;
        radius = explosionRadius;
        delaySeconds = delay;
        StartCoroutine(ExplodeRoutine());
    }

    private IEnumerator ExplodeRoutine()
    {
        yield return new WaitForSeconds(delaySeconds);

        // 爆発音を再生
        var soundManager = DigSoundManager.Instance;
        if (soundManager != null)
        {
            soundManager.PlayPickaxeExplosionSound(transform.position);
        }

        if (digManager != null)
        {
            digManager.DigAt(transform.position, radius);
        }

        if (DigEffectManager.Instance != null)
        {
            DigEffectManager.Instance.CreateDigEffect(transform.position, radius);
        }

        Destroy(gameObject);
    }
}

