public interface IInteractableAction
{
    int Priority { get; }
    string GetExecutePrompt(Interactor interactor);
    bool CanExecute(Interactor interactor);
    bool Execute(Interactor interactor);
}