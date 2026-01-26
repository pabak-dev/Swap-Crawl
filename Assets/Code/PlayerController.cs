using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EntityInventory))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float swapCooldown = 3f;
    public LayerMask targetLayer;

    [Header("Visuals")]
    public SwapZap swapZapPrefab;
    public SwapReticle swapReticleManager;

    public bool IsSwapReady => Time.time >= lastSwapTime + swapCooldown;

    private Rigidbody2D rb;
    private EntityInventory myInventory;
    private BodyVisuals bodyVisuals;
    private Vector2 movement;
    private float lastSwapTime;
    private bool isKnockedBack;

    private EntityInventory currentHoverTarget;

    [Header("Settings")]
    public float maxSwapRange = 10f;

    [Header("Effects")]
    public GameObject feetDustPrefab;
    public Transform feetTransform;
    public float dustSpawnRate = 0.2f;
    private float dustTimer;

    public LineRenderer aimLine;

    public RectTransform swapTimerFill;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myInventory = GetComponent<EntityInventory>();
        bodyVisuals = GetComponentInChildren<BodyVisuals>();
        lastSwapTime = -swapCooldown; 
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.sqrMagnitude > 0.01f)
        {
            dustTimer -= Time.deltaTime;
            if (dustTimer <= 0f)
            {
                GameObject dust = Instantiate(feetDustPrefab, feetTransform.position, Quaternion.identity);
                Destroy(dust, 1f);
                dustTimer = dustSpawnRate;
            }
        }
        else
        {
            dustTimer = 0f;
        }

        bool holdingWeaponSwap = Input.GetMouseButton(1);
        bool holdingToolSwap = Input.GetKey(KeyCode.E);
        
        bool releaseWeapon = Input.GetMouseButtonUp(1);
        bool releaseTool = Input.GetKeyUp(KeyCode.E);

        if (holdingWeaponSwap || holdingToolSwap || releaseWeapon || releaseTool)
        {
            HandleTargeting();
        }
        else
        {
            if (swapReticleManager != null) swapReticleManager.HideAll();
            currentHoverTarget = null;
        }

        if (releaseWeapon)
        {
            ExecuteSwap(EntityInventory.SwapType.Weapon);
        }
        else if (releaseTool)
        {
            ExecuteSwap(EntityInventory.SwapType.Tool);
        }

        swapTimerFill.localScale = new Vector3(Mathf.Clamp01((Time.time - lastSwapTime) / swapCooldown), 1f, 1f);
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
        if (bodyVisuals != null) bodyVisuals.isKnockedBack = true;
        
        rb.AddForce(force, ForceMode2D.Impulse);
        
        CancelInvoke(nameof(StopKnockback));
        Invoke(nameof(StopKnockback), 0.3f);
    }

    private void StopKnockback()
    {
        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
        if (bodyVisuals != null) bodyVisuals.isKnockedBack = false;
    }

    private void HandleTargeting()
    {
        if (Time.timeScale == 0f) return;

        if (aimLine != null)
        {
            aimLine.enabled = true;
            Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosWorld.z = 0f;
            
            Vector3 dir = (mousePosWorld - transform.position).normalized;

            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1, transform.position + dir * maxSwapRange);
        }

        if (swapReticleManager == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, maxSwapRange, targetLayer);
        List<EntityInventory> validTargets = new();

        foreach (var hit in hits)
        {
            EntityInventory inv = hit.GetComponent<EntityInventory>();
            if (inv != null && inv != myInventory)
            {
                validTargets.Add(inv);
            }
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;
        RaycastHit2D aimHit = Physics2D.Raycast(transform.position, direction, maxSwapRange, targetLayer);
        
        currentHoverTarget = null;
        if (aimHit.collider != null)
        {
            EntityInventory inv = aimHit.collider.GetComponent<EntityInventory>();
            if (inv != null && inv != myInventory)
            {
                currentHoverTarget = inv;
            }
        }

        swapReticleManager.ShowReticles(validTargets, currentHoverTarget, IsSwapReady);
    }

    private void ExecuteSwap(EntityInventory.SwapType type)
    {
        if (currentHoverTarget != null && IsSwapReady)
        {
            myInventory.SwapItems(currentHoverTarget, type);
            lastSwapTime = Time.time;

            if (type == EntityInventory.SwapType.Tool)
            {
                HotPotato myPotato = GetComponent<HotPotato>();
                if (myPotato != null) myPotato.Arm();

                HotPotato targetPotato = currentHoverTarget.GetComponent<HotPotato>();
                if (targetPotato != null) targetPotato.Arm();
            }

            if (bodyVisuals != null) bodyVisuals.TriggerSwapFlash();
            if (currentHoverTarget.bodyVisuals != null) currentHoverTarget.bodyVisuals.TriggerSwapFlash();
        }
        
        if (swapReticleManager != null) swapReticleManager.HideAll();
        if (aimLine != null) aimLine.enabled = false;
        currentHoverTarget = null;
    }
}