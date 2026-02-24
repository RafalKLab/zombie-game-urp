using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AiTarget))]
[RequireComponent(typeof(CharacterCore))]
public class InteractableActionInviteToCommunity : MonoBehaviour, IInteractableAction
{
    [SerializeField] private int priority = 1;
    [SerializeField] private string promt = "F - Invite to community";

    private Faction faction;
    private bool destoryPending = false;

    private void Start()
    {
        faction = GetComponent<AiTarget>().GetFaction();
    }

    private bool isDepleted = false;

    public int Priority => priority;

    public bool IsDepleted => isDepleted;

    public bool CanExecute(Interactor interactor)
    {
        if (interactor == null) return false;
        if (destoryPending) return false;
        return faction == Faction.Neutral;
    }

    public bool Execute(Interactor interactor)
    {
        bool success = CommunityManager.Instance
            .TryAddToCommunity(GetComponent<CharacterCore>());

        if (success)
        {
            destoryPending = true;
            StartCoroutine(RemoveActionNextFrame());
        }

        return success;
    }

    public string GetExecutePrompt(Interactor interactor)
    {
        return promt;
    }

    private IEnumerator RemoveActionNextFrame()
    {
        yield return null;

        Destroy(this);
    }
}
