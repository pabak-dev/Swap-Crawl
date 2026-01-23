using UnityEngine;

[CreateAssetMenu(fileName = "New Tool", menuName = "Items/Tool")]
public class ToolData : ScriptableObject
{
    public string toolName;
    public float moveSpeedMultiplier = 1f;
    public bool isCursed;
    public Sprite visualIndicator;
}