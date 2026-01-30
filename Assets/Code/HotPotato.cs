using UnityEngine;
using TMPro;

[RequireComponent(typeof(EntityInventory))]
[RequireComponent(typeof(Health))]
public class HotPotato : MonoBehaviour
{
    public float explodeTime = 5f;
    public float explosionDamage = 999f;
    public TextMeshProUGUI timerText; 

    private EntityInventory inventory;
    private Health health;
    public float timer;
    private bool isArmed;
    private bool hasPotato;
    public GameObject explosionVFX;
    public AudioClip explosionClip;

    private void Awake()
    {
        inventory = GetComponent<EntityInventory>();
        health = GetComponent<Health>();
    }

    void Start()
    {
        timer = explodeTime;
    }

    public void Arm()
    {
        isArmed = true;
    }

    private void Update()
    {
        if (inventory == null || inventory.currentTool == null) 
        {
            ResetPotato();
            return;
        }

        if (inventory.currentTool.toolName == "Hot Potato")
        {
            if (!isArmed)
            {
                if (timerText != null) timerText.gameObject.SetActive(false);
                return;
            }

            if (!hasPotato)
            {
                hasPotato = true;
            }

            timer -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = Mathf.Ceil(timer).ToString();
                timerText.gameObject.SetActive(true);
            }

            if (timer <= 0)
            {
                Explode();
            }
        }
        else
        {
            ResetPotato();
        }
    }

    private void ResetPotato()
    {
        hasPotato = false;
        isArmed = false;
        timer = explodeTime;
        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    private void Explode()
    {
        if (health != null)
        {
            health.TakeDamage(explosionDamage);
        }

        Health[] allHealthComponents = FindObjectsByType<Health>(FindObjectsSortMode.None);

        float radiusSqr = 1 * 1;
        Vector3 explosionCenter = transform.position;

        foreach (Health h in allHealthComponents)
        {
            if (h == null || h == health) continue;

            float distSqr = (h.transform.position - explosionCenter).sqrMagnitude;

            if (distSqr <= radiusSqr)
            {
                h.TakeDamage(explosionDamage);
            }
        }

        inventory.currentTool = null;
        inventory.UpdateVisuals();

        GlobalSFX.Instance.Play(explosionClip);
        Destroy(Instantiate(explosionVFX, transform.position, Quaternion.identity), 2f);
        
        ResetPotato();
    }
}