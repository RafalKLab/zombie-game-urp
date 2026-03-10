using UnityEngine;

public class BaseViewUI : MonoBehaviour
{
    private bool isActive = false;
    public bool IsActive => isActive;

    public void Hide()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        isActive = true;
        gameObject.SetActive(true);
    }
}
