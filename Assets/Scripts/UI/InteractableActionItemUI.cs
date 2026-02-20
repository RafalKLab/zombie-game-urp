using TMPro;
using UnityEngine;

public class InteractableActionItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBlock;
    [SerializeField] private CanvasGroup canvasGroup;

    public void SetText(string text)
    {
        if (textBlock != null)
            textBlock.text = text;
    }

    public void SetEnabledVisual(bool enabled)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = enabled ? 1f : 0.35f;
            canvasGroup.interactable = enabled;
            canvasGroup.blocksRaycasts = enabled;
        }
        else if (textBlock != null)
        {
            textBlock.alpha = enabled ? 1f : 0.35f;
        }
    }
}