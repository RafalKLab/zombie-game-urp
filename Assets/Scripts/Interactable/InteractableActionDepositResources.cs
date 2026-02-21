using UnityEngine;

public class InteractableActionDepositResources : MonoBehaviour, IInteractableAction
{
    [SerializeField] private int priority = 0;
    [SerializeField] private string executePrompt = "Deposit resources";
    [SerializeField] private ResourceManager resourceManager;

    public int Priority => priority;

    public bool CanExecute(Interactor interactor)
    {
        if (resourceManager == null)
            return false;

        if (interactor.resourceItemSO == null)
            return false;

        return true;
    }

    public bool Execute(Interactor interactor)
    {
        resourceManager.AddResourceAmount(interactor.resourceItemSO.resourceType, interactor.resourceItemSO.resourceUnits);

        interactor.resourceItemSO = null;

        return true;
    }

    public string GetExecutePrompt(Interactor interactor)
    {
        return executePrompt;
    }
}
