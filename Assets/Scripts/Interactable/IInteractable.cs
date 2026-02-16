using UnityEngine;

public interface IInteractable
{
    int Priority { get; }
    bool CanInteract(Interactor interactor);
    string GetInteractPrompt(Interactor interactor);
    bool Interact(Interactor interactor);
    Transform GetUIAnchor();
}
