using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class InteractableActionOpenStorage : MonoBehaviour, IInteractableAction
{
    [SerializeField] private string executePrompt = "Open storage";

    private Inventory inventory;

    public int Priority => 0;

    public bool IsDepleted => false;
    private bool canExecute = false;

    private void Start()
    {
        if (TryGetComponent<Inventory>(out inventory))
        {
            canExecute = true;
        }
    }

    public bool CanExecute(Interactor interactor)
    {
        return canExecute;
    }

    public bool Execute(Interactor interactor)
    {
        if (!CanExecute(interactor)) return false;

        if (UiEventsManager.Instance != null)
        {
            UiEventsManager.Instance.RequestOpenStorage(inventory);

            return true;
        }

        return false;
    }

    public string GetExecutePrompt(Interactor interactor)
    {
        return executePrompt;
    }
}
