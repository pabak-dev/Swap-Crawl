using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SimpleProjectile : MonoBehaviour
{
    public float damage;
    public float knockbackForce;
    public float speed;
    public GameObject owner;
    public LayerMask hitLayers;

    private Rigidbody2D rb;

    public bool isVampiric;
    public AudioClip hitSFX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject == owner) return;
        if (owner != null && collision.transform.IsChildOf(owner.transform)) return;

        if (((1 << collision.gameObject.layer) & hitLayers) == 0) return;

        Health targetHealth = collision.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);

            if (isVampiric && owner != null)
            {
                Health ownerHealth = owner.GetComponent<Health>();
                if (ownerHealth != null) ownerHealth.Heal(5f);
            }
        }

        Rigidbody2D targetRb = collision.GetComponent<Rigidbody2D>();
        if (targetRb != null && knockbackForce > 0.1f) 
        {
            Vector2 dir = rb.linearVelocity.normalized;
            
            PlayerController pc = collision.GetComponent<PlayerController>();
            EnemyAI ai = collision.GetComponent<EnemyAI>();

            if (pc != null) pc.ApplyKnockback(dir * knockbackForce);
            else if (ai != null) ai.ApplyKnockback(dir * knockbackForce);
            else targetRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }

        GlobalSFX.Instance.Play(hitSFX);

        Destroy(gameObject);
    }
}