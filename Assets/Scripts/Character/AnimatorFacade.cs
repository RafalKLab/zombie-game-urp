using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class AnimatorFacade : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    private IMoveModeProvider moveModeProvider;

    [Header("Animator params")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string isRunningParam = "IsRunning";
    [SerializeField] private string runAnimSpeedParam = "RunAnimSpeed";

    [Header("Tuning")]
    [SerializeField] private float stopSpeedDeadzone = 0.01f;
    [SerializeField] private float speedDampTime = 0.10f;

    [Header("Run anim scaling (relative to RunSpeed)")]
    [SerializeField] private float runAnimSpeedMin = 0.2f;
    [SerializeField] private float runAnimSpeedMax = 1.4f;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>(true);

        moveModeProvider = GetComponent<IMoveModeProvider>();

        if (animator != null)
            animator.applyRootMotion = false;
    }

    private void Update()
    {
        if (agent == null || animator == null) return;

        float v = agent.velocity.magnitude;

        // BlendTree Speed (0..1) within current agent.speed
        float normalized = v / Mathf.Max(agent.speed, 0.01f);
        if (normalized < stopSpeedDeadzone) normalized = 0f;
        animator.SetFloat(speedParam, normalized, speedDampTime, Time.deltaTime);

        // Running flag (immediate)
        bool wantsRun = moveModeProvider != null && moveModeProvider.IsRunning;
        animator.SetBool(isRunningParam, wantsRun);

        // RUN animation playback speed proportional to current velocity vs RunSpeed from SO
        if (moveModeProvider != null)
        {
            float runSpeed = Mathf.Max(moveModeProvider.RunSpeed, 0.01f);

            float runAnim = v / runSpeed;

            runAnim = Mathf.Clamp(runAnim, runAnimSpeedMin, runAnimSpeedMax);
            animator.SetFloat(runAnimSpeedParam, runAnim);
        }
    }
}
