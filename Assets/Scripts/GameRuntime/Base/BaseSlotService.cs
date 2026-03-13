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

    public bool TryStartBuild(BaseSlotData baseSlotData, BaseBuildingType buildingType, float buildTime)
    {
        if (!CanBuildOnSlot(baseSlotData)) return false;

        baseSlotData.SetBuildingType(buildingType);
        baseSlotData.StartConstruction(buildTime);
        return true;
    }

    public bool TryStartRepair(BaseSlotData baseSlotData, float repairTime)
    {
        if (!CanRepairSlot(baseSlotData)) return false;

        baseSlotData.StartConstruction(repairTime);
        return true;
    }

    public bool TryFinishConstruction(BaseSlotData baseSlotData)
    {
        if (baseSlotData == null) return false;
        if (!baseSlotData.IsConstructionFinished()) return false;

        baseSlotData.FinishConstruction();
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