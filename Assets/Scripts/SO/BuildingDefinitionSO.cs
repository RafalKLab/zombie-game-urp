using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Base/Building Definition")]
public class BuildingDefinitionSO : ScriptableObject
{
    [Header("Features")]
    public bool canBeBuilt;
    public bool canBeDemolished;
    public bool canBeRepaired;

    [Header("Identity")]
    public BaseBuildingType buildingType;
    public string displayName;

    [Header("Visual")]
    public GameObject ruinedPrefab;
    public GameObject finishedBuildingPrefab;
    public Sprite icon;

    [Header("Build")]
    public float buildTime = 5f;
    public GameObject buildConstructionPrefab;

    [Header("Repair")]
    public float repairTime = 3f;
    public GameObject repairConstructionPrefab;
}