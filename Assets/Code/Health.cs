using UnityEngine;
using System.Collections;
using TMPro;

public class Health : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;

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

    void Update()
    {
        if (healthText != null){
            healthText.text = currentHealth.ToString("F0") + " / " + maxHealth.ToString("F0");
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
        if (TryGetComponent<EnemyAI>(out var enemyAI))
        {
            enemyAI.enabled = false;
        }
        
        Destroy(gameObject, 0.095f);
    }
}