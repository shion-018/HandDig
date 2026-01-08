using UnityEngine;

public class DrillTipProjectile : MonoBehaviour
{
    public System.Action onDestroyed;

    [SerializeField] private LayerMask destructLayers;

    private VoxelDigManager digManager;
    private DrillDigStats stats;
    private int upgradeLevel;
    private LayerMask diggableLayers;
    private float lifetime;
    private DigSoundManager soundManager;
    private int speedLevel;

    private float digTimer = 0f;
    private float moveSpeed;
    private float lifeTimer = 0f;

    public void Initialize(VoxelDigManager digManager, DrillDigStats stats, int level,
        LayerMask diggableLayers, float lifetime, DigSoundManager soundManager, int speedLevel)
    {
        this.digManager = digManager;
        this.stats = stats;
        this.upgradeLevel = level;
        this.diggableLayers = diggableLayers;
        this.lifetime = lifetime;
        this.soundManager = soundManager;
        this.speedLevel = speedLevel;

        float interval = stats.GetDigInterval(speedLevel);
        moveSpeed = Mathf.Clamp(3f / interval, 2f, 10f);
        
        // 発射時の音を再生
        soundManager?.PlayDrillProjectileSound(transform.position);
    }

    private void Update()
    {
        if (stats == null || digManager == null) return;

        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        digTimer += Time.deltaTime;
        float interval = stats.GetDigInterval(speedLevel) * 0.5f;
        if (digTimer >= interval)
        {
            digTimer = 0f;
            TryDig();
        }
    }

    private void TryDig()
    {
        float radius = stats.GetRadius(upgradeLevel);
        Vector3 pos = transform.position;

        bool canDrill = true;
        if (diggableLayers.value != 0)
        {
            canDrill = Physics.CheckSphere(
                pos,
                radius,
                diggableLayers,
                QueryTriggerInteraction.Ignore
            );
        }

        if (canDrill)
        {
            bool dug = digManager.TryDigAt(pos, radius);
            if (dug)
            {
                soundManager?.PlayDrillDigSound(pos);
                DigEffectManager.Instance?.CreateDigEffect(pos, radius);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & destructLayers) != 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        onDestroyed?.Invoke();
    }
}
