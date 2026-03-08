using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnMouseRightClick;
    public event EventHandler OnMouseLeftClick;
    public event EventHandler OnInteractClick;
    public event EventHandler<OnInteractActionSlotEventArgs> OnInteractActionSlot;

    public event Action OnToggleSelectCharacterStage;
    public event Action OnCycleNextCharacter;
    public event Action OnCyclePreviousCharacter;
    public event Action OnClickPreviewCharacter;

    public class OnInteractActionSlotEventArgs : EventArgs
    {
        public int index;
    }

    public event Action OnInventoryOpen;
    public event Action OnInventoryClose;

    private InputActions inputActions;

    private void Awake()
    {
        Instance = this;

        inputActions = new InputActions();
        inputActions.Player.Enable();
        inputActions.SelectCharacterStage.Enable();

        inputActions.Player.RightClick.performed += RightClick_performed;
        inputActions.Player.LeftClick.performed += LeftClick_performed;
        inputActions.Player.Interact.performed += Interact_performed;
        inputActions.Player.InteractActionSlot.performed += InteractActionSlot_performed;

        inputActions.Player.InventoryOpen.performed += InventoryOpen_performed;
        inputActions.Player.InventoryClose.performed += InventoryClose_performed;

        inputActions.SelectCharacterStage.ToggleSelectCharacterStage.performed += ToggleSelectCharacterStage_performed;
        inputActions.SelectCharacterStage.CycleNextCharacter.performed += CycleNextCharacter_performed;
        inputActions.SelectCharacterStage.CyclePreviousCharacter.performed += CyclePreviousCharacter_performed;
        inputActions.SelectCharacterStage.ClickPreviewCharacter.performed += ClickPreviewCharacter_performed;
    }

    private void ClickPreviewCharacter_performed(InputAction.CallbackContext obj)
    {
        OnClickPreviewCharacter?.Invoke();
    }

    private void CyclePreviousCharacter_performed(InputAction.CallbackContext obj)
    {
        OnCyclePreviousCharacter?.Invoke();
    }

    private void CycleNextCharacter_performed(InputAction.CallbackContext obj)
    {
        OnCycleNextCharacter?.Invoke();
    }

    private void ToggleSelectCharacterStage_performed(InputAction.CallbackContext obj)
    {
        OnToggleSelectCharacterStage?.Invoke();
    }

    private void InventoryOpen_performed(InputAction.CallbackContext obj)
    {
        OnInventoryOpen?.Invoke();
    }

    private void InventoryClose_performed(InputAction.CallbackContext obj)
    {
        OnInventoryClose?.Invoke();
    }

    private void InteractActionSlot_performed(InputAction.CallbackContext obj)
    {
        if (obj.control is not KeyControl keyControl)
            return;

        var key = keyControl.keyCode;

        int index = -1;

        if (key >= Key.Digit1 && key <= Key.Digit9)
            index = key - Key.Digit1;

        else if (key >= Key.Numpad1 && key <= Key.Numpad9)
            index = key - Key.Numpad1;

        if (index >= 0 && index <= 8)
        {
            OnInteractActionSlot?.Invoke(this, new OnInteractActionSlotEventArgs { index = index });
        }
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractClick?.Invoke(this, EventArgs.Empty);
    }

    private void LeftClick_performed(InputAction.CallbackContext obj)
    {
        OnMouseLeftClick?.Invoke(this, EventArgs.Empty);
    }

    private void RightClick_performed(InputAction.CallbackContext obj)
    {
        OnMouseRightClick?.Invoke(this, EventArgs.Empty);
    }
}
