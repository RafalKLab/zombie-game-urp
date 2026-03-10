using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NavButtonUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Colors")]
    [SerializeField] private Color defaultTextColor;
    [SerializeField] private Color selectedTextColor;

    private void Start()
    {
        Unselect();
    }

    public void Select()
    {
        background.gameObject.SetActive(true);
        text.color = selectedTextColor;
    }

    public void Unselect()
    {
        background.gameObject.SetActive(false);
        text.color = defaultTextColor;
    }
}
