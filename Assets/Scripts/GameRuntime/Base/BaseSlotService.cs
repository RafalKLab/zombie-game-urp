public class BaseSlotService
{
    public bool CanBuildOnSlot(BaseSlotData baseSlotData)
    {
        if (baseSlotData == null) return false;
        return baseSlotData.SlotState == BaseSlotState.Empty;
    }

    public bool CanRepairSlot(BaseSlotData baseSlotData)
    {
        if (baseSlotData == null) return false;
        return baseSlotData.SlotState == BaseSlotState.Ruined;
    }

    public bool CanDemolishSlot(BaseSlotData baseSlotData)
    {
        if (baseSlotData == null) return false;
        if (baseSlotData.IsPredefined) return false;

        return baseSlotData.SlotState == BaseSlotState.Active;
    }

    public bool TryStartBuild(BaseSlotData baseSlotData, BaseBuildingType buildingType)
    {
        if (!CanBuildOnSlot(baseSlotData)) return false;

        baseSlotData.SetBuildingType(buildingType);
        baseSlotData.SetSlotState(BaseSlotState.UnderConstruction);
        return true;
    }

    public bool TryStartRepair(BaseSlotData baseSlotData)
    {
        if (!CanRepairSlot(baseSlotData)) return false;

        baseSlotData.SetSlotState(BaseSlotState.UnderConstruction);
        return true;
    }

    public bool TryFinishConstruction(BaseSlotData baseSlotData)
    {
        if (baseSlotData == null) return false;
        if (baseSlotData.SlotState != BaseSlotState.UnderConstruction) return false;

        baseSlotData.SetSlotState(BaseSlotState.Active);
        return true;
    }

    public bool TryDemolish(BaseSlotData baseSlotData)
    {
        if (!CanDemolishSlot(baseSlotData)) return false;

        baseSlotData.SetBuildingType(BaseBuildingType.None);
        baseSlotData.SetSlotState(BaseSlotState.Empty);
        return true;
    }
}