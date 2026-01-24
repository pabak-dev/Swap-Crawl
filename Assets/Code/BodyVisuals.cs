using UnityEngine;

public class BodyVisuals : MonoBehaviour
{
    public Animator animator;
    public float squashAmount = 0.1f;
    public float squashSpeed = 15f;

    private Rigidbody2D parentRb;
    private Vector3 originalScale;

    private void Start()
    {
        parentRb = GetComponentInParent<Rigidbody2D>();
        originalScale = transform.localScale;
        
        originalScale = new Vector3(Mathf.Abs(originalScale.x), Mathf.Abs(originalScale.y), originalScale.z);
    }

    private void Update()
    {
        if (parentRb == null) return;

        Vector2 vel = parentRb.linearVelocity;
        float speed = vel.magnitude;

        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
        }

        float direction = Mathf.Sign(transform.localScale.x);
        if (Mathf.Abs(vel.x) > 0.01f)
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
}