using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseDetailSlotViewUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private TextMeshProUGUI slotStateText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Button closeButton;

    public void Show(BaseSlotRef baseSlotRef, Action closeAction)
    {
        if (baseSlotRef.Point == null) return;
        if (baseSlotRef.Data == null) return;
        if (displayNameText == null) return;
        if (slotStateText == null) return;

        RefreshContent(baseSlotRef);
        ShowCloseButton(closeAction);
        ShowActionButton(baseSlotRef);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        HideCloseButton();
        HideActionButton();

        displayNameText.text = string.Empty;
        slotStateText.text = string.Empty;
    }

    private void RefreshContent(BaseSlotRef baseSlotRef)
    {
        BuildingDefinitionSO buildingDefinitionSO = baseSlotRef.Point.GetAttachBuildingDefinition();

        if (buildingDefinitionSO != null)
        {
            displayNameText.text = buildingDefinitionSO.displayName;
        }
        else
        {
            displayNameText.text = "Empty slot";
        }

        slotStateText.text = baseSlotRef.Data.SlotState.ToString();
    }

    private void ShowCloseButton(Action closeAction)
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => closeAction?.Invoke());
        closeButton.gameObject.SetActive(true);
    }

    private void HideCloseButton()
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.gameObject.SetActive(false);
    }

    private void ShowActionButton(BaseSlotRef baseSlotRef)
    {
        Debug.Log($"Slot state {baseSlotRef.Data.SlotState}");

        switch (baseSlotRef.Data.SlotState)
        {
            case BaseSlotState.UnderConstruction:
                ShowCancelButton(baseSlotRef);
                return;

            case BaseSlotState.Ruined:
                ShowRepairButton(baseSlotRef);
                return;

            case BaseSlotState.Empty:
                ShowBuildButton(baseSlotRef);
                return;

            case BaseSlotState.Active:
                if (baseSlotRef.Data.IsPredefined)
                {
                    HideActionButton();
                    return;
                }

                ShowDemolishButton(baseSlotRef);
                return;

            default:
                HideActionButton();
                return;
        }
    }

    private void ShowBuildButton(BaseSlotRef baseSlotRef)
    {
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => CallBuild(baseSlotRef));
        actionButtonText.text = "Build";
        actionButton.gameObject.SetActive(true);
    }

    private void ShowRepairButton(BaseSlotRef baseSlotRef)
    {
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => CallRepair(baseSlotRef));
        actionButtonText.text = "Repair";
        actionButton.gameObject.SetActive(true);
    }

    private void ShowCancelButton(BaseSlotRef baseSlotRef)
    {
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => CallCancel(baseSlotRef));
        actionButtonText.text = "Cancel";
        actionButton.gameObject.SetActive(true);
    }

    private void ShowDemolishButton(BaseSlotRef baseSlotRef)
    {
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => CallDemolish(baseSlotRef));
        actionButtonText.text = "Demolish";
        actionButton.gameObject.SetActive(true);
    }

    private void HideActionButton()
    {
        actionButton.onClick.RemoveAllListeners();
        actionButtonText.text = string.Empty;
        actionButton.gameObject.SetActive(false);
    }

    private void CallBuild(BaseSlotRef baseSlotRef)
    {
        Debug.Log("Build action called");
    }

    private void CallRepair(BaseSlotRef baseSlotRef)
    {
        Debug.Log("Repair action called");
    }

    private void CallCancel(BaseSlotRef baseSlotRef)
    {
        Debug.Log("Cancel action called");
    }

    private void CallDemolish(BaseSlotRef baseSlotRef)
    {
        Debug.Log("Demolish action called");
    }
}