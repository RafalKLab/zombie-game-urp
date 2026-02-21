using TMPro;
using UnityEngine;

public class ResourceTypeLineItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;

    public void SetText(string text)
    {
        if (text == null) return;
        if (textComponent == null) return;

        textComponent.text = text;
    }
}
