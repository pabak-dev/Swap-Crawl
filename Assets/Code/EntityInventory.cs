using UnityEngine;

public class EntityInventory : MonoBehaviour
{
    public enum SwapType { Weapon, Tool }

    public WeaponData currentWeapon;
    public ToolData currentTool;
    public SpriteRenderer heldItemRenderer;
    public BodyVisuals bodyVisuals;

    private void Start()
    {
        UpdateVisuals();
    }

    public void SwapItems(EntityInventory other, SwapType type)
    {
        if (type == SwapType.Weapon)
        {
            WeaponData temp = currentWeapon;
            currentWeapon = other.currentWeapon;
            other.currentWeapon = temp;
        }
        else if (type == SwapType.Tool)
        {
            ToolData temp = currentTool;
            currentTool = other.currentTool;
            other.currentTool = temp;
        }

        UpdateVisuals();
        other.UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (currentWeapon != null && heldItemRenderer != null)
        {
            heldItemRenderer.sprite = currentWeapon.sprite;
            
            WeaponVisuals visuals = heldItemRenderer.GetComponent<WeaponVisuals>();
            if (visuals != null)
            {
                visuals.isRanged = currentWeapon.isRanged;
            }
        }

        if (bodyVisuals != null && bodyVisuals.toolOverlay != null)
        {print("xxx");
            if (currentTool != null)
            {
                bodyVisuals.toolOverlay.sprite = currentTool.visualIndicator;
            }
            else
            {
                bodyVisuals.toolOverlay.sprite = null;
            }
        }
    }
}