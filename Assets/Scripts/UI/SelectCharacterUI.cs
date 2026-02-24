using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectCharacterUI : MonoBehaviour
{
    [SerializeField] private Transform selectCharacterButtonTemplate;

    private readonly Dictionary<PlayableCharacter, SelectCharacterButtonUI> buttonByPlayableCharacter
        = new Dictionary<PlayableCharacter, SelectCharacterButtonUI>();

    private void Awake()
    {
        selectCharacterButtonTemplate.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        SubscribeToSpawnerEvents();
        RebuildCharacterButtons();
    }

    private void OnDisable()
    {
        UnsubscribeFromSpawnerEvents();
        ClearAllCharacterButtons();
    }

    private void SubscribeToSpawnerEvents()
    {
        if (CommunityManager.Instance == null) return;

        CommunityManager.Instance.OnPlayableCharacterSpawned += CommunityManager_OnPlayableCharacterSpawned;
        CommunityManager.Instance.OnPlayableCharacterRemoved += CommunityManager_OnPlayableCharacterRemoved;
    }

    private void UnsubscribeFromSpawnerEvents()
    {
        if (CommunityManager.Instance == null) return;

        CommunityManager.Instance.OnPlayableCharacterSpawned -= CommunityManager_OnPlayableCharacterSpawned;
        CommunityManager.Instance.OnPlayableCharacterRemoved -= CommunityManager_OnPlayableCharacterRemoved;
    }

    private void RebuildCharacterButtons()
    {
        ClearAllCharacterButtons();

        if (CommunityManager.Instance == null) return;

        Dictionary<string, PlayableCharacter> spawnedPlayableCharacterDictionary =
            CommunityManager.Instance.GetSpawnedPlayableCharacterDictionary();

        foreach (PlayableCharacter playableCharacter in spawnedPlayableCharacterDictionary.Values)
        {
            AddCharacterButton(playableCharacter);
        }
    }

    private void ClearAllCharacterButtons()
    {
        foreach (KeyValuePair<PlayableCharacter, SelectCharacterButtonUI> entry in buttonByPlayableCharacter)
        {
            SelectCharacterButtonUI selectCharacterButtonUI = entry.Value;
            if (selectCharacterButtonUI != null)
            {
                Destroy(selectCharacterButtonUI.gameObject);
            }
        }

        buttonByPlayableCharacter.Clear();
    }

    private void CommunityManager_OnPlayableCharacterSpawned(object sender, CommunityManager.OnPlayableCharacterSpawnedEventArgs e)
    {
        AddCharacterButton(e.playableCharacter);
    }

    private void CommunityManager_OnPlayableCharacterRemoved(object sender, CommunityManager.OnPlayableCharacterRemovedEventArgs e)
    {
        RemoveCharacterButton(e.playableCharacter);
    }

    private void AddCharacterButton(PlayableCharacter playableCharacter)
    {
        if (playableCharacter == null) return;
        if (buttonByPlayableCharacter.ContainsKey(playableCharacter)) return;

        Transform selectCharacterButtonTransform = Instantiate(selectCharacterButtonTemplate, transform);
        selectCharacterButtonTransform.gameObject.SetActive(true);

        SelectCharacterButtonUI selectCharacterButtonUI =
            selectCharacterButtonTransform.GetComponent<SelectCharacterButtonUI>();

        selectCharacterButtonUI.SetData(playableCharacter);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        buttonByPlayableCharacter[playableCharacter] = selectCharacterButtonUI;
    }

    private void RemoveCharacterButton(PlayableCharacter playableCharacter)
    {
        if (playableCharacter == null) return;

        if (buttonByPlayableCharacter.TryGetValue(playableCharacter, out SelectCharacterButtonUI selectCharacterButtonUI))
        {
            if (selectCharacterButtonUI != null)
            {
                Destroy(selectCharacterButtonUI.gameObject);
            }

            buttonByPlayableCharacter.Remove(playableCharacter);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }
    }

    public void Activate() => gameObject.SetActive(true);
    public void Deactivate() => gameObject.SetActive(false);
}
