using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class BaseViewUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseDetailSlotViewUI baseDetailSlotViewUI;
    [SerializeField] private BaseViewUiCameraController baseViewUiCameraController;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private BaseManager baseManager;
    [SerializeField] private List<BaseViewSlotUI> baseViewSlotUIList = new();

    public bool IsActive => isActive;

    private bool isActive;
    private bool isInitialized;
    private Coroutine showLabelsCoroutine;

    #region Initialization

    private void Initialize()
    {
        if (isInitialized) return;

        foreach (BaseViewSlotUI baseViewSlotUI in baseViewSlotUIList)
        {
            baseViewSlotUI.SetBaseManager(baseManager);
            baseViewSlotUI.InitButton(PreviewBaseSlotAction);
        }

        isInitialized = true;
    }

    #endregion

    #region Public API

    public void Show()
    {
        Initialize();

        isActive = true;
        gameObject.SetActive(true);

        ShowLabels();
    }

    public void Hide()
    {
        isActive = false;

        StopShowLabelsRoutine();
        HideBaseDetailSlotViewUIInternal();
        HideLabels();

        baseViewUiCameraController.DisableAllCameras();

        gameObject.SetActive(false);
    }

    #endregion

    #region Labels

    private void ShowLabels()
    {
        StopShowLabelsRoutine();

        foreach (BaseViewSlotUI baseViewSlotUI in baseViewSlotUIList)
        {
            baseViewSlotUI.UpdateVisual();
        }

        if (!isActive) return;

        showLabelsCoroutine = StartCoroutine(ShowLabelsRoutine());
    }

    private void HideLabels()
    {
        StopShowLabelsRoutine();

        foreach (BaseViewSlotUI baseViewSlotUI in baseViewSlotUIList)
        {
            baseViewSlotUI.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowLabelsRoutine()
    {
        yield return null;

        while (cinemachineBrain != null && cinemachineBrain.ActiveBlend != null)
        {
            yield return null;
        }

        if (!isActive)
        {
            showLabelsCoroutine = null;
            yield break;
        }

        foreach (BaseViewSlotUI baseViewSlotUI in baseViewSlotUIList)
        {
            baseViewSlotUI.gameObject.SetActive(true);
        }

        showLabelsCoroutine = null;
    }

    private void StopShowLabelsRoutine()
    {
        if (showLabelsCoroutine == null) return;

        StopCoroutine(showLabelsCoroutine);
        showLabelsCoroutine = null;
    }

    #endregion

    #region Slot Preview

    private void PreviewBaseSlotAction(BaseSlotPoint baseSlotPoint)
    {
        HideLabels();
        baseViewUiCameraController.ShowSlotCamera(baseSlotPoint);
        ShowBaseDetailSlotViewUI(baseSlotPoint);
    }

    #endregion

    #region Base Detail Slot View

    private void ShowBaseDetailSlotViewUI(BaseSlotPoint baseSlotPoint)
    {
        if (baseDetailSlotViewUI == null) return;

        baseDetailSlotViewUI.Show(baseSlotPoint, HideBaseDetailSlotViewUI);
    }

    private void HideBaseDetailSlotViewUI()
    {
        if (!isActive) return;

        HideBaseDetailSlotViewUIInternal();
        baseViewUiCameraController.ShowDefaultCamera();
        ShowLabels();
    }

    private void HideBaseDetailSlotViewUIInternal()
    {
        if (baseDetailSlotViewUI == null) return;

        baseDetailSlotViewUI.Hide();
    }

    #endregion
}