using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseViewSlotUI : MonoBehaviour
{
    [SerializeField] private string mapSlotId;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI secText;

    private BaseManager baseManager;

    public void SetBaseManager(BaseManager baseManager)
    {
        this.baseManager = baseManager;
    }

    public void UpdateVisual()
    {
        if (baseManager == null) return;
        if (mainText == null) return;
        if (secText == null) return;

        if (baseManager.TryGetSlotById(mapSlotId, out BaseSlotPoint baseSlotPoint))
        {
            BuildingDefinitionSO buildingDefinitionSO = baseSlotPoint.GetAttachBuildingDefinition();

            if (buildingDefinitionSO != null)
            {
                mainText.text = buildingDefinitionSO.displayName;
            }
            else
            {
                mainText.text = "Empty slot";
            }
        }
    }

    public void InitButton(Action<BaseSlotPoint> enablePreviewAction)
    {
        if (button == null) return;

        // get slot point
        if (baseManager == null) return;

        if (baseManager.TryGetSlotById(mapSlotId, out BaseSlotPoint baseSlotPoint))
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() => enablePreviewAction(baseSlotPoint));
        } else
        {
            return;
        }
    }
}
