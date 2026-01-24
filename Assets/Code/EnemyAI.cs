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

    private Rigidbody2D rb;
    private EntityInventory inventory;
    private WeaponHandler weaponHandler;
    private WeaponVisuals weaponVisuals;
    private Transform visualBody;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inventory = GetComponent<EntityInventory>();
        weaponHandler = GetComponent<WeaponHandler>();
        
        weaponVisuals = GetComponentInChildren<WeaponVisuals>();
        if (weaponVisuals != null)
        {
            visualBody = weaponVisuals.bodyTransform;
        }

        weaponHandler.usePlayerInput = false;
    }

    private void Update()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > detectionRange)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        HandleCombat(distanceToPlayer);
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

        if (distance > requiredDistance)
        {
            float toolSpeedMult = (inventory.currentTool != null) ? inventory.currentTool.moveSpeedMultiplier : 1f;
            rb.linearVelocity = direction * moveSpeed * toolSpeedMult;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            
            if (weapon != null && distance <= weapon.range)
            {
                weaponHandler.AttemptAttack();
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