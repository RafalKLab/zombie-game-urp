using UnityEngine;

public class InteractableActionDemolish : MonoBehaviour, IInteractableAction
{
    [SerializeField] private int priority = 0;
    [SerializeField] private string executePrompt = "Demolish";
    [SerializeField] private int interactionsToDemolish = 4;
    [SerializeField] private ItemDefinitionSO rewardItem;
    [SerializeField] private int rewardAmount = 1;

    private int currentDemolishProgress = 0;
    private bool isDepleted = false;
    public int Priority => priority;

    public bool IsDepleted => isDepleted;

    public bool CanExecute(Interactor interactor)
    {
        return true;
    }

    public bool Execute(Interactor interactor)
    {
        return true;
    }

    public string GetExecutePrompt(Interactor interactor)
    {
        return executePrompt;
    }
}
