using UnityEngine;

public class MainOverviewUI : MonoBehaviour
{
    [Header("Gameplay UI")]
    [SerializeField] private ResourcesUI resourcesUI;
    [SerializeField] private DefenseBaseUI defenseBaseUI;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private SelectedCharacterUI selectedCharacterUI;
    [SerializeField] private AssignCharacterMenu assignCharacterMenu;

    [Header("Overview UI")]
    [SerializeField] private SelectCharacterStageUI selectCharacterStageUI;
    [SerializeField] private BaseViewUI baseViewUI;

    [Header("Overview Logic")]
    [SerializeField] private SelectCharacterStage selectCharacterStage;

    [Header("Shared UI")]
    [SerializeField] private Transform navigation;
    [SerializeField] private NavButtonUI charactersNavButtonUI;
    [SerializeField] private NavButtonUI baseViewNavButtonUI;


    private void Start()
    {
        HideAllOverviewUi();
        ShowGameplayUi();
    }

    public void ToggleUiOverview(UiOverviewType uiOverviewType)
    {
        switch (uiOverviewType)
        {
            case UiOverviewType.Characters:
                ToggleCharacterSelection();
                break;

            case UiOverviewType.BaseView:
                ToggleBaseView();
                break;
        }
    }

    public void ToggleCharacterSelection()
    {
        if (selectCharacterStage.IsActive)
        {
            HideAll();
            return;
        }

        ShowCharacterSelectStage();
    }

    public void ToggleBaseView()
    {
        if (baseViewUI.IsActive)
        {
            HideAll();
            return;
        }

        ShowBaseView();
    }

    public void ShowCharacterSelectStage()
    {
        HideAllOverviewUi();
        HideGameplayUi();

        selectCharacterStage.Show();
        selectCharacterStageUI.gameObject.SetActive(true);
        navigation.gameObject.SetActive(true);

        SelectNavigationButton(UiOverviewType.Characters);
    }

    public void ShowBaseView()
    {
        HideAllOverviewUi();
        HideGameplayUi();

        baseViewUI.Show();
        navigation.gameObject.SetActive(true);

        SelectNavigationButton(UiOverviewType.BaseView);
    }

    public void HideAll()
    {
        HideAllOverviewUi();
        ShowGameplayUi();

        ResetNavigationButtons();
    }

    private void HideAllOverviewUi()
    {
        selectCharacterStage.Hide();
        selectCharacterStageUI.gameObject.SetActive(false);

        baseViewUI.Hide();

        navigation.gameObject.SetActive(false);
    }

    public void HideGameplayUi()
    {
        resourcesUI.gameObject.SetActive(false);
        defenseBaseUI.gameObject.SetActive(false);

        inventoryUI.HideAll();
        selectedCharacterUI.Deactivate();
        assignCharacterMenu.Hide();
    }

    public void ShowGameplayUi()
    {
        navigation.gameObject.SetActive(false);

        resourcesUI.gameObject.SetActive(true);
        defenseBaseUI.gameObject.SetActive(true);

        PlayableCharacter activePlayableCharacter = ActiveCharacterManager.Instance.GetActivePlayableCharacter();
        if (activePlayableCharacter != null)
        {
            selectedCharacterUI.Activate(activePlayableCharacter);
        }
    }

    private void SelectNavigationButton(UiOverviewType type)
    {
        ResetNavigationButtons();

        switch (type)
        {
            case UiOverviewType.Characters:
                charactersNavButtonUI.Select();
                break;

            case UiOverviewType.BaseView:
                baseViewNavButtonUI.Select();
                break;
        }
    }

    private void ResetNavigationButtons()
    {
        charactersNavButtonUI.Unselect();
        baseViewNavButtonUI.Unselect();
    }
}

public enum UiOverviewType
{
    Characters,
    BaseView,
}