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

    public class OnInteractActionSlotEventArgs : EventArgs
    {
        public int index;
    }

    private InputActions inputActions;

    private void Awake()
    {
        Instance = this;

        inputActions = new InputActions();
        inputActions.Player.Enable();

        inputActions.Player.RightClick.performed += RightClick_performed;
        inputActions.Player.LeftClick.performed += LeftClick_performed;
        inputActions.Player.Interact.performed += Interact_performed;
        inputActions.Player.InteractActionSlot.performed += InteractActionSlot_performed;
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
