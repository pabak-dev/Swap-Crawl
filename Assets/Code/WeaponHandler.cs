using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [Header("References")]
    public EntityInventory inventory;
    public WeaponVisuals weaponVisuals;
    public LayerMask enemyLayer;

    private float nextAttackTime;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AttemptAttack();
        }
    }

    private void AttemptAttack()
    {
        WeaponData weapon = inventory.currentWeapon;
        if (weapon == null) return;
        
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + (1f / weapon.fireRate);

        if (weapon.isRanged)
        {
            FireRifle(weapon);
        }
        else
        {
            SwingSword(weapon);
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
        }
    }

    private void FireRifle(WeaponData weapon)
    {
        weaponVisuals.TriggerRecoil();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDir = (mousePos - transform.position).normalized;

        if (weapon.attackVFX != null)
        {
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Instantiate(weapon.attackVFX, transform.position + (Vector3)aimDir * 0.5f, rot);
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDir, weapon.range, enemyLayer);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject == gameObject) return;

            Health targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(weapon.damage);
            }
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