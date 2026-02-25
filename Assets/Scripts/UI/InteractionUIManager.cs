using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionUIManager : MonoBehaviour
{
    [SerializeField] private Transform interactionIndicator;
    [SerializeField] private TextMeshProUGUI interactionIndicatorPromt;

    [Header("Multi options UI")]
    [SerializeField] private Transform actionsPanel;
    [SerializeField] private InteractableActionItemUI actionItemPrefab;

    private readonly List<InteractableActionItemUI> itemPool = new();

    private bool menuOpen;
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
        activeInteractor.OnInteractNeedsChoice += OnInteractNeedsChoice;

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
        {
            activeInteractor.OnInteractableChange -= OnInteractableChange;
            activeInteractor.OnInteractNeedsChoice -= OnInteractNeedsChoice;
        }

        activeInteractor = null;
        menuOpen = false;
    }

    private void OnInteractableChange(object sender, Interactor.OnInteractableChangeEventArgs e)
    {
        menuOpen = false;

        if (actionsPanel != null)
            actionsPanel.gameObject.SetActive(false);

        if (e.interactable == null || activeInteractor == null)
        {
            Hide();
            return;
        }

        UpdateCanvasVisual(e.interactable);
    }

    private void OnInteractNeedsChoice(object sender, Interactor.OnInteractNeedsChoiceEventArgs e)
    {
        if (activeInteractor == null) return;
        if (e.interactable == null) return;

        menuOpen = true;
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

        if (!menuOpen)
        {
            Show();
        }
        else
        {
            if (interactionIndicator != null)
                interactionIndicator.gameObject.SetActive(false);
        }

        RenderActions(interactable);
    }

    private void Show()
    {
        if (interactionIndicator != null)
            interactionIndicator.gameObject.SetActive(true);
    }

    private void Hide()
    {
        if (interactionIndicator != null)
            interactionIndicator.gameObject.SetActive(false);

        if (actionsPanel != null)
            actionsPanel.gameObject.SetActive(false);
    }

    private void RenderActions(IInteractable interactable)
    {
        if (actionsPanel == null || actionItemPrefab == null) return;
        if (activeInteractor == null) return;

        var actions = interactable.GetActions(activeInteractor);
        if (actions == null || actions.Count == 0)
        {
            actionsPanel.gameObject.SetActive(false);
            return;
        }

        // Policz ile wykonalnych
        int executable = 0;
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i] != null && actions[i].CanExecute(activeInteractor))
                executable++;

            if (executable > 1) break;
        }

        if (!menuOpen)
        {
            actionsPanel.gameObject.SetActive(false);
            return;
        }

        //if (executable <= 1)
        //{
        //    menuOpen = false;
        //    actionsPanel.gameObject.SetActive(false);
        //    return;
        //}

        actionsPanel.gameObject.SetActive(true);

        EnsurePoolSize(actions.Count);

        Transform anchor = interactable.GetUIAnchor();
        if (anchor != null)
            actionsPanel.position = anchor.position;

        for (int i = 0; i < itemPool.Count; i++)
        {
            bool active = i < actions.Count;
            itemPool[i].gameObject.SetActive(active);
            if (!active) continue;

            var action = actions[i];

            string label = action != null
                ? action.GetExecutePrompt(activeInteractor)
                : "<missing action>";

            itemPool[i].SetText($"{i + 1}. {label}");

            bool can = action != null && action.CanExecute(activeInteractor);
            itemPool[i].SetEnabledVisual(can);
        }
    }

    private void EnsurePoolSize(int needed)
    {
        while (itemPool.Count < needed)
        {
            var item = Instantiate(actionItemPrefab, actionsPanel);
            itemPool.Add(item);
        }
    }
}