using UnityEngine;

public class BaseSlotPoint : MonoBehaviour
{
    [SerializeField] private string slotId;
    [SerializeField] private BaseSlotType slotType = BaseSlotType.General;
    [SerializeField] private BaseSlotState startState = BaseSlotState.Empty;
    [SerializeField] private BaseBuildingType startBuilding = BaseBuildingType.None;
    [SerializeField] private bool isPredefined;

    public string GetSlotId() => slotId;
    public Transform GetTransform() => transform;

    public BaseSlotData CreateSlotData()
    {
        return new BaseSlotData(
            slotId,
            slotType,
            startState,
            startBuilding,
            isPredefined
        );
    }
}