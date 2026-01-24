using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SwapReticle : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public LayerMask targetLayer;

    [Header("Visuals")]
    public Color readyColor = Color.white;
    public Color cooldownColor = new(1, 0.2f, 0.2f, 0.5f);

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
        
        sr.sortingOrder = 100;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - player.transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(player.transform.position, direction, 10f, targetLayer);

        bool isValidTarget = false;

        if (hit.collider != null)
        {
            if (hit.collider.gameObject != player.gameObject)
            {
                EntityInventory targetInv = hit.collider.GetComponent<EntityInventory>();
                if (targetInv != null)
                {
                    isValidTarget = true;
                    transform.position = hit.transform.position;
                }
            }
        }

        if (isValidTarget)
        {
            sr.enabled = true;
            sr.color = player.IsSwapReady ? readyColor : cooldownColor;
        }
        else
        {
            sr.enabled = false;
        }
    }
}