using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class BaseViewUiCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private int activePriority = 100;
    [SerializeField] private int inactivePriority = 0;

    private void SetAllSlotCamerasInactive()
    {
        BaseManager baseManager = FactionBaseRegistry.Instance.GetBaseManagerByFaction(Faction.Player);

        foreach (BaseSlotPoint baseSlotPoint in baseManager.GetBaseSlotPointList())
        {
            if (baseSlotPoint == null) continue;

            CinemachineCamera slotCamera = baseSlotPoint.GetSlotViewCamera();
            if (slotCamera == null) continue;

            slotCamera.Priority = inactivePriority;
        }
    }

    public void ShowDefaultCamera()
    {
        DisableAllCameras();

        if (defaultCamera != null)
        {
            defaultCamera.Priority = activePriority;
            Debug.Log("[BaseViewUiCameraController] Default camera enabled", this);
        }
        else
        {
            Debug.LogWarning("[BaseViewUiCameraController] Default camera is NULL", this);
        }
    }

    public void ShowSlotCamera(BaseSlotPoint baseSlotPoint)
    {
        if (baseSlotPoint == null)
        {
            Debug.LogWarning("[BaseViewUiCameraController] ShowSlotCamera failed: baseSlotPoint is NULL", this);
            return;
        }

        CinemachineCamera slotCamera = baseSlotPoint.GetSlotViewCamera();
        if (slotCamera == null)
        {
            Debug.LogWarning($"[BaseViewUiCameraController] ShowSlotCamera failed: slot camera is NULL on '{baseSlotPoint.name}'", this);
            return;
        }

        DisableAllCameras();

        if (defaultCamera != null)
        {
            defaultCamera.Priority = inactivePriority;
        }

        slotCamera.Priority = activePriority;

        Debug.Log($"[BaseViewUiCameraController] Slot camera enabled: '{slotCamera.name}' for slot '{baseSlotPoint.name}'", this);
    }

    public void DisableAllCameras()
    {
        // disable slot cameras
        BaseManager baseManager = FactionBaseRegistry.Instance.GetBaseManagerByFaction(Faction.Player);
        foreach (BaseSlotPoint baseSlotPoint in baseManager.GetBaseSlotPointList())
        {
            if (baseSlotPoint == null) continue;

            CinemachineCamera slotCamera = baseSlotPoint.GetSlotViewCamera();
            if (slotCamera == null) continue;

            slotCamera.Priority = inactivePriority;
        }

        // disable default camera
        if (defaultCamera != null)
        {
            defaultCamera.Priority = inactivePriority;
        }
    }
}