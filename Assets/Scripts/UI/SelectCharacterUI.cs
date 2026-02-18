using System.Collections.Generic;
using UnityEngine;

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
        if (PlayerSpawner.Instance == null) return;

        PlayerSpawner.Instance.OnPlayableCharacterSpawned += PlayerSpawner_OnPlayableCharacterSpawned;
        PlayerSpawner.Instance.OnPlayableCharacterRemoved += PlayerSpawner_OnPlayableCharacterRemoved;
    }

    private void UnsubscribeFromSpawnerEvents()
    {
        if (PlayerSpawner.Instance == null) return;

        PlayerSpawner.Instance.OnPlayableCharacterSpawned -= PlayerSpawner_OnPlayableCharacterSpawned;
        PlayerSpawner.Instance.OnPlayableCharacterRemoved -= PlayerSpawner_OnPlayableCharacterRemoved;
    }

    private void RebuildCharacterButtons()
    {
        ClearAllCharacterButtons();

        if (PlayerSpawner.Instance == null) return;

        Dictionary<string, PlayableCharacter> spawnedPlayableCharacterDictionary =
            PlayerSpawner.Instance.GetSpawnedPlayableCharacterDictionary();

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

    private void PlayerSpawner_OnPlayableCharacterSpawned(object sender, PlayerSpawner.OnPlayableCharacterSpawnedEventArgs e)
    {
        AddCharacterButton(e.playableCharacter);
    }

    private void PlayerSpawner_OnPlayableCharacterRemoved(object sender, PlayerSpawner.OnPlayableCharacterRemovedEventArgs e)
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
        }
    }

    public void Activate() => gameObject.SetActive(true);
    public void Deactivate() => gameObject.SetActive(false);
}
