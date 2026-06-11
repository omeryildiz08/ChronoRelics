using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonClickSfxHandler : MonoBehaviour, IPointerDownHandler
{
    private UIButtonClickSfxBinder binder;
    private Button button;

    public void Initialize(UIButtonClickSfxBinder owner, Button targetButton)
    {
        binder = owner;
        button = targetButton;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (binder == null || button == null || !button.IsActive() || !button.interactable)
        {
            return;
        }

        binder.PlayClickSound();
    }
}
