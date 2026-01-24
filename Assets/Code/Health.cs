using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 10f;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        // flash white feedback
        Debug.Log($"{name} took {amount} damage! HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // poof particle
        Destroy(gameObject);
    }
}