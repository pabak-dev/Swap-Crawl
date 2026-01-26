using UnityEngine;
using System.Collections;
using TMPro;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private RectTransform healthBarFill;

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
    private EntityInventory inventory;

    [SerializeField] private GameObject poof_VFX;
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        currentHealth = maxHealth;
        sr = GetComponentInChildren<SpriteRenderer>();
        inventory = GetComponent<EntityInventory>();

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

        if (healthBarFill != null)
        {
            healthBarFill.localScale = new Vector3(Mathf.Max(currentHealth / maxHealth, 0f), 1f, 1f);
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        Debug.Log($"{name} healed {amount}! HP: {currentHealth}");
    }

    public void TakeDamage(float amount)
    {
        if (inventory != null && inventory.currentTool != null && inventory.currentTool.toolName == "Vampire Ring")
        {
            amount *= 1.5f;
        }

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
        if (poof_VFX != null) Destroy(Instantiate(poof_VFX, transform.position, Quaternion.identity), 2f);

        if (GameManager.Instance != null) GameManager.Instance.OnEnemyKilled();
        
        if (TryGetComponent<EnemyAI>(out var enemyAI))
        {
            enemyAI.enabled = false;
        }
        else
        {
            Time.timeScale = 0f;
            gameOverPanel.SetActive(true);
        }
        
        Destroy(gameObject, 0.095f);
    }
}