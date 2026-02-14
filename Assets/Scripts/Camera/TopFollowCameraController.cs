using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopFollowCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera topFollowCamera;
    [SerializeField] private CinemachineFollow topFollowCameraFollow;
    [SerializeField] private Transform target;
    [SerializeField] private Transform overviewCameraTarget;
    
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 120f;

    [SerializeField] private float zoomSpeed = 0.02f;
    [SerializeField] private float zoomSmooth = 8f;
    [SerializeField] private float minDistance = 6f;
    [SerializeField] private float maxDistance = 35f;

    [SerializeField] private float followSmooth = 12f;

    private ActiveCharacterManager activeCharacterManager;
    private float desiredDistance;
    private float rotateInput;
    private float zoomInput;
    private bool canControl = false;
    private PlayableCharacter _activeCharacterCached;

    private void Start()
    {
        activeCharacterManager = ActiveCharacterManager.Instance;
        desiredDistance = topFollowCameraFollow.FollowOffset.y;
    }

    private void Update()
    {
        if (!CheckBeforeUpdate()) return;

        Vector3 desiredPosition = _activeCharacterCached.GetCameraLookAtPoint().position;
        target.position = Vector3.Lerp(target.position, desiredPosition, followSmooth * Time.deltaTime);

        float rot = rotateInput * rotationSpeed * Time.deltaTime;
        target.Rotate(Vector3.up, rot, Space.World);
        overviewCameraTarget.rotation = target.rotation;
    }


    private void LateUpdate()
    {
        if (!CheckBeforeUpdate()) return;
        if (!topFollowCamera.IsLive) return;

        if (Mathf.Abs(zoomInput) > 0.0001f)
        {
            desiredDistance -= zoomInput * zoomSpeed;
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        }

        Vector3 offset = topFollowCameraFollow.FollowOffset;
        offset.y = Mathf.Lerp(offset.y, desiredDistance, zoomSmooth * Time.deltaTime);
        topFollowCameraFollow.FollowOffset = offset;
    }

    private bool CheckBeforeUpdate()
    {
        if (!canControl) return false;
        if (topFollowCamera == null) return false;
        if (topFollowCameraFollow == null) return false;
        if (activeCharacterManager == null) return false;
        if (target == null) return false;

        _activeCharacterCached = activeCharacterManager.GetActivePlayableCharacter();
        if (_activeCharacterCached == null) return false;

        return true;
    }


    public void OnRotate(InputAction.CallbackContext callbackContext)
    {
        rotateInput = callbackContext.ReadValue<float>();
    }
    public void OnZoom(InputAction.CallbackContext callbackContext)
    {
        zoomInput = callbackContext.ReadValue<float>();
    }

    public void AllowControl()
    {
        canControl = true;
    }

    public void RevokeControl()
    {
        canControl = false;
    }
}
