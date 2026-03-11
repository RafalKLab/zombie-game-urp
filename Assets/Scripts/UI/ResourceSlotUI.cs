using TMPro;
using UnityEngine;

public class ResourceSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resourceName;
    [SerializeField] private TextMeshProUGUI amount;

    public void SetName(string resourceName)
    {
        this.resourceName.text = resourceName;
    }

    public void SetAmount(int amount)
    {
        this.amount.text = amount.ToString();
    }
}
