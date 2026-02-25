using System.Collections.Generic;
using UnityEngine;

public class AssignCharacterMenu : MonoBehaviour
{
    [SerializeField] private AssignCharacterButtonUI assignCharacterButtonUIPrefab;

    private List<AssignCharacterButtonUI> cachedButtons = new ();
    private CommunityManager communityManager;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Show(DefenseAssignContext defenseAssignContext)
    {
        communityManager = CommunityManager.Instance;
        gameObject.SetActive(true);
        ClearButtons();
        LoadButtons(defenseAssignContext);
    }

    public void Hide()
    {
        ClearButtons();
        gameObject.SetActive(false);
    }


    public void LoadButtons(DefenseAssignContext defenseAssignContext)
    {
        List<PlayableCharacter> playableCharacters = communityManager.GetIdlePlayableCharacters();

        foreach (PlayableCharacter playableCharacter in playableCharacters)
        {
            AssignCharacterButtonUI assignCharacterButtonUI = Instantiate(assignCharacterButtonUIPrefab, transform);
            assignCharacterButtonUI.Init(playableCharacter, defenseAssignContext);
            cachedButtons.Add(assignCharacterButtonUI);
        }
    }

    private void ClearButtons()
    {
        foreach (AssignCharacterButtonUI assignCharacterButtonUI in cachedButtons)
        {
            assignCharacterButtonUI.DestorySelf();
        }

        cachedButtons = new();
    }
}
