using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseViewSlotUI : MonoBehaviour
{
    [SerializeField] private string mapSlotId;
    [SerializeField] private Image icon;

    private BaseManager baseManager;

    public void SetBaseManager(BaseManager baseManager)
    {
        this.baseManager = baseManager;
    }

    public void UpdateVisual()
    {
        if (baseManager == null) return;

        if (baseManager.TryGetSlotById(mapSlotId, out BaseSlotPoint baseSlotPoint))
        {
            BuildingDefinitionSO buildingDefinitionSO = baseSlotPoint.GetAttachBuildingDefinition();

            if (buildingDefinitionSO != null)
            {
                icon.sprite = buildingDefinitionSO.icon;
            }
        }
    }
}
