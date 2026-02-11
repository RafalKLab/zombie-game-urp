using StarterAssets;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera aimVirtualCamera;
    [SerializeField] private CinemachineBrain brain;

    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;

    [SerializeField] private LayerMask aimLayerMask = new LayerMask();

    public Transform spawnBulletPosition;
    public Transform bulletPrefab;

    private StarterAssetsInputs starterAssetsInputs;
    private ThirdPersonController thirdPersonController;
    private Animator animator;

    private void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();

        brain.DefaultBlend.Time = 0.1f;
    }

    private void Update()
    {
        if (starterAssetsInputs.aim)
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

            Vector3 mouseWorldPosition = Vector3.zero;

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimLayerMask))
            {
                mouseWorldPosition = raycastHit.point;
            }

            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.Sensitivity = aimSensitivity;
            thirdPersonController.SetRotateOnMove(false);

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

        } else
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));

            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.Sensitivity = normalSensitivity;
            thirdPersonController.SetRotateOnMove(true);
        }

        //if (starterAssetsInputs.shoot)
        //{
        //    Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;

        //    Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
        //    starterAssetsInputs.shoot = false;
        //}
    }
}
