using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EntityInventory))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float swapCooldown = 3f;
    public LayerMask targetLayer;

    [Header("Visuals")]
    public SwapZap swapZapPrefab;

    public bool IsSwapReady => Time.time >= lastSwapTime + swapCooldown;

    private Rigidbody2D rb;
    private EntityInventory myInventory;
    private Vector2 movement;
    private float lastSwapTime;
    private bool isKnockedBack;

    [Header("Settings")]
    public float maxSwapRange = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myInventory = GetComponent<EntityInventory>();
        lastSwapTime = -swapCooldown; 
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (Input.GetMouseButtonDown(1)) 
        {
            TrySwap(EntityInventory.SwapType.Weapon);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            TrySwap(EntityInventory.SwapType.Tool);
        }
    }

    private void FixedUpdate()
    {
        if (isKnockedBack) return;

        float speedMult = (myInventory.currentTool != null) ? myInventory.currentTool.moveSpeedMultiplier : 1f;
        rb.linearVelocity = movement.normalized * moveSpeed * speedMult;
    }

    public void ApplyKnockback(Vector2 force)
    {
        isKnockedBack = true;
        rb.AddForce(force, ForceMode2D.Impulse);
        
        CancelInvoke(nameof(StopKnockback));
        Invoke(nameof(StopKnockback), 0.2f);
    }

    private void StopKnockback()
    {
        isKnockedBack = false;
    }

    private void TrySwap(EntityInventory.SwapType type)
    {
        if (!IsSwapReady) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxSwapRange, targetLayer);

        if (hit.collider != null)
        {
            EntityInventory targetInventory = hit.collider.GetComponent<EntityInventory>();
            if (targetInventory != null && targetInventory != myInventory)
            {
                myInventory.SwapItems(targetInventory, type);
                lastSwapTime = Time.time;

                if (swapZapPrefab != null)
                {
                    SwapZap zap = Instantiate(swapZapPrefab, transform.position, Quaternion.identity);
                    zap.Initialize(transform.position, targetInventory.transform.position);
                }
            }
        }
    }
}