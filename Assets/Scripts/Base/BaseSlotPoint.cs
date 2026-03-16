using System.Collections;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using UnityEngine;

public class BaseSlotPoint : MonoBehaviour
{
    [Header("Slot setup")]
    [SerializeField] private string slotId;
    [SerializeField] private BaseSlotType slotType = BaseSlotType.General;
    [SerializeField] private BaseSlotState startState = BaseSlotState.Empty;
    [SerializeField] private bool isPredefined;
    [SerializeField] private BuildingDefinitionSO startBuildingDefinition;
    [SerializeField] private NavMeshSurface globalSurface;

    [SerializeField] private CinemachineCamera slotViewCamera;

    public CinemachineCamera GetSlotViewCamera() => slotViewCamera;

    private GameObject spawnedBuilding;
    private BuildingDefinitionSO currentBuildingDefinition;

    public string GetSlotId() => slotId;
    public Transform GetTransform() => transform;
    public BuildingDefinitionSO GetStartBuildingDefinition() => startBuildingDefinition;

    public BaseSlotData CreateSlotData()
    {
        BaseBuildingType buildingType = BaseBuildingType.None;

        if (startBuildingDefinition != null)
        {
            buildingType = startBuildingDefinition.buildingType;
            currentBuildingDefinition = startBuildingDefinition;
        }

        return new BaseSlotData(
            slotId,
            slotType,
            startState,
            buildingType,
            isPredefined
        );
    }

    public BuildingDefinitionSO GetAttachBuildingDefinition()
    {
        return currentBuildingDefinition;
    }

    public void AttachBuildingDefinition(BuildingDefinitionSO buildingDefinitionSO)
    {
        currentBuildingDefinition = buildingDefinitionSO;
    }

    public void DettachBuildingDefinition()
    {
        currentBuildingDefinition = null;
    }

    public void SyncVisual(BaseSlotData baseSlotData)
    {
        if (baseSlotData == null)
        {
            ClearSpawnedBuilding();
            return;
        }

        switch (baseSlotData.SlotState)
        {
            case BaseSlotState.Empty:
                ClearSpawnedBuilding();
                break;

            case BaseSlotState.Ruined:
                SpawnRuinedBuilding();
                break;

            case BaseSlotState.UnderConstruction:
                SpawnBuildConstructionBuilding();
                break;

            case BaseSlotState.Active:
                SpawnFinalBuilding();
                break;
        }

        UpdateNavMesh();
    }

    public void SpawnBuildConstructionBuilding()
    {
        if (currentBuildingDefinition == null) return;
        if (currentBuildingDefinition.buildConstructionPrefab == null) return;
        ClearSpawnedBuilding();

        spawnedBuilding = Instantiate(currentBuildingDefinition.buildConstructionPrefab, transform);
    }

    public void SpawnRepairConstructionBuilding()
    {
        if (currentBuildingDefinition == null) return;
        if (currentBuildingDefinition.repairConstructionPrefab == null) return;

        ClearSpawnedBuilding();
        spawnedBuilding = Instantiate(currentBuildingDefinition.repairConstructionPrefab, transform);
    }

    public void SpawnFinalBuilding()
    {
        if (currentBuildingDefinition == null) return;
        if (currentBuildingDefinition.finishedBuildingPrefab == null) return;
        ClearSpawnedBuilding();

        spawnedBuilding = Instantiate(currentBuildingDefinition.finishedBuildingPrefab, transform);
    }

    public void SpawnRuinedBuilding()
    {
        if (currentBuildingDefinition == null) return;
        if (currentBuildingDefinition.ruinedPrefab == null) return;

        ClearSpawnedBuilding();
        spawnedBuilding = Instantiate(currentBuildingDefinition.ruinedPrefab, transform);
    }

    public void ClearSpawnedBuilding()
    {
        if (spawnedBuilding == null) return;

        Destroy(spawnedBuilding);
        spawnedBuilding = null;

        UpdateNavMesh();
    }

    private void UpdateNavMesh()
    {
        StartCoroutine(UpdateNavMeshNextFrame());
    }

    private IEnumerator UpdateNavMeshNextFrame()
    {
        yield return null;

        if (globalSurface.navMeshData != null)
            globalSurface.UpdateNavMesh(globalSurface.navMeshData);
    }

    public GameObject GetSpawnedBuilding() => spawnedBuilding;
}