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

    [Header("Debug")]
    [SerializeField] private int debugSlotIndex;
    [SerializeField] private BaseBuildingType debugBuildingType = BaseBuildingType.Workshop;

    private BaseRadar baseRadar;
    private BaseSupplyManager baseSupplyManager;

    private BaseSlotService baseSlotService;

    private void Awake()
    {
        baseRadar = GetComponent<BaseRadar>();
        baseSupplyManager = GetComponent<BaseSupplyManager>();

        baseSlotService = new BaseSlotService();
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        baseSlotDataList.Clear();

        foreach (BaseSlotPoint slotPoint in baseSlotPointList)
        {
            BaseSlotData data = slotPoint.CreateSlotData();
            baseSlotDataList.Add(data);
        }
    }

    public Faction GetFaction() => faction;
    public BaseRadar GetBaseRadar() => baseRadar;
    public BaseSupplyManager GetBaseSupplyManager() => baseSupplyManager;

    public Vector3 GetCenter() => center != null ? center.position : transform.position;
    public float GetRadius() => baseRadius;

    public List<BaseSlotPoint> GetBaseSlotPointList() => baseSlotPointList;
    public List<BaseSlotData> GetBaseSlotDataList() => baseSlotDataList;
    public bool CanBuildOnSlot(BaseSlotData baseSlotData) => baseSlotService.CanBuildOnSlot(baseSlotData);
    public bool CanRepairSlot(BaseSlotData baseSlotData) => baseSlotService.CanRepairSlot(baseSlotData);
    public bool CanDemolishSlot(BaseSlotData baseSlotData) => baseSlotService.CanDemolishSlot(baseSlotData);

    public bool TryStartBuild(BaseSlotData baseSlotData, BaseBuildingType buildingType) => baseSlotService.TryStartBuild(baseSlotData, buildingType);
    public bool TryStartRepair(BaseSlotData baseSlotData) => baseSlotService.TryStartRepair(baseSlotData);
    public bool TryFinishConstruction(BaseSlotData baseSlotData) => baseSlotService.TryFinishConstruction(baseSlotData);
    public bool TryDemolish(BaseSlotData baseSlotData) => baseSlotService.TryDemolish(baseSlotData);
}