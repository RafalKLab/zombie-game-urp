using UnityEngine;

public class InteractableActionDemolish : MonoBehaviour, IInteractableActionTwoStep
{
    [SerializeField] private int priority = 0;
    [SerializeField] private string executePrompt = "Demolish";
    [SerializeField] private int interactionsToDemolish = 4;
    [SerializeField] private ItemDefinitionSO rewardItem;
    [SerializeField] private int rewardAmount = 1;

    [Header("Cooldown")]
    [SerializeField] private float interactionCooldown = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip interactionSound;
    [SerializeField] private AudioClip interactionSoundFinal;

    [SerializeField] private float depletedDelay = 0.5f;
    private bool isDepleting = false;

    private AudioSource audioSource;
    private int currentDemolishProgress = 0;
    private bool isDepleted = false;

    // cooldown state
    private float nextAllowedTime = 0f;

    public int Priority => priority;
    public bool IsDepleted => isDepleted;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public bool CanExecute(Interactor interactor)
    {
        if (Time.time < nextAllowedTime) return false;
        if (!interactor.Character.HasMeleeWeapon()) return false;

        if (isDepleted) return false;
        if (isDepleting) return false;

        return true;
    }

    public bool Execute(Interactor interactor)
    {
        if (!CanExecute(interactor)) return false;

        // start cooldown immediately when execute begins
        nextAllowedTime = Time.time + interactionCooldown;

        FaceTargetInstant(interactor.transform, transform);

        interactor.SetPendingInteractableActionTwoStep(this);
        interactor.Character.PlayMeleeAttackAnimation();

        return true;
    }

    public bool FinalizeExecute(Interactor interactor)
    {
        if (isDepleted) return false;

        currentDemolishProgress++;

        if (currentDemolishProgress < interactionsToDemolish)
        {
            PlaySound(interactionSound);
            return true;
        }

        // FINAL HIT
        PlaySound(interactionSoundFinal);
        isDepleting = true;

        if (rewardItem != null && rewardAmount > 0)
        {
            interactor.Character.inventory.TryAddReturnRemaining(rewardItem, rewardAmount);
        }

        StartCoroutine(MarkDepletedAfterDelay());

        return true;
    }

    public string GetExecutePrompt(Interactor interactor)
    {
        return executePrompt;
    }

    private void FaceTargetInstant(Transform actor, Transform target)
    {
        Vector3 dir = target.position - actor.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        actor.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    private void PlaySound(AudioClip audioClip)
    {
        if (audioSource == null) return;
        if (audioClip == null) return;

        audioSource.PlayOneShot(audioClip);
    }
    private System.Collections.IEnumerator MarkDepletedAfterDelay()
    {
        float delay = depletedDelay;

        if (interactionSoundFinal != null)
            delay = Mathf.Max(delay, interactionSoundFinal.length);

        yield return new WaitForSeconds(delay);

        isDepleting = false;
        isDepleted = true;
    }
}