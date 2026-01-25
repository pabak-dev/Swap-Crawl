using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float damage;
    public float range;
    public float fireRate;
    public Sprite sprite;

    [Header("Visuals")]
    public GameObject attackVFX;
    public bool isRanged;
    public Vector2 muzzleOffset;
}