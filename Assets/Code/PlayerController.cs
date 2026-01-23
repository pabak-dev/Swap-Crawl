using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EntityInventory))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float swapCooldown = 3f;
    
    private Rigidbody2D rb;
    private EntityInventory myInventory;
    private Vector2 movement;
    private float lastSwapTime;

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
        float speedMult = (myInventory.currentTool != null) ? myInventory.currentTool.moveSpeedMultiplier : 1f;
        rb.MovePosition(rb.position + moveSpeed * speedMult * Time.fixedDeltaTime * movement.normalized);
    }

    private void TrySwap(EntityInventory.SwapType type)
    {
        if (Time.time < lastSwapTime + swapCooldown) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            EntityInventory targetInventory = hit.collider.GetComponent<EntityInventory>();
            if (targetInventory != null && targetInventory != myInventory)
            {
                myInventory.SwapItems(targetInventory, type);
                lastSwapTime = Time.time;
            }
        }
    }
}