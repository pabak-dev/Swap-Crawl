using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityInventory : MonoBehaviour
{
    public enum SwapType { Weapon, Tool }

    public WeaponData currentWeapon;
    public ToolData currentTool;
    public SpriteRenderer heldItemRenderer;
    public BodyVisuals bodyVisuals;

    public bool isPlayer;
    public TextMeshProUGUI toolNameText;
    public Image toolIconImage;

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
        {
            if (currentTool != null)
            {
                bodyVisuals.toolOverlay.sprite = currentTool.visualIndicator;
            }
            else
            {
                bodyVisuals.toolOverlay.sprite = null;
            }
        }

        if (isPlayer)
        {
            toolIconImage.sprite = currentTool != null ? currentTool.visualIndicator : null;
            toolIconImage.enabled = currentTool != null;

            string descText;
            descText = currentTool != null ? currentTool.toolName : "No Tool Equipped";
            descText += "\n";
            descText += currentTool != null ? currentTool.description : "No effects";

            toolNameText.text = descText;
        }
    }
}