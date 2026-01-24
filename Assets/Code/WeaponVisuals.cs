using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class WeaponVisuals : MonoBehaviour
{
    [Header("Settings")]
    public float visualRotationOffset = -45f;
    public float rotationSmoothness = 15f;
    public float bobSpeed = 2f;
    public float bobAmount = 0.05f;
    public bool isRanged = true;

    [Header("Ranged Settings")]
    public float orbitRadius = 1.2f;
    public Vector2 pivotOffset = new Vector2(0, 0.5f);

    [Header("Constraint")]
    public Transform bodyTransform;

    private SpriteRenderer sr;
    private SpriteRenderer bodyRenderer;
    private Vector3 defaultPos;
    private Vector3 parentDefaultPos;
    private bool isBusy;
    private float currentAngle;
    private float currentRangedRecoil;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        defaultPos = transform.localPosition;
        
        if (transform.parent != null)
        {
            parentDefaultPos = transform.parent.localPosition;
        }

        if (bodyTransform != null)
        {
            bodyRenderer = bodyTransform.GetComponent<SpriteRenderer>();
            if (bodyRenderer == null) bodyRenderer = bodyTransform.GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (isRanged)
        {
            HandleRangedOrbit();
        }
        else
        {
            HandleMeleeBehavior();
        }
    }

    private void HandleRangedOrbit()
    {
        if (bodyTransform == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 orbitCenter = bodyTransform.position + (Vector3)pivotOffset;
        Vector2 direction = (mousePos - orbitCenter).normalized;

        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        float activeRadius = orbitRadius - currentRangedRecoil;
        Vector3 targetPos = orbitCenter + (Vector3)(direction * activeRadius);
        
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * rotationSmoothness);
        transform.rotation = Quaternion.Euler(0, 0, baseAngle);

        if (Mathf.Abs(baseAngle) > 90)
        {
            transform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        if (bodyRenderer != null)
        {
            if (direction.y > 0.1f) 
            {
                sr.sortingOrder = bodyRenderer.sortingOrder - 1;
            }
            else 
            {
                sr.sortingOrder = bodyRenderer.sortingOrder + 1;
            }
        }
    }

    private void HandleMeleeBehavior()
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
        float targetAngle = 0f;

        if (isRanged)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = mousePos - transform.parent.position;
            targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
        else
        {
            targetAngle = (facingDir > 0) ? 0f : 180f;
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

        if (isRanged)
        {
            StartCoroutine(RangedRecoilRoutine());
        }
        else
        {
            StartCoroutine(RecoilRoutine());
        }
    }

    [Header("Swing Settings")]
    public float swingAngle = 120f;
    public float swingDuration = 0.25f;

    public void TriggerSwing()
    {
        if (currentSwingRoutine != null) StopCoroutine(currentSwingRoutine);
        currentSwingRoutine = StartCoroutine(SwingRoutine());
    }

    private IEnumerator RangedRecoilRoutine()
    {
        float duration = 0.05f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            currentRangedRecoil = Mathf.Lerp(0f, 0.4f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        duration = 0.2f;
        elapsed = 0f;
        while (elapsed < duration)
        {
            currentRangedRecoil = Mathf.Lerp(0.4f, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        currentRangedRecoil = 0f;
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

    private Coroutine currentSwingRoutine;

    private IEnumerator SwingRoutine()
    {
        isBusy = true;
        
        float dirMultiplier = (transform.localScale.y < 0) ? -1f : 1f;
        
        Quaternion startRot = transform.localRotation;
        float baseZ = startRot.eulerAngles.z;

        float windUpAngle = baseZ + (20f * dirMultiplier); 
        float strikeAngle = baseZ - (swingAngle * dirMultiplier); 

        float timer = 0f;
        
        float windUpTime = swingDuration * 0.2f;
        while (timer < windUpTime)
        {
            timer += Time.deltaTime;
            float t = timer / windUpTime;
            t = 1f - Mathf.Pow(1f - t, 3); 
            
            Quaternion target = Quaternion.Euler(0, 0, Mathf.LerpAngle(baseZ, windUpAngle, t));
            transform.localRotation = target;
            yield return null;
        }

        timer = 0f;
        float strikeTime = swingDuration * 0.4f;
        while (timer < strikeTime)
        {
            timer += Time.deltaTime;
            float t = timer / strikeTime;
            t = t * t * t * t; 
            
            Quaternion target = Quaternion.Euler(0, 0, Mathf.LerpAngle(windUpAngle, strikeAngle, t));
            transform.localRotation = target;
            yield return null;
        }

        timer = 0f;
        float recoveryTime = swingDuration * 0.4f;
        while (timer < recoveryTime)
        {
            timer += Time.deltaTime;
            float t = timer / recoveryTime;
            t = t * t * (3f - 2f * t);
            
            Quaternion target = Quaternion.Euler(0, 0, Mathf.LerpAngle(strikeAngle, baseZ, t));
            transform.localRotation = target;
            yield return null;
        }

        transform.localRotation = Quaternion.Euler(0, 0, baseZ);
        isBusy = false; 
    }
}