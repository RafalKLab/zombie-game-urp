using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseDetailSlotViewUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private Button acitonButton; // build or demolish
    [SerializeField] private Button closeButton;

    public void Show(BaseSlotPoint baseSlotPoint, Action closeAction)
    {
        if (baseSlotPoint == null) return;
        if (displayNameText == null) return;

        BuildingDefinitionSO buildingDefinitionSO = baseSlotPoint.GetAttachBuildingDefinition();
        if (buildingDefinitionSO != null)
        {
            displayNameText.text = buildingDefinitionSO.displayName;
        } else
        {
            displayNameText.text = "Empty slot";
        }

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => closeAction());

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        closeButton.onClick.RemoveAllListeners();
        displayNameText.text = "";
    }
}
