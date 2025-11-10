using UnityEngine;

public class DrillTipProjectile : MonoBehaviour
{
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

    public void Initialize(VoxelDigManager digManager, DrillDigStats stats, int level, LayerMask diggableLayers, float lifetime, DigSoundManager soundManager, int speedLevel)
    {
        this.digManager = digManager;
        this.stats = stats;
        this.upgradeLevel = level;
        this.diggableLayers = diggableLayers;
        this.lifetime = lifetime;
        this.soundManager = soundManager;
        this.speedLevel = speedLevel;

        // 掘削間隔が短い（速い）ほど移動速度も速くする
        float interval = stats.GetDigInterval(0);
        moveSpeed = Mathf.Clamp(3f / interval, 2f, 10f); // 最小2、最大10くらいの範囲に調整
    }

    private void Update()
    {
        if (stats == null || digManager == null) return;

        // 一定速度で前進（物理を使わない）
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);

        // 寿命管理
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 掘削タイミング管理
        digTimer += Time.deltaTime;
        // ★ 掘削間隔に軽い補正をかける
        float interval = stats.GetDigInterval(speedLevel) * 0.3f;
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
            canDrill = Physics.CheckSphere(pos, radius * 0.35f, diggableLayers, QueryTriggerInteraction.Ignore);
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

    // 跳ね返り防止：コライダーに当たったら止まる or 消える
    private void OnCollisionEnter(Collision collision)
    {
        // 掘削対象以外にぶつかったら消滅
        if (((1 << collision.gameObject.layer) & diggableLayers) == 0)
        {
            Destroy(gameObject);
        }
    }
}
