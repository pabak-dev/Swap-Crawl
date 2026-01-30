using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class BodyVisuals : MonoBehaviour
{
    public Animator animator;
    public float squashAmount = 0.1f;
    public float squashSpeed = 15f;

    public bool isKnockedBack;

    private Rigidbody2D parentRb;
    private Vector3 originalScale;
    private SpriteRenderer sr;
    public SpriteRenderer toolOverlay;
    public SpriteRenderer weaponRenderer;

    private Material originalMaterial;
    public Material flashMaterial;

    private void Start()
    {
        parentRb = GetComponentInParent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        originalScale = new Vector3(Mathf.Abs(originalScale.x), Mathf.Abs(originalScale.y), originalScale.z);

        if (sr != null)
        {
            originalMaterial = sr.material;
        }
    }

    private void Update()
    {
        if (sr != null)
        {
            sr.sortingOrder = Mathf.RoundToInt(transform.position.y * -100f);
            
            if (weaponRenderer != null)
            {
                weaponRenderer.sortingOrder = sr.sortingOrder + 2;
            }

            if (toolOverlay != null)
            {
                toolOverlay.sortingOrder = sr.sortingOrder + 1;
            }
        }

        if (parentRb == null) return;

        Vector2 vel = parentRb.linearVelocity;
        float speed = vel.magnitude;

        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
        }

        float direction = Mathf.Sign(transform.localScale.x);
        
        if (Mathf.Abs(vel.x) > 0.01f && !isKnockedBack)
        {
            direction = Mathf.Sign(vel.x);
        }

        float squashX = 0;
        float squashY = 0;

        if (speed > 0.01f)
        {
            float wave = Mathf.Sin(Time.time * squashSpeed) * squashAmount;
            squashX = wave;
            squashY = -wave;
        }
        else
        {
            float currentDiff = Mathf.Abs(transform.localScale.x) - originalScale.x;
            squashX = Mathf.Lerp(currentDiff, 0, Time.deltaTime * 10f);
            squashY = -squashX;
        }

        float finalX = (originalScale.x + squashX) * direction;
        float finalY = originalScale.y + squashY;

        transform.localScale = new Vector3(finalX, finalY, originalScale.z);
    }

    public void TriggerSwapFlash()
    {
        StartCoroutine(SwapFlashRoutine());
    }

    private IEnumerator SwapFlashRoutine()
    {
        sr.material = flashMaterial;
        yield return new WaitForSeconds(0.15f);
        sr.material = originalMaterial;
    }
}