using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 10f;
    private float currentHealth;

    [Header("Visual Feedback")]
    [Tooltip("Assign the 'FlashMat' solid white material here.")]
    [SerializeField] private Material flashMaterial; 
    [SerializeField] private float flashDuration = 0.1f;

    private SpriteRenderer sr;
    private Material originalMaterial;
    private Coroutine flashRoutine;

    private void Awake()
    {
        currentHealth = maxHealth;
        
        sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            originalMaterial = sr.material;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        Debug.Log($"{name} took {amount} damage! HP: {currentHealth}");

        if (sr != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRoutine()
    {
        if (flashMaterial != null)
        {
            sr.material = flashMaterial;
            yield return new WaitForSeconds(flashDuration);
            sr.material = originalMaterial;
        }
        else
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            sr.color = Color.white;
        }

        flashRoutine = null;
    }

    private void Die()
    {
        // poof particle
        Destroy(gameObject);
    }
}