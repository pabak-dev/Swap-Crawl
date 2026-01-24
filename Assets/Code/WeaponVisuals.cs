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
    private Vector3 parentDefaultPos;
    private bool isBusy;
    private float currentAngle;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        defaultPos = transform.localPosition;
        
        if (transform.parent != null)
        {
            parentDefaultPos = transform.parent.localPosition;
        }
    }

    private void Update()
    {
        float facingDir = 1f;
        if (bodyTransform != null)
        {
            facingDir = Mathf.Sign(bodyTransform.localScale.x);
        }

        if (!isBusy)
        {
            float newY = defaultPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.localPosition = new Vector3(defaultPos.x, newY, defaultPos.z);

            if (transform.parent != null)
            {
                float targetParentX = (facingDir > 0) ? Mathf.Abs(parentDefaultPos.x) : -Mathf.Abs(parentDefaultPos.x);
                transform.parent.localPosition = new Vector3(targetParentX, parentDefaultPos.y, parentDefaultPos.z);
            }
        }

        HandleSmoothAiming(facingDir);
    }

    private void HandleSmoothAiming(float facingDir)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.parent.position;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // if (facingDir > 0) 
        // {
        //     if (Mathf.Abs(targetAngle) > 89f)
        //     {
        //         targetAngle = 45f;
        //     }
        // }
        // else 
        // {
        //     if (Mathf.Abs(targetAngle) < 91)
        //     {
        //         targetAngle = 135f;
        //     }
        // }

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
        
        transform.localPosition = defaultPos;
        
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