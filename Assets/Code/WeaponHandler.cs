using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [Header("References")]
    public EntityInventory inventory;
    public WeaponVisuals weaponVisuals;
    public BulletTracer tracerPrefab;
    public LayerMask enemyLayer;

    [Header("Control")]
    public bool usePlayerInput = true;
    public Transform targetTransform;
    
    [Header("Combat Settings")]
    public float knockbackForce = 15f;

    private float nextAttackTime;
    private Health myHealth;

    private Vector3 AimPosition
    {
        get
        {
            if (targetTransform != null) return targetTransform.position;
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    void Start()
    {
        myHealth = GetComponent<Health>();
        if (!usePlayerInput)
        {
            targetTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void Update()
    {
        if (usePlayerInput && Input.GetMouseButtonDown(0))
        {
            AttemptAttack(0f);
        }
    }

    public void AttemptAttack(float spreadAngle)
    {
        WeaponData weapon = inventory.currentWeapon;
        if (weapon == null) return;
        
        if (Time.time < nextAttackTime) return;

        float fireRate = weapon.fireRate;
        if (inventory.currentTool != null && inventory.currentTool.toolName == "Berserker's Skull")
        {
            fireRate *= 1.5f;
            if (myHealth != null) myHealth.TakeDamage(1f);
        }

        nextAttackTime = Time.time + (1f / fireRate);

        if (weapon.projectilePrefab != null)
        {
            FireProjectile(weapon, spreadAngle);
        }
        else if (weapon.isRanged)
        {
            FireRifle(weapon, spreadAngle);
        }
        else
        {
            SwingSword(weapon);
        }
    }

    private void FireProjectile(WeaponData weapon, float spreadAngle)
    {
        weaponVisuals.TriggerRecoil();
        
        Vector2 aimDir = (AimPosition - transform.position).normalized;
        if (spreadAngle > 0)
        {
            float randomAngle = Random.Range(-spreadAngle, spreadAngle);
            aimDir = Quaternion.Euler(0, 0, randomAngle) * aimDir;
        }

        Vector3 muzzlePos = weaponVisuals.transform.position + (weaponVisuals.transform.right * weapon.muzzleOffset.x) + (weaponVisuals.transform.up * weapon.muzzleOffset.y);
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        GameObject projObj = Instantiate(weapon.projectilePrefab, muzzlePos, Quaternion.Euler(0, 0, angle));

        Collider2D projCollider = projObj.GetComponent<Collider2D>();
        Collider2D[] ownerColliders = GetComponentsInChildren<Collider2D>();

        if (projCollider != null)
        {
            foreach (Collider2D ownerCol in ownerColliders)
            {
                Physics2D.IgnoreCollision(projCollider, ownerCol);
            }
        }

        SimpleProjectile projScript = projObj.GetComponent<SimpleProjectile>();
        
        if (projScript != null)
        {
            projScript.damage = weapon.damage;
            projScript.knockbackForce = knockbackForce * 0.2f; 
            projScript.speed = weapon.projectileSpeed;
            projScript.owner = gameObject;
            projScript.hitLayers = enemyLayer;

            if (inventory.currentTool != null && inventory.currentTool.toolName == "Vampire Ring")
            {
                projScript.isVampiric = true;
            }
        }
    }

    private void SwingSword(WeaponData weapon)
    {
        weaponVisuals.TriggerSwing();

        float facingDir = Mathf.Sign(weaponVisuals.bodyTransform.localScale.x);
        
        Vector3 hitPoint = transform.position + (facingDir * weapon.range * Vector3.right);

        if (weapon.attackVFX != null)
        {
            float angle = (facingDir > 0) ? 0f : 180f;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Instantiate(weapon.attackVFX, hitPoint, rot);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPoint, 0.8f, enemyLayer);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Health targetHealth = hit.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(weapon.damage);
            }
            
            CheckVampireHeal();

            Vector2 direction = (hit.transform.position - transform.position).normalized;
            Vector2 force = direction * knockbackForce;

            PlayerController pc = hit.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ApplyKnockback(force);
                continue;
            }

            EnemyAI ai = hit.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.ApplyKnockback(force);
                continue;
            }

            Rigidbody2D targetRb = hit.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                targetRb.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }

    private void FireRifle(WeaponData weapon, float spreadAngle)
    {
        weaponVisuals.TriggerRecoil();

        Vector2 aimDir = (AimPosition - transform.position).normalized;

        if (spreadAngle > 0)
        {
            float randomAngle = Random.Range(-spreadAngle, spreadAngle);
            aimDir = Quaternion.Euler(0, 0, randomAngle) * aimDir;
        }

        Vector3 muzzlePos = weaponVisuals.transform.position + (weaponVisuals.transform.right * weapon.muzzleOffset.x) + (weaponVisuals.transform.up * weapon.muzzleOffset.y);

        if (weapon.attackVFX != null)
        {
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Instantiate(weapon.attackVFX, muzzlePos, rot);
        }

        Vector3 tracerStart = muzzlePos;
        Vector3 tracerEnd = transform.position + (Vector3)aimDir * weapon.range;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDir, weapon.range, enemyLayer);

        if (hit.collider != null)
        {
            tracerEnd = hit.point;

            if (hit.collider.gameObject == gameObject) return;

            Health targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(weapon.damage);
                CheckVampireHeal();
            }

            Vector2 force = aimDir * knockbackForce * 0.35f;

            PlayerController pc = hit.collider.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ApplyKnockback(force);
            }
            else
            {
                EnemyAI ai = hit.collider.GetComponent<EnemyAI>();
                if (ai != null)
                {
                    ai.ApplyKnockback(force);
                }
                else
                {
                    Rigidbody2D targetRb = hit.collider.GetComponent<Rigidbody2D>();
                    if (targetRb != null)
                    {
                        targetRb.AddForce(force, ForceMode2D.Impulse);
                    }
                }
            }
        }

        if (tracerPrefab != null)
        {
            BulletTracer tracer = Instantiate(tracerPrefab, Vector3.zero, Quaternion.identity);
            tracer.Initialize(tracerStart, tracerEnd);
        }
    }

    private void CheckVampireHeal()
    {
        if (inventory.currentTool != null && inventory.currentTool.toolName == "Vampire Ring")
        {
            if (myHealth != null) myHealth.Heal(2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (inventory != null && inventory.currentWeapon != null && !inventory.currentWeapon.isRanged)
        {
            Gizmos.color = Color.red;
            Vector3 hitPoint = transform.position + (transform.right * inventory.currentWeapon.range);
            Gizmos.DrawWireSphere(hitPoint, 0.8f);
        }
    }
}