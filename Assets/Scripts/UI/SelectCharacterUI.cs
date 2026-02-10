using UnityEngine;

public class SelectCharacterUI : MonoBehaviour
{
    [SerializeField] Transform selectCharacterButtonTemplate;

    private void Awake()
    {
        selectCharacterButtonTemplate.gameObject.SetActive(false);

        PlayerSpawner.Instance.OnPlayableCharacterSpawned += PlayerSpawner_OnPlayableCharacterSpawned;
    }

    private void PlayerSpawner_OnPlayableCharacterSpawned(object sender, PlayerSpawner.OnPlayableCharacterSpawnedEventArgs e)
    {
        CreateSelectCharacterButton(e.playableCharacter);
    }

    private void CreateSelectCharacterButton(PlayableCharacter playableCharacter)
    {
        Transform selectCharacterButton = Instantiate(selectCharacterButtonTemplate, transform);
        selectCharacterButton.gameObject.SetActive(true);

        selectCharacterButton.GetComponent<SelectCharacterButtonUI>().SetData(playableCharacter);
    }
}
