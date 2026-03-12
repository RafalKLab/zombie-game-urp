public class BaseSlotData
{
    public string SlotId { get; private set; }
    public BaseSlotType SlotType { get; private set; }
    public BaseSlotState SlotState { get; private set; }
    public BaseBuildingType BuildingType { get; private set; }
    public bool IsPredefined { get; private set; }

    public BaseSlotData(
        string slotId,
        BaseSlotType slotType,
        BaseSlotState slotState,
        BaseBuildingType buildingType,
        bool isPredefined)
    {
        SlotId = slotId;
        SlotType = slotType;
        SlotState = slotState;
        BuildingType = buildingType;
        IsPredefined = isPredefined;
    }

    public void SetSlotState(BaseSlotState newState)
    {
        SlotState = newState;
    }

    public void SetBuildingType(BaseBuildingType newBuildingType)
    {
        BuildingType = newBuildingType;
    }

    public bool IsEmpty() => SlotState == BaseSlotState.Empty;
    public bool IsRuined() => SlotState == BaseSlotState.Ruined;
    public bool IsUnderConstruction() => SlotState == BaseSlotState.UnderConstruction;
    public bool IsActive() => SlotState == BaseSlotState.Active;
}