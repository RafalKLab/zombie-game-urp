using System.Collections.Generic;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    [SerializeField] private Faction faction;

    [SerializeField] private Transform center;
    [SerializeField] private float baseRadius;

    [Header("Base slots")]
    [SerializeField] private List<BaseSlotPoint> baseSlotPointList = new List<BaseSlotPoint>();
    private List<BaseSlotData> baseSlotDataList = new List<BaseSlotData>();
    private Dictionary<string, BaseSlotPoint> baseSlotPointById = new();

    [Header("Debug")]
    //[SerializeField] private BuildingDefinitionSO buildingDefinitionSO;
    [SerializeField] private int bIdenx;

    private BaseRadar baseRadar;
    private BaseSupplyManager baseSupplyManager;
    private ResourceManager resourceManager;

    private BaseSlotService baseSlotService;

    private void Awake()
    {
        baseRadar = GetComponent<BaseRadar>();
        baseSupplyManager = GetComponent<BaseSupplyManager>();
        resourceManager = GetComponent<ResourceManager>();

        baseSlotService = new BaseSlotService();
        InitializeSlots();
    }

    private void Update()
    {
        UpdateBuildingConstruction(Time.deltaTime);
    }

    private void InitializeSlots()
    {
        baseSlotDataList.Clear();
        baseSlotPointById.Clear();

        foreach (BaseSlotPoint slotPoint in baseSlotPointList)
        {
            BaseSlotData data = slotPoint.CreateSlotData();
            baseSlotDataList.Add(data);

            string slotId = slotPoint.GetSlotId();

            if (baseSlotPointById.ContainsKey(slotId))
            {
                Debug.LogError($"Duplicate slotId detected: {slotId}", this);
                continue;
            }

            baseSlotPointById.Add(slotId, slotPoint);

            slotPoint.SyncVisual(data);
        }
    }

    public Faction GetFaction() => faction;
    public BaseRadar GetBaseRadar() => baseRadar;
    public ResourceManager GetResourceManager() => resourceManager;
    public BaseSupplyManager GetBaseSupplyManager() => baseSupplyManager;

    public Vector3 GetCenter() => center != null ? center.position : transform.position;
    public float GetRadius() => baseRadius;

    public List<BaseSlotPoint> GetBaseSlotPointList() => baseSlotPointList;
    public List<BaseSlotData> GetBaseSlotDataList() => baseSlotDataList;
    public bool CanBuildOnSlot(BaseSlotData baseSlotData) => baseSlotService.CanBuildOnSlot(baseSlotData);
    public bool CanRepairSlot(BaseSlotData baseSlotData) => baseSlotService.CanRepairSlot(baseSlotData);
    public bool CanDemolishSlot(BaseSlotData baseSlotData) => baseSlotService.CanDemolishSlot(baseSlotData);

    //public bool TryStartRepair(BaseSlotData baseSlotData) => baseSlotService.TryStartRepair(baseSlotData);
    public bool TryFinishConstruction(BaseSlotData baseSlotData) => baseSlotService.TryFinishConstruction(baseSlotData);
    public bool TryDemolish(BaseSlotData baseSlotData) => baseSlotService.TryDemolish(baseSlotData);

    [ContextMenu("Debug Repair on slot")]
    public void RepairSlot()
    {
        int slotIndex = bIdenx;
        if (slotIndex < 0 || slotIndex >= baseSlotDataList.Count) return;

        BaseSlotData slotData = baseSlotDataList[slotIndex];
        BaseSlotPoint baseSlotPoint = baseSlotPointList[slotIndex];

        BuildingDefinitionSO slotBuilding = baseSlotPoint.GetAttachBuildingDefinition();

        bool success = baseSlotService.TryStartRepair(
            slotData,
            slotBuilding.repairTime
        );

        if (success) {
            baseSlotPoint.SyncVisual(slotData);
        }

        Debug.Log($"RepairSlot success: {success}, state: {slotData.SlotState}, building: {slotData.BuildingType}, time: {slotData.BuildRemainingTime}");
    }

    private void UpdateBuildingConstruction(float deltaTime)
    {
        foreach (BaseSlotData baseSlotData in baseSlotDataList)
        {
            if (baseSlotData.SlotState != BaseSlotState.UnderConstruction) continue;

            baseSlotData.TickConstruction(deltaTime);

            if (baseSlotData.BuildRemainingTime > 0f) continue;

            FinalizeSlotConstruction(baseSlotData);
        }
    }

    private void FinalizeSlotConstruction(BaseSlotData baseSlotData)
    {
        if (baseSlotData == null) return;

        bool success = baseSlotService.TryFinishConstruction(baseSlotData);
        if (!success)
        {
            Debug.LogWarning($"Failed to finalize construction on slot: {baseSlotData.SlotId}", this);
            return;
        }

        int slotIndex = baseSlotDataList.IndexOf(baseSlotData);
        if (slotIndex < 0 || slotIndex >= baseSlotPointList.Count)
        {
            Debug.LogWarning($"Could not find matching BaseSlotPoint for slot: {baseSlotData.SlotId}", this);
            return;
        }

        BaseSlotPoint baseSlotPoint = baseSlotPointList[slotIndex];

        baseSlotPoint.SyncVisual(baseSlotData);

        Debug.Log($"Construction finalized on slot: {baseSlotData.SlotId}", this);
    }

    public bool TryGetSlotById(string targetSlotId, out BaseSlotPoint targetBaseSlotPoint)
    {
        return baseSlotPointById.TryGetValue(targetSlotId, out targetBaseSlotPoint);
    }
}