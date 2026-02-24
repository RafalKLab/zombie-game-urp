using System;
using System.Collections.Generic;
using UnityEngine;

public class CommunityManager : MonoBehaviour
{
    [SerializeField] private CharacterSO startingCharacterSO;

    public event EventHandler<OnPlayableCharacterSpawnedEventArgs> OnPlayableCharacterSpawned;
    public class OnPlayableCharacterSpawnedEventArgs : EventArgs
    {
        public PlayableCharacter playableCharacter;
    }

    public event EventHandler<OnPlayableCharacterRemovedEventArgs> OnPlayableCharacterRemoved;
    public class OnPlayableCharacterRemovedEventArgs : EventArgs
    {
        public PlayableCharacter playableCharacter;
    }

    public static CommunityManager Instance { get; private set; }

    // id => PlayableCharacter
    private Dictionary<string, PlayableCharacter> spawnedPlayableCharacterDictionary;

    private void Awake()
    {
        Instance = this;

        spawnedPlayableCharacterDictionary = new Dictionary<string, PlayableCharacter>();
    }

    private void Start()
    {
        CharacterDeathManager.Instance.OnCharacterKilled += CharacterDeathManager_OnCharacterKilled;

        if (startingCharacterSO != null)
        {
            SpawnCharacter(startingCharacterSO);
        }
    }

    private void CharacterDeathManager_OnCharacterKilled(object sender, CharacterDeathManager.CharacterKilledEventArgs e)
    {
        bool wasRemoved = spawnedPlayableCharacterDictionary.Remove(e.playableCharacter.GetInstanceGuid());

        if (wasRemoved)
        {
            OnPlayableCharacterRemoved?.Invoke(this, new OnPlayableCharacterRemovedEventArgs
            {
                playableCharacter = e.playableCharacter
            });
        }
    }

    public Dictionary<string, PlayableCharacter> GetSpawnedPlayableCharacterDictionary()
    {
        return spawnedPlayableCharacterDictionary;
    }

    public void SpawnCharacter(CharacterSO characterSO)
    {
        Vector3 spawnPosition = transform.position;

        Transform characterTransform = Instantiate(characterSO.prefab, spawnPosition, Quaternion.identity);
        CharacterCore characterCore = characterTransform.GetComponent<CharacterCore>();
        if (characterCore == null)
        {
            Debug.LogError($"Prefab {characterSO.name} missing CharacterCore!");
            Destroy(characterTransform.gameObject);
            return;
        }

        PlayableCharacter playableCharacter = characterTransform.gameObject.AddComponent<PlayableCharacter>();

        string id;
        do
        {
            id = Guid.NewGuid().ToString("N");
        } while (spawnedPlayableCharacterDictionary.ContainsKey(id));

        playableCharacter.SetInstanceGuid(id);

        spawnedPlayableCharacterDictionary.Add(id, playableCharacter);

        CharacterDeathManager.Instance.SubscribeToPlayableCharacterOnKilled(playableCharacter);

        OnPlayableCharacterSpawned?.Invoke(this,
            new OnPlayableCharacterSpawnedEventArgs { playableCharacter = playableCharacter });
    }

    public bool TryAddToCommunity(CharacterCore characterCore)
    {
        if (characterCore == null) return false;

        var playableCharacter = characterCore.GetComponent<PlayableCharacter>();
        if (playableCharacter == null)
            playableCharacter = characterCore.gameObject.AddComponent<PlayableCharacter>();

        string existingId = playableCharacter.GetInstanceGuid();
        if (!string.IsNullOrEmpty(existingId) && spawnedPlayableCharacterDictionary.ContainsKey(existingId))
            return false;

        string id;
        do { id = Guid.NewGuid().ToString("N"); }
        while (spawnedPlayableCharacterDictionary.ContainsKey(id));

        playableCharacter.SetInstanceGuid(id);

        playableCharacter.gameObject.GetComponent<AiTarget>().SetFaction(Faction.Player);
        playableCharacter.gameObject.GetComponent<CharacterAi>().RefreshBase();

        spawnedPlayableCharacterDictionary.Add(id, playableCharacter);

        CharacterDeathManager.Instance.SubscribeToPlayableCharacterOnKilled(playableCharacter);

        OnPlayableCharacterSpawned?.Invoke(this,
            new OnPlayableCharacterSpawnedEventArgs { playableCharacter = playableCharacter });

        return true;
    }
}
