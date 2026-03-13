public class BaseSlotData
{
    public string SlotId { get; private set; }
    public BaseSlotType SlotType { get; private set; }
    public BaseSlotState SlotState { get; private set; }
    public BaseBuildingType BuildingType { get; private set; }
    public bool IsPredefined { get; private set; }

    public float BuildRemainingTime { get; private set; }

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
        BuildRemainingTime = 0f;
    }

    public void SetSlotState(BaseSlotState newState)
    {
        SlotState = newState;
    }

    public void SetBuildingType(BaseBuildingType newBuildingType)
    {
        BuildingType = newBuildingType;
    }

    public void StartConstruction(float buildTime)
    {
        SlotState = BaseSlotState.UnderConstruction;
        BuildRemainingTime = buildTime;
    }

    public void TickConstruction(float deltaTime)
    {
        if (SlotState != BaseSlotState.UnderConstruction) return;

        BuildRemainingTime -= deltaTime;
        if (BuildRemainingTime < 0f)
        {
            BuildRemainingTime = 0f;
        }
    }

    public bool IsConstructionFinished()
    {
        return SlotState == BaseSlotState.UnderConstruction && BuildRemainingTime <= 0f;
    }

    public void FinishConstruction()
    {
        SlotState = BaseSlotState.Active;
        BuildRemainingTime = 0f;
    }
}