using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Settings")]
    [SerializeField] private bool ignoreIfNotInteractable = true;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ignoreIfNotInteractable && _button != null && !_button.interactable) return;

        if (hoverSound != null && GlobalSFX.Instance != null)
        {
            GlobalSFX.Instance.Play(hoverSound);
        }

        PlayHoverVFX(); 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ignoreIfNotInteractable && _button != null && !_button.interactable) return;

        if (clickSound != null && GlobalSFX.Instance != null)
        {
            GlobalSFX.Instance.Play(clickSound);
        }

        PlayClickVFX();
    }

    private void PlayHoverVFX()
    {
        // anims
    }

    private void PlayClickVFX()
    {
        // anims
    }
}