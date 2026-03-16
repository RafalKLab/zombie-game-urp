using System.Collections.Generic;
using UnityEngine;

public class BaseViewUI : MonoBehaviour
{
    [SerializeField] private BaseManager baseManager;
    [SerializeField] private List<BaseViewSlotUI> baseViewSlotUIList = new List<BaseViewSlotUI>();

    private bool isActive = false;
    public bool IsActive => isActive;

    private bool isInitialized = false;

    private void Initialize()
    {
        if (isInitialized) return;

        foreach (BaseViewSlotUI baseViewSlotUI in baseViewSlotUIList)
        {
            baseViewSlotUI.SetBaseManager(baseManager);
        }

        isInitialized = true;
    }

    public void Hide()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        Initialize();

        foreach (BaseViewSlotUI baseViewSlotUI in baseViewSlotUIList)
        {
            baseViewSlotUI.UpdateVisual();
        }

        isActive = true;
        gameObject.SetActive(true);
    }
}