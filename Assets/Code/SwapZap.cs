using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class SwapZap : MonoBehaviour
{
    public float duration = 0.15f;
    private LineRenderer lr;

    public void Initialize(Vector3 start, Vector3 end)
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        float timer = 0;
        Color startColor = lr.startColor;
        float startWidth = lr.widthMultiplier;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            
            Color c = startColor;
            c.a = Mathf.Lerp(1, 0, progress);
            lr.startColor = c;
            lr.endColor = c;
            
            lr.widthMultiplier = Mathf.Lerp(startWidth, 0, progress);
            
            yield return null;
        }

        Destroy(gameObject);
    }
}