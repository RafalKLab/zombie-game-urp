using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnMouseRightClick;
    public event EventHandler OnMouseLeftClick;
    public event EventHandler OnInteractClick;

    private InputActions inputActions;

    private void Awake()
    {
        Instance = this;

        inputActions = new InputActions();
        inputActions.Player.Enable();

        inputActions.Player.RightClick.performed += RightClick_performed;
        inputActions.Player.LeftClick.performed += LeftClick_performed;
        inputActions.Player.Interact.performed += Interact_performed;
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
