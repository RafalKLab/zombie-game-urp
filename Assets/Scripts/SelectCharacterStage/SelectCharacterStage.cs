using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectCharacterStage : MonoBehaviour
{
    [SerializeField] private Transform characterPreviewContainer;
    [SerializeField] private CharacterPreviewSlot characterPreviewSlotPrefab;
    [SerializeField] private int maxPreviewSlots = 10;

    [SerializeField] private float slotSpacing = 1.5f;
    [SerializeField] private Vector3 basePosition = new Vector3(0f, 1.5f, 0f);

    [SerializeField] private CinemachineCamera stageCinemachineCamera;
    [SerializeField] private Transform cameraLookTarget;
    [SerializeField] private LayerMask stageClickableMask;

    private int currentSelectedNavigationIndex = -1;
    private bool selectCharacterStageIsActive = false;
    private List<CharacterPreviewSlot> previewSlots = new List<CharacterPreviewSlot>();

    private void Awake()
    {
        GenerateSlots();
        AssignNavigationIndexes();
    }

    private void Start()
    {
        SyncExistingCharactersToSlots();

        GameInput.Instance.OnToggleSelectCharacterStage += GameInput_OnToggleSelectCharacterStage;
        GameInput.Instance.OnCycleNextCharacter += GameInput_OnCycleNextCharacter;
        GameInput.Instance.OnCyclePreviousCharacter += GameInput_OnCyclePreviousCharacter;
        GameInput.Instance.OnClickPreviewCharacter += GameInput_OnClickPreviewCharacter;
    }

    private void GameInput_OnClickPreviewCharacter()
    {
        if (!selectCharacterStageIsActive) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 999f, stageClickableMask))
        {
            CharacterPreviewSlot clickedSlot = hit.collider.GetComponentInParent<CharacterPreviewSlot>();
            if (clickedSlot == null) return;
            if (!clickedSlot.HasCharacterAssigned()) return;

            currentSelectedNavigationIndex = clickedSlot.NavigationIndex;
            stageCinemachineCamera.Follow = clickedSlot.transform;
        }
    }

    private void GameInput_OnCyclePreviousCharacter()
    {
        if (!selectCharacterStageIsActive) return;

        CyclePreviousCharacter();
    }

    private void GameInput_OnCycleNextCharacter()
    {
        if (!selectCharacterStageIsActive) return;

        CycleNextCharacter();
    }

    private void GameInput_OnToggleSelectCharacterStage()
    {
        if (selectCharacterStageIsActive)
        {
            // toggle off
            stageCinemachineCamera.Priority = 0;
            selectCharacterStageIsActive = false;
        } else
        {
            // toggle on
            stageCinemachineCamera.Priority = 1000;
            selectCharacterStageIsActive = true;

            CharacterPreviewSlot camTarget = GetInitialCameraTargetSlot();
            if (camTarget != null)
            {
                currentSelectedNavigationIndex = camTarget.NavigationIndex;
                stageCinemachineCamera.Follow = camTarget.transform;
            }
        }
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void GenerateSlots()
    {
        for (int i = 0; i < maxPreviewSlots; i++)
        {
            CharacterPreviewSlot slot = Instantiate(characterPreviewSlotPrefab, characterPreviewContainer);

            float zOffset = GetZOffset(i);
            slot.transform.localPosition = basePosition + new Vector3(0f, 0f, zOffset);
            slot.SetSlotIndex(i);

            previewSlots.Add(slot);
        }
    }

    private float GetZOffset(int index)
    {
        if (index == 0)
            return 0f;

        int step = (index + 1) / 2;
        float offset = step * slotSpacing;

        if (index % 2 == 0)
            offset = -offset;

        return offset;
    }

    private void SubscribeToEvents()
    {
        if (CommunityManager.Instance == null) return;

        CommunityManager.Instance.OnPlayableCharacterSpawned += CommunityManager_OnPlayableCharacterSpawned;
        CommunityManager.Instance.OnPlayableCharacterRemoved += CommunityManager_OnPlayableCharacterRemoved;
    }

    private void UnsubscribeFromEvents()
    {
        if (CommunityManager.Instance == null) return;

        CommunityManager.Instance.OnPlayableCharacterSpawned -= CommunityManager_OnPlayableCharacterSpawned;
        CommunityManager.Instance.OnPlayableCharacterRemoved -= CommunityManager_OnPlayableCharacterRemoved;
    }

    private CharacterPreviewSlot FindSlot(bool wantEmpty)
    {
        for (int i = 0; i < previewSlots.Count; i++)
        {
            bool isEmpty = !previewSlots[i].HasCharacterAssigned();

            if (isEmpty == wantEmpty)
            {
                return previewSlots[i];
            }
        }

        return null;
    }

    private CharacterPreviewSlot FindSlotWithPlayableCharacter(PlayableCharacter playableCharacter)
    {
        for (int i = 0; i < previewSlots.Count; i++)
        {
            if (previewSlots[i].HasCharacterAssigned() &&
                previewSlots[i].GetAssignedPlayableCharacter() == playableCharacter)
            {
                return previewSlots[i];
            }
        }

        return null;
    }

    private void RemoveCharacterFromSlot(PlayableCharacter playableCharacter)
    {
        if (playableCharacter == null) return;

        CharacterPreviewSlot slot = FindSlotWithPlayableCharacter(playableCharacter);
        if (slot == null) return;

        slot.ClearCharacter();
    }

    private void AddCharacterToFirstEmptySlot(PlayableCharacter playableCharacter)
    {
        if (playableCharacter == null) return;

        if (FindSlotWithPlayableCharacter(playableCharacter) != null)
            return;

        CharacterPreviewSlot emptySlot = FindSlot(true);
        if (emptySlot == null)
        {
            Debug.LogWarning("Brak wolnych slotow.");
            return;
        }


        emptySlot.SetCharacter(playableCharacter, cameraLookTarget);
    }

    private void CommunityManager_OnPlayableCharacterSpawned(object sender, CommunityManager.OnPlayableCharacterSpawnedEventArgs e)
    {
        AddCharacterToFirstEmptySlot(e.playableCharacter);
    }

    private void CommunityManager_OnPlayableCharacterRemoved(object sender, CommunityManager.OnPlayableCharacterRemovedEventArgs e)
    {
        RemoveCharacterFromSlot(e.playableCharacter);
    }
    private void SyncExistingCharactersToSlots()
    {
        if (CommunityManager.Instance == null) return;


        Dictionary<string, PlayableCharacter> spawnedPlayableCharacterDictionary =
            CommunityManager.Instance.GetSpawnedPlayableCharacterDictionary();


        foreach (PlayableCharacter playableCharacter in spawnedPlayableCharacterDictionary.Values)
        {
            AddCharacterToFirstEmptySlot(playableCharacter);
        }
    }

    private void CycleNextCharacter()
    {
        CycleCharacter(1);
    }

    private void CyclePreviousCharacter()
    {
        CycleCharacter(-1);
    }

    private void CycleCharacter(int direction)
    {
        if (previewSlots.Count == 0) return;

        int startIndex = currentSelectedNavigationIndex;

        if (startIndex < 0 || startIndex >= previewSlots.Count)
        {
            CharacterPreviewSlot firstNotEmptySlot = FindSlot(false);
            if (firstNotEmptySlot == null) return;

            currentSelectedNavigationIndex = firstNotEmptySlot.NavigationIndex;
            stageCinemachineCamera.Follow = firstNotEmptySlot.transform;
            return;
        }

        int nextIndex = startIndex;

        for (int i = 0; i < previewSlots.Count; i++)
        {
            nextIndex += direction;

            if (nextIndex >= previewSlots.Count)
                nextIndex = 0;
            else if (nextIndex < 0)
                nextIndex = previewSlots.Count - 1;

            CharacterPreviewSlot slot = FindSlotByNavigationIndex(nextIndex);

            if (slot != null && slot.HasCharacterAssigned())
            {
                currentSelectedNavigationIndex = nextIndex;
                stageCinemachineCamera.Follow = slot.transform;
                return;
            }
        }
    }

    private CharacterPreviewSlot GetInitialCameraTargetSlot()
    {
        PlayableCharacter activePlayableCharacter = ActiveCharacterManager.Instance.GetActivePlayableCharacter();

        if (activePlayableCharacter != null)
        {
            CharacterPreviewSlot activeCharacterSlot = FindSlotWithPlayableCharacter(activePlayableCharacter);
            if (activeCharacterSlot != null)
            {
                return activeCharacterSlot;
            }
        }

        return FindSlot(false);
    }
    private void AssignNavigationIndexes()
    {
        List<CharacterPreviewSlot> sortedSlots = new List<CharacterPreviewSlot>(previewSlots);

        sortedSlots.Sort((a, b) =>
            a.transform.localPosition.z.CompareTo(b.transform.localPosition.z));

        for (int i = 0; i < sortedSlots.Count; i++)
        {
            sortedSlots[i].SetNavigationIndex(i);
        }
    }
    private CharacterPreviewSlot FindSlotByNavigationIndex(int navigationIndex)
    {
        for (int i = 0; i < previewSlots.Count; i++)
        {
            if (previewSlots[i].NavigationIndex == navigationIndex)
            {
                return previewSlots[i];
            }
        }

        return null;
    }
}