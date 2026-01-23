using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float damage;
    public float range;
    public float fireRate;
    public Sprite sprite;
}