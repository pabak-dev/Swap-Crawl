using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class ButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Settings")]
    [SerializeField] private bool ignoreIfNotInteractable = true;
    [SerializeField] private float hoverScale = 1.1f;

    private Button _button;
    private Vector3 _originalScale;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _originalScale = transform.localScale;
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

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ignoreIfNotInteractable && _button != null && !_button.interactable) return;
        
        PlayExitVFX();
    }

    private void PlayHoverVFX()
    {

        transform.DOKill(); 

        transform.DOScale(_originalScale * hoverScale, 0.15f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true); 
    }

    private void PlayExitVFX()
    {
        transform.DOKill();

        transform.DOScale(_originalScale, 0.15f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    private void PlayClickVFX()
    {
        transform.DOKill();

        transform.DOPunchScale(_originalScale * -0.1f, 0.15f, 10, 1)
            .SetUpdate(true)
            .OnComplete(() => {
                transform.localScale = _originalScale; 
            });
    }
}