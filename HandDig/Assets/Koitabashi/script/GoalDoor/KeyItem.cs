using UnityEngine;

/// <summary>
/// 鍵アイテムのスクリプト
/// プレイヤーが触れると収集され、DoorControllerに通知する
/// </summary>
public class KeyItem : MonoBehaviour
{
    [Header("鍵設定")]
    [SerializeField] private bool canBeCollected = true;
    [SerializeField] private bool destroyOnCollection = true;
    [SerializeField] private float collectionDelay = 0f;
    
    [Header("エフェクト設定")]
    [SerializeField] private GameObject collectionEffect;
    [SerializeField] private AudioClip collectionSound;
    [SerializeField] private float effectDuration = 1f;
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugInfo = true;
    
    private bool isCollected = false;
    private AudioSource audioSource;
    
    // イベント
    public System.Action<KeyItem> OnKeyCollected;
    
    private void Start()
    {
        // AudioSourceを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && collectionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected || !canBeCollected) return;
        
        // プレイヤーかどうかを判定（タグで判定）
        if (other.CompareTag("Player"))
        {
            CollectKey();
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (isCollected || !canBeCollected) return;
        
        // プレイヤーかどうかを判定（タグで判定）
        if (collision.gameObject.CompareTag("Player"))
        {
            CollectKey();
        }
    }
    
    /// <summary>
    /// 鍵を収集する
    /// </summary>
    public void CollectKey()
    {
        if (isCollected || !canBeCollected) return;
        
        isCollected = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"[KeyItem] 鍵が収集されました: {gameObject.name}");
        }
        
        // エフェクトを再生
        PlayCollectionEffect();
        
        // 音を再生
        PlayCollectionSound();
        
        // イベントを発火
        OnKeyCollected?.Invoke(this);
        
        // 指定時間後にオブジェクトを削除
        if (destroyOnCollection)
        {
            if (collectionDelay > 0f)
            {
                Invoke(nameof(DestroyKey), collectionDelay);
            }
            else
            {
                DestroyKey();
            }
        }
    }
    
    /// <summary>
    /// 収集エフェクトを再生
    /// </summary>
    private void PlayCollectionEffect()
    {
        if (collectionEffect != null)
        {
            GameObject effect = Instantiate(collectionEffect, transform.position, transform.rotation);
            
            // エフェクトを指定時間後に削除
            if (effectDuration > 0f)
            {
                Destroy(effect, effectDuration);
            }
        }
    }
    
    /// <summary>
    /// 収集音を再生
    /// </summary>
    private void PlayCollectionSound()
    {
        if (collectionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectionSound);
        }
    }
    
    /// <summary>
    /// 鍵オブジェクトを削除
    /// </summary>
    private void DestroyKey()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[KeyItem] 鍵オブジェクトを削除します: {gameObject.name}");
        }
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 鍵が収集可能かどうかを設定
    /// </summary>
    public void SetCanBeCollected(bool canCollect)
    {
        canBeCollected = canCollect;
    }
    
    /// <summary>
    /// 鍵が既に収集されているかどうか
    /// </summary>
    public bool IsCollected()
    {
        return isCollected;
    }
    
    /// <summary>
    /// 手動で鍵を収集する（テスト用）
    /// </summary>
    [ContextMenu("テスト用：鍵を収集")]
    public void TestCollectKey()
    {
        CollectKey();
    }
    
    private void OnDrawGizmosSelected()
    {
        // コライダーの範囲を可視化
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}
