using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverviewCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ActiveCharacterManager activeCharacterManager;

    [SerializeField] private CinemachineCamera overviewCamera;
    [SerializeField] private CinemachineFollow overviewCameraFollow;
    [SerializeField] private Transform target;

    [Header("Settings")]
    [SerializeField] private float cameraMovementSpeed = 8f;
    [SerializeField] private float sprintSpeed = 16f;
    [SerializeField] private float rotationSpeed = 120f;

    [SerializeField] private float zoomSpeed = 0.02f;
    [SerializeField] private float minDistance = 6f;
    [SerializeField] private float maxDistance = 35f;


    private Vector2 moveInput;
    private float rotateInput;
    private bool isSprinting;
    private float zoomInput;

    private void Update()
    {
        if (target == null) return;
        if (!overviewCamera.IsLive) return;

        Vector3 forward = target.forward;
        forward.y = 0f; 
        forward.Normalize();

        Vector3 right = target.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = right * moveInput.x + forward * moveInput.y;
        if (move.sqrMagnitude > 1f)
            move.Normalize();


        float speed = isSprinting ? sprintSpeed : cameraMovementSpeed;
        target.position += move * speed * Time.deltaTime;

        target.Rotate(Vector3.up, rotateInput * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void LateUpdate()
    {
        if (target == null) return;
        if (!overviewCamera.IsLive) return;

        if (zoomInput != 0f)
        {
            Vector3 offset = overviewCameraFollow.FollowOffset;

            offset.y -= zoomInput * zoomSpeed;
            offset.y = Mathf.Clamp(offset.y, minDistance, maxDistance);

            overviewCameraFollow.FollowOffset = offset;

            zoomInput = 0f;
        }
    }


    public void OnMove(InputAction.CallbackContext callbackContext)
    {
        moveInput = callbackContext.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext callbackContext)
    {
        isSprinting = callbackContext.ReadValueAsButton();
    }

    public void OnRotate(InputAction.CallbackContext callbackContext)
    {
        rotateInput = callbackContext.ReadValue<float>();
    }
    public void OnZoom(InputAction.CallbackContext callbackContext)
    {
        Debug.Log(callbackContext.ReadValue<float>());
        zoomInput = callbackContext.ReadValue<float>();
    }
}
