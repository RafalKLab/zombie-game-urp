using UnityEngine;

public class InteractableActionClose : MonoBehaviour, IInteractableAction
{
    public int Priority => -999;

    public bool IsDepleted => false;

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
        return "Close";
    }
}
