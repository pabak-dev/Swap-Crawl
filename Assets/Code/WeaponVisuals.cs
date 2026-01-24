using UnityEngine;
using System.Collections;

public class WeaponVisuals : MonoBehaviour
{
    [Header("Settings")]
    public float visualRotationOffset = -45f;
    public float rotationSmoothness = 15f;
    public float bobSpeed = 2f;
    public float bobAmount = 0.05f;

    [Header("Constraint")]
    public Transform bodyTransform;

    private SpriteRenderer sr;
    private Vector3 defaultPos;
    private bool isBusy;
    private float currentAngle;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        defaultPos = transform.localPosition;
    }

    private void Update()
    {
        if (!isBusy)
        {
            float newY = defaultPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            
            float targetX = defaultPos.x;
            if (Mathf.Abs(currentAngle) > 90)
            {
                targetX = -Mathf.Abs(defaultPos.x);
            }
            else
            {
                targetX = Mathf.Abs(defaultPos.x);
            }

            transform.localPosition = new Vector3(targetX, newY, defaultPos.z);
        }

        HandleSmoothAiming();
    }

    private void HandleSmoothAiming()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.parent.position;

        float facingDir = 1f;
        if (bodyTransform != null)
        {
            facingDir = Mathf.Sign(bodyTransform.localScale.x);
        }

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (facingDir > 0)
        {
            targetAngle = Mathf.Clamp(targetAngle, -80f, 80f);
        }
        else
        {
            if (targetAngle <= 0) targetAngle += 360;
            targetAngle = Mathf.Clamp(targetAngle, 100f, 260f);
        }

        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSmoothness);

        float activeOffset = visualRotationOffset;
        if (Mathf.Abs(currentAngle) > 90)
        {
            activeOffset = -visualRotationOffset;
        }

        transform.rotation = Quaternion.Euler(0, 0, currentAngle + activeOffset);

        if (Mathf.Abs(currentAngle) > 90)
        {
            transform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void TriggerRecoil()
    {
        if (isBusy) return;
        StartCoroutine(RecoilRoutine());
    }

    public void TriggerSwing()
    {
        if (isBusy) return;
        StartCoroutine(SwingRoutine());
    }

    private IEnumerator RecoilRoutine()
    {
        isBusy = true;
        
        Vector3 recoilOffset = transform.right * -0.2f; 
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = startPos + recoilOffset; 
        
        float timer = 0;
        while(timer < 0.1f) 
        {
            transform.position -= transform.right * Time.deltaTime * 5f; 
            timer += Time.deltaTime;
            yield return null;
        }
        while(timer < 0.2f) 
        {
             transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, Time.deltaTime * 10f);
             timer += Time.deltaTime;
             yield return null;
        }
        
        isBusy = false;
    }

    private IEnumerator SwingRoutine()
    {
        isBusy = true;
        Quaternion startRot = transform.localRotation;
        Quaternion endRot = Quaternion.Euler(0, 0, startRot.eulerAngles.z - 100); 

        float elapsed = 0;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            transform.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isBusy = false; 
    }
}