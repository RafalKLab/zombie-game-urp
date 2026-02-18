using TMPro;
using UnityEngine;

public class InteractionUIManager : MonoBehaviour
{
    [SerializeField] private Transform interactionIndicator;
    [SerializeField] private TextMeshProUGUI interactionIndicatorPromt;

    private Interactor activeInteractor;

    private void Start()
    {
        ActiveCharacterManager.Instance.OnActiveCharacterChanged += OnActiveCharacterChanged;
        Hide();
    }

    private void OnDestroy()
    {
        if (ActiveCharacterManager.Instance != null)
            ActiveCharacterManager.Instance.OnActiveCharacterChanged -= OnActiveCharacterChanged;

        Unsubscribe();
    }

    private void OnActiveCharacterChanged(object sender, ActiveCharacterManager.OnActiveCharacterChangedEventArgs e)
    {
        Unsubscribe();

        activeInteractor = e.playableCharacter != null
            ? e.playableCharacter.GetComponent<Interactor>()
            : null;

        if (activeInteractor == null)
        {
            Hide();
            return;
        }

        activeInteractor.OnInteractableChange += OnInteractableChange;

        var currentInteractable = activeInteractor.GetCurrentInteractable();
        if (currentInteractable != null)
        {
            UpdateCanvasVisual(currentInteractable);
        }
        else
        {
            Hide();
        }
    }

    private void Unsubscribe()
    {
        if (activeInteractor != null)
            activeInteractor.OnInteractableChange -= OnInteractableChange;

        activeInteractor = null;
    }

    private void OnInteractableChange(object sender, Interactor.OnInteractableChangeEventArgs e)
    {
        if (e.interactable == null || activeInteractor == null)
        {
            Hide();
            return;
        }

        UpdateCanvasVisual(e.interactable);
    }

    private void UpdateCanvasVisual(IInteractable interactable)
    {
        if (activeInteractor == null) return;
        if (interactionIndicator == null) return;
        if (interactionIndicatorPromt == null) return;

        Transform anchor = interactable.GetUIAnchor();
        if (anchor == null) return;

        interactionIndicatorPromt.text = interactable.GetInteractPrompt(activeInteractor);
        interactionIndicator.position = anchor.position;

        Show();
    }

    private void Show()
    {
        if (interactionIndicator != null) interactionIndicator.gameObject.SetActive(true);
    }

    private void Hide()
    {
        if (interactionIndicator != null) interactionIndicator.gameObject.SetActive(false);
    }
}
