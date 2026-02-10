using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public event EventHandler<OnPlayableCharacterSpawnedEventArgs> OnPlayableCharacterSpawned;
    public class OnPlayableCharacterSpawnedEventArgs : EventArgs
    {
        public PlayableCharacter playableCharacter;
    }

    public static PlayerSpawner Instance { get; private set; }

    // id => PlayableCharacter
    private Dictionary<string, PlayableCharacter> spawnedPlayableCharacterDictionary;

    private void Awake()
    {
        Instance = this;

        spawnedPlayableCharacterDictionary = new Dictionary<string, PlayableCharacter>();
    }

    private void Start()
    {
        CharacterDeathManager characterDeathManager = CharacterDeathManager.Instance;
        CharacterDeathManager.Instance.OnCharacterKilled += CharacterDeathManager_OnCharacterKilled;


        AvailableCharacterListSO availableCharacterList = Resources.Load<AvailableCharacterListSO>(typeof(AvailableCharacterListSO).Name);
        float positionOffset = 0f;

        foreach (CharacterSO characterSO in availableCharacterList.list)
        {
            Vector3 spawnPosition = transform.position;

            spawnPosition.x -= 1.5f;
            spawnPosition.x += 1.5f * positionOffset;

            Transform characterTransform = Instantiate(characterSO.prefab, spawnPosition, Quaternion.identity);
            CharacterCore characterCore = characterTransform.GetComponent<CharacterCore>();
            if (characterCore == null)
            {
                Debug.LogError($"Prefab {characterSO.name} missing CharacterCore!");
                Destroy(characterTransform.gameObject);
                continue;
            }

            PlayableCharacter playableCharacter = characterTransform.gameObject.AddComponent<PlayableCharacter>();

            // Generate ID (with a collision safety check)
            string id;
            do
            {
                id = Guid.NewGuid().ToString("N");
            } while (spawnedPlayableCharacterDictionary.ContainsKey(id));

            playableCharacter.SetInstanceGuid(id);

            spawnedPlayableCharacterDictionary.Add(id, playableCharacter);
            characterDeathManager.SubscribeToPlayableCharacterOnKilled(playableCharacter);

            OnPlayableCharacterSpawned?.Invoke(this, new OnPlayableCharacterSpawnedEventArgs { playableCharacter = playableCharacter });

            positionOffset++;
        }
    }

    private void CharacterDeathManager_OnCharacterKilled(object sender, CharacterDeathManager.CharacterKilledEventArgs e)
    {
        spawnedPlayableCharacterDictionary.Remove(e.playableCharacter.GetInstanceGuid());
    }

    public Dictionary<string, PlayableCharacter> GetSpawnedPlayableCharacterDictionary()
    {
        return spawnedPlayableCharacterDictionary;
    }
}
