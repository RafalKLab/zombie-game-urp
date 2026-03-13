//using System.Collections.Generic;
//using NUnit.Framework;

//public class BaseSlotServiceTests
//{
//    private BaseSlotService baseSlotService;

//    [SetUp]
//    public void SetUp()
//    {
//        baseSlotService = new BaseSlotService();
//    }

//    public static IEnumerable<TestCaseData> TryStartBuildCases()
//    {
//        yield return new TestCaseData(
//            BaseSlotState.Empty,
//            BaseBuildingType.None,
//            false,
//            BaseBuildingType.Workshop,
//            true,
//            BaseSlotState.UnderConstruction,
//            BaseBuildingType.Workshop
//        ).SetName("TryStartBuild_WhenSlotIsEmpty_ShouldSucceed");

//        yield return new TestCaseData(
//            BaseSlotState.Ruined,
//            BaseBuildingType.CommandCenter,
//            true,
//            BaseBuildingType.Workshop,
//            false,
//            BaseSlotState.Ruined,
//            BaseBuildingType.CommandCenter
//        ).SetName("TryStartBuild_WhenSlotIsRuined_ShouldFail");

//        yield return new TestCaseData(
//            BaseSlotState.UnderConstruction,
//            BaseBuildingType.Medbay,
//            false,
//            BaseBuildingType.Workshop,
//            false,
//            BaseSlotState.UnderConstruction,
//            BaseBuildingType.Medbay
//        ).SetName("TryStartBuild_WhenSlotIsUnderConstruction_ShouldFail");

//        yield return new TestCaseData(
//            BaseSlotState.Active,
//            BaseBuildingType.Watchtower,
//            false,
//            BaseBuildingType.Workshop,
//            false,
//            BaseSlotState.Active,
//            BaseBuildingType.Watchtower
//        ).SetName("TryStartBuild_WhenSlotIsActive_ShouldFail");
//    }

//    [TestCaseSource(nameof(TryStartBuildCases))]
//    public void TryStartBuild_ShouldReturnExpectedResult_AndUpdateSlotCorrectly(
//        BaseSlotState initialSlotState,
//        BaseBuildingType initialBuildingType,
//        bool isPredefined,
//        BaseBuildingType requestedBuildingType,
//        bool expectedSuccess,
//        BaseSlotState expectedSlotState,
//        BaseBuildingType expectedBuildingType)
//    {
//        // Arrange
//        BaseSlotData payload = CreateSlotData(
//            slotState: initialSlotState,
//            buildingType: initialBuildingType,
//            isPredefined: isPredefined
//        );

//        // Act
//        bool actualSuccess = baseSlotService.TryStartBuild(payload, requestedBuildingType);

//        // Assert
//        Assert.AreEqual(expectedSuccess, actualSuccess);
//        Assert.AreEqual(expectedSlotState, payload.SlotState);
//        Assert.AreEqual(expectedBuildingType, payload.BuildingType);
//    }

//    public static IEnumerable<TestCaseData> TryStartRepairCases()
//    {
//        yield return new TestCaseData(
//            BaseSlotState.Ruined,
//            BaseBuildingType.Warehouse,
//            true,
//            true,
//            BaseSlotState.UnderConstruction,
//            BaseBuildingType.Warehouse
//        ).SetName("Repair_RuinedSlot_ShouldStartConstruction");

//        yield return new TestCaseData(
//            BaseSlotState.Empty,
//            BaseBuildingType.None,
//            false,
//            false,
//            BaseSlotState.Empty,
//            BaseBuildingType.None
//        ).SetName("Repair_EmptySlot_ShouldFail");

//        yield return new TestCaseData(
//            BaseSlotState.Active,
//            BaseBuildingType.Warehouse,
//            true,
//            false,
//            BaseSlotState.Active,
//            BaseBuildingType.Warehouse
//        ).SetName("Repair_ActiveSlot_ShouldFail");

//        yield return new TestCaseData(
//            BaseSlotState.UnderConstruction,
//            BaseBuildingType.Warehouse,
//            true,
//            false,
//            BaseSlotState.UnderConstruction,
//            BaseBuildingType.Warehouse
//        ).SetName("Repair_UnderConstructionSlot_ShouldFail");
//    }

//    [TestCaseSource(nameof(TryStartRepairCases))]
//    public void TryStartRepair_ShouldReturnExpectedResult_AndUpdateSlotCorrectly(
//    BaseSlotState initialState,
//    BaseBuildingType initialBuilding,
//    bool isPredefined,
//    bool expectedSuccess,
//    BaseSlotState expectedState,
//    BaseBuildingType expectedBuilding)
//    {
//        // Arrange
//        BaseSlotData payload = CreateSlotData(initialState, initialBuilding, isPredefined);

//        // Act
//        bool actualSuccess = baseSlotService.TryStartRepair(payload);

//        // Assert
//        Assert.AreEqual(expectedSuccess, actualSuccess);
//        Assert.AreEqual(expectedState, payload.SlotState);
//        Assert.AreEqual(expectedBuilding, payload.BuildingType);
//    }

