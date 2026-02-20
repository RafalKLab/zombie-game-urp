using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    int Priority { get; }
    bool CanInteract(Interactor interactor);
    string GetInteractPrompt(Interactor interactor);
    InteractResult Interact(Interactor interactor);
    Transform GetUIAnchor();
    IReadOnlyList<IInteractableAction> GetActions(Interactor interactor);
}

public enum InteractResult
{
    None,
    Executed,
    NeedsChoice,
}