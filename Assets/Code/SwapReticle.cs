using UnityEngine;
using System.Collections.Generic;

public class SwapReticle : MonoBehaviour
{
    public GameObject reticlePrefab;

    [Header("Visuals")]
    public Color selectedColor = Color.green;
    public Color cooldownColor = new(1, 0.2f, 0.2f, 0.5f);
    public Color availableColor = Color.white;

    private List<SpriteRenderer> activeReticles = new();

    public void ShowReticles(List<EntityInventory> targets, EntityInventory lockedTarget, bool isReady)
    {
        while (activeReticles.Count < targets.Count)
        {
            GameObject obj = Instantiate(reticlePrefab, transform);
            activeReticles.Add(obj.GetComponent<SpriteRenderer>());
        }

        for (int i = 0; i < activeReticles.Count; i++)
        {
            if (i < targets.Count)
            {
                activeReticles[i].gameObject.SetActive(true);
                activeReticles[i].transform.position = targets[i].transform.position;
                
                if (!isReady)
                {
                    activeReticles[i].color = cooldownColor;
                }
                else if (targets[i] == lockedTarget)
                {
                    activeReticles[i].color = selectedColor;
                }
                else
                {
                    activeReticles[i].color = availableColor;
                }
            }
            else
            {
                activeReticles[i].gameObject.SetActive(false);
            }
        }
    }

    public void HideAll()
    {
        foreach (var r in activeReticles)
        {
            r.gameObject.SetActive(false);
        }
    }
}