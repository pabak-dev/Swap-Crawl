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

    [Header("Navigation")]
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 1.5f;
    public float enemyWidth = 0.5f;

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
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }
        
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
                if (feetDustPrefab && feetTransform) {
                     GameObject dust = Instantiate(feetDustPrefab, feetTransform.position, Quaternion.identity);
                     Destroy(dust, 1f);
                }
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

        Vector2 directToPlayer = (playerTarget.position - transform.position).normalized;

        float minFireDelay = isRanged ? 0.25f : 0.1f;
        float maxFireDelay = isRanged ? 0.75f : 0.3f;

        if (distance > requiredDistance)
        {
            float toolSpeedMult = (inventory.currentTool != null) ? inventory.currentTool.moveSpeedMultiplier : 1f;

            Vector2 smartMoveDir = GetDirectionWithAvoidance(directToPlayer);
            rb.linearVelocity = moveSpeed * toolSpeedMult * smartMoveDir;
            
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
            if (directToPlayer.x > 0)
            {
                visualBody.localScale = new Vector3(Mathf.Abs(visualBody.localScale.x), visualBody.localScale.y, visualBody.localScale.z);
            }
            else if (directToPlayer.x < 0)
            {
                visualBody.localScale = new Vector3(-Mathf.Abs(visualBody.localScale.x), visualBody.localScale.y, visualBody.localScale.z);
            }
        }
    }

    private Vector2 GetDirectionWithAvoidance(Vector2 desiredDir)
    {
        if (obstacleLayer == 0) return desiredDir;

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, enemyWidth, desiredDir, obstacleCheckDistance, obstacleLayer);

        if (hit.collider == null)
        {
            return desiredDir;
        }

        float[] candidateAngles = { 45f, -45f, 90f, -90f };

        foreach (float angle in candidateAngles)
        {
            Vector2 rotatedDir = Quaternion.Euler(0, 0, angle) * desiredDir;
            
            hit = Physics2D.CircleCast(transform.position, enemyWidth, rotatedDir, obstacleCheckDistance * 0.75f, obstacleLayer);
            
            if (hit.collider == null)
            {
                return Vector2.Lerp(desiredDir, rotatedDir, 0.7f).normalized;
            }
        }

        return desiredDir;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        if (playerTarget != null)
        {
            Vector2 dir = (playerTarget.position - transform.position).normalized;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)dir * obstacleCheckDistance);
            Gizmos.DrawWireSphere(transform.position + (Vector3)dir * obstacleCheckDistance, enemyWidth);
        }
    }
}