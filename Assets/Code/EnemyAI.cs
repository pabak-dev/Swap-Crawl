using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EntityInventory))]
[RequireComponent(typeof(WeaponHandler))]
public class EnemyAI : MonoBehaviour
{
    [Header("Targeting")]
    public Transform playerTarget;
    public float detectionRange = 10f;
    
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stopDistanceRanged = 5f;
    public float stopDistanceMelee = 1f;

    [Header("Combat Stats")]
    public float aimSpread = 15f;

    [Header("Effects")]
    public GameObject feetDustPrefab;
    public Transform feetTransform;
    public float dustSpawnRate = 0.2f;
    private float dustTimer;

    private float currentFireTimer;

    private Rigidbody2D rb;
    private EntityInventory inventory;
    private WeaponHandler weaponHandler;
    private WeaponVisuals weaponVisuals;
    private BodyVisuals bodyVisuals;
    private Transform visualBody;
    private bool isKnockedBack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inventory = GetComponent<EntityInventory>();
        weaponHandler = GetComponent<WeaponHandler>();
        bodyVisuals = GetComponentInChildren<BodyVisuals>();
        
        weaponVisuals = GetComponentInChildren<WeaponVisuals>();
        if (weaponVisuals != null)
        {
            visualBody = weaponVisuals.bodyTransform;
        }

        weaponHandler.usePlayerInput = false;
    }

    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        
        currentFireTimer = Random.Range(0.5f, 1f);
    }

    private void Update()
    {
        if (isKnockedBack) return;
        if (playerTarget == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > detectionRange)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (rb.linearVelocity.sqrMagnitude > 0.01f)
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

        HandleCombat(distanceToPlayer);
    }

    public void ApplyKnockback(Vector2 force)
    {
        isKnockedBack = true;
        if (bodyVisuals != null) bodyVisuals.isKnockedBack = true;
        
        rb.AddForce(force, ForceMode2D.Impulse);
        
        CancelInvoke(nameof(StopKnockback));
        Invoke(nameof(StopKnockback), 0.2f);
    }

    private void StopKnockback()
    {
        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
        if (bodyVisuals != null) bodyVisuals.isKnockedBack = false;
    }

    private void HandleCombat(float distance)
    {
        WeaponData weapon = inventory.currentWeapon;
        bool isRanged = (weapon != null && weapon.isRanged);
        float requiredDistance = isRanged ? stopDistanceRanged : stopDistanceMelee;

        weaponHandler.targetTransform = playerTarget;
        if (weaponVisuals != null)
        {
            weaponVisuals.targetTransform = playerTarget;
        }

        Vector2 direction = (playerTarget.position - transform.position).normalized;

        float minFireDelay = isRanged ? 0.25f : 0.1f;
        float maxFireDelay = isRanged ? 0.75f : 0.3f;

        if (distance > requiredDistance)
        {
            float toolSpeedMult = (inventory.currentTool != null) ? inventory.currentTool.moveSpeedMultiplier : 1f;
            rb.linearVelocity = direction * moveSpeed * toolSpeedMult;
            
            currentFireTimer = Random.Range(minFireDelay, maxFireDelay);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            
            if (weapon != null && distance <= weapon.range)
            {
                currentFireTimer -= Time.deltaTime;

                if (currentFireTimer <= 0)
                {
                    weaponHandler.AttemptAttack(aimSpread);
                    currentFireTimer = Random.Range(minFireDelay, maxFireDelay);
                }
            }
        }

        if (visualBody != null)
        {
            if (direction.x > 0)
            {
                visualBody.localScale = new Vector3(Mathf.Abs(visualBody.localScale.x), visualBody.localScale.y, visualBody.localScale.z);
            }
            else if (direction.x < 0)
            {
                visualBody.localScale = new Vector3(-Mathf.Abs(visualBody.localScale.x), visualBody.localScale.y, visualBody.localScale.z);
            }
        }
    }
}