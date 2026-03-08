using UnityEngine;

public class CharacterPreviewSlot : MonoBehaviour
{
    [SerializeField] private Transform characterPreviewVisualContainer;

    private Animator animator;
    private PlayableCharacter playableCharacter;
    private Transform characterVisual;

    public int SlotIndex { get; private set; }
    public int NavigationIndex { get; private set; }

    public void SetSlotIndex(int slotIndex) => SlotIndex = slotIndex;
    public void SetNavigationIndex(int navigationIndex) => NavigationIndex = navigationIndex;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetCharacter(PlayableCharacter playableCharacter)
    {
        this.playableCharacter = playableCharacter;

        UpdateCharacterVisual();
    }

    private void UpdateCharacterVisual()
    {
        if (playableCharacter == null) return;

        CharacterSO characterSO = playableCharacter.GetCharacterCore().GetCharacterSO();
        if (characterSO == null) return;

        characterVisual = Instantiate(characterSO.mesh, characterPreviewVisualContainer);

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    public bool HasCharacterAssigned()
    {
        return playableCharacter != null;
    }

    public void ClearCharacter()
    {
        Destroy(characterVisual);
        characterVisual = null;
        playableCharacter = null;
    }

    public PlayableCharacter GetAssignedPlayableCharacter()
    {
        return playableCharacter; ;
    }
}
