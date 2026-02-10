using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnMouseRightClick;
    public event EventHandler OnMouseLeftClick;

    private InputActions inputActions;

    private void Awake()
    {
        Instance = this;

        inputActions = new InputActions();
        inputActions.Player.Enable();

        inputActions.Player.RightClick.performed += LeftClick_performed;
        inputActions.Player.LeftClick.performed += LeftClick_performed1;
    }

    private void LeftClick_performed1(InputAction.CallbackContext obj)
    {
        OnMouseLeftClick?.Invoke(this, EventArgs.Empty);
    }

    private void LeftClick_performed(InputAction.CallbackContext obj)
    {
        OnMouseRightClick?.Invoke(this, EventArgs.Empty);
    }
}
