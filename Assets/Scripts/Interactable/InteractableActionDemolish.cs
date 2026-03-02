using UnityEngine;

public class InteractableActionDemolish : MonoBehaviour, IInteractableActionTwoStep
{
    [SerializeField] private int priority = 0;
    [SerializeField] private string executePrompt = "Demolish";
    [SerializeField] private int interactionsToDemolish = 4;
    [SerializeField] private ItemDefinitionSO rewardItem;
    [SerializeField] private int rewardAmount = 1;

    [Header("Audio")]
    [SerializeField] private AudioClip interactionSound;
    [SerializeField] private AudioClip interactionSoundFinal;

    private AudioSource audioSource;
    private int currentDemolishProgress = 0;
    private bool isDepleted = false;
    public int Priority => priority;

    public bool IsDepleted => isDepleted;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public bool CanExecute(Interactor interactor)
    {
        if (!interactor.Character.HasMeleeWeapon()) return false;

        return !isDepleted;
    }

    public bool Execute(Interactor interactor)
    {
        if (!CanExecute(interactor)) return false;

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
        isDepleted = true;

        if (rewardItem != null && rewardAmount > 0)
        {
            interactor.Character.inventory.TryAddReturnRemaining(rewardItem, rewardAmount);
        }

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
}
