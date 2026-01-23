using UnityEngine;

public class EntityInventory : MonoBehaviour
{
    public enum SwapType { Weapon, Tool }

    public WeaponData currentWeapon;
    public ToolData currentTool;
    public SpriteRenderer heldItemRenderer;

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
        }
    }
}