//    public static IEnumerable<TestCaseData> TryFinishConstructionCases()
//    {
//        yield return new TestCaseData(
//            BaseSlotState.UnderConstruction,
//            BaseBuildingType.Workshop,
//            false,
//            true,
//            BaseSlotState.Active,
//            BaseBuildingType.Workshop
//        ).SetName("FinishConstruction_UnderConstruction_ShouldActivate");

//        yield return new TestCaseData(
//            BaseSlotState.Empty,
//            BaseBuildingType.None,
//            false,
//            false,
//            BaseSlotState.Empty,
//            BaseBuildingType.None
//        ).SetName("FinishConstruction_EmptySlot_ShouldFail");

//        yield return new TestCaseData(
//            BaseSlotState.Ruined,
//            BaseBuildingType.CommandCenter,
//            true,
//            false,
//            BaseSlotState.Ruined,
//            BaseBuildingType.CommandCenter
//        ).SetName("FinishConstruction_RuinedSlot_ShouldFail");

//        yield return new TestCaseData(
//            BaseSlotState.Active,
//            BaseBuildingType.Workshop,
//            false,
//            false,
//            BaseSlotState.Active,
//            BaseBuildingType.Workshop
//        ).SetName("FinishConstruction_ActiveSlot_ShouldFail");
//    }

//    [TestCaseSource(nameof(TryFinishConstructionCases))]
//    public void TryFinishConstruction_ShouldReturnExpectedResult_AndUpdateSlotCorrectly(
//    BaseSlotState initialState,
//    BaseBuildingType initialBuilding,
//    bool isPredefined,
//    bool expectedSuccess,
//    BaseSlotState expectedState,
//    BaseBuildingType expectedBuilding)
//    {
//        // Arrange
//        BaseSlotData payload = CreateSlotData(initialState, initialBuilding, isPredefined);

//        // Act
//        bool actualSuccess = baseSlotService.TryFinishConstruction(payload);

//        // Assert
//        Assert.AreEqual(expectedSuccess, actualSuccess);
//        Assert.AreEqual(expectedState, payload.SlotState);
//        Assert.AreEqual(expectedBuilding, payload.BuildingType);
//    }

//    public static IEnumerable<TestCaseData> TryDemolishCases()
//    {
//        yield return new TestCaseData(
//            BaseSlotState.Active,
//            BaseBuildingType.Workshop,
//            false,
//            true,
//            BaseSlotState.Empty,
//            BaseBuildingType.None
//        ).SetName("Demolish_ActiveNonPredefined_ShouldSucceed");

//        yield return new TestCaseData(
//            BaseSlotState.Active,
//            BaseBuildingType.CommandCenter,
//            true,
//            false,
//            BaseSlotState.Active,
//            BaseBuildingType.CommandCenter
//        ).SetName("Demolish_PredefinedActive_ShouldFail");

//        yield return new TestCaseData(
//            BaseSlotState.Empty,
//            BaseBuildingType.None,
//            false,
//            false,
//            BaseSlotState.Empty,
//            BaseBuildingType.None
//        ).SetName("Demolish_EmptySlot_ShouldFail");

//        yield return new TestCaseData(
//            BaseSlotState.Ruined,
//            BaseBuildingType.Warehouse,
//            true,
//            false,
//            BaseSlotState.Ruined,
//            BaseBuildingType.Warehouse
//        ).SetName("Demolish_RuinedSlot_ShouldFail");

//        yield return new TestCaseData(
//            BaseSlotState.UnderConstruction,
//            BaseBuildingType.Medbay,
//            false,
//            false,
//            BaseSlotState.UnderConstruction,
//            BaseBuildingType.Medbay
//        ).SetName("Demolish_UnderConstruction_ShouldFail");
//    }

//    [TestCaseSource(nameof(TryDemolishCases))]
//    public void TryDemolish_ShouldReturnExpectedResult_AndUpdateSlotCorrectly(
//    BaseSlotState initialState,
//    BaseBuildingType initialBuilding,
//    bool isPredefined,
//    bool expectedSuccess,
//    BaseSlotState expectedState,
//    BaseBuildingType expectedBuilding)
//    {
//        // Arrange
//        BaseSlotData payload = CreateSlotData(initialState, initialBuilding, isPredefined);

//        // Act
//        bool actualSuccess = baseSlotService.TryDemolish(payload);

//        // Assert
//        Assert.AreEqual(expectedSuccess, actualSuccess);
//        Assert.AreEqual(expectedState, payload.SlotState);
//        Assert.AreEqual(expectedBuilding, payload.BuildingType);
//    }

//    private BaseSlotData CreateSlotData(
//        BaseSlotState slotState,
//        BaseBuildingType buildingType,
//        bool isPredefined,
//        BaseSlotType slotType = BaseSlotType.General,
//        string slotId = "test_slot")
//    {
//        return new BaseSlotData(
//            slotId,
//            slotType,
//            slotState,
//            buildingType,
//            isPredefined
//        );
//    }
//}