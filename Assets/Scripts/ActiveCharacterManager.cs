using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActiveCharacterManager : MonoBehaviour
{
    public event EventHandler<OnActiveCharacterChangedEventArgs> OnActiveCharacterChanged;
    public class OnActiveCharacterChangedEventArgs : EventArgs
    {
        public PlayableCharacter playableCharacter;
    }

    public static ActiveCharacterManager Instance { get; private set; }

    private const int FOLLOW_CAMERA_PRIORITY_ACTIVE = 20;
    private const int FOLLOW_CAMERA_PRIORITY_DEFAULT = 5;

    [SerializeField] private CinemachineCamera followCamera;
    [SerializeField] private Transform followCameraTarget;
    [SerializeField] private TopFollowCameraController topFollowCameraController;
    [SerializeField] private CinemachineCamera overviewCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private float deathCameraDelay = 3.5f;

    private PlayableCharacter activePlayableCharacter;

    private void Awake()
    {
        Instance = this;

        CharacterDeathManager.Instance.OnCharacterKilled += CharacterDeathManager_OnCharacterKilled;
    }

    private void CharacterDeathManager_OnCharacterKilled(object sender, CharacterDeathManager.CharacterKilledEventArgs e)
    {
        if (e.playableCharacter == activePlayableCharacter)
        {
            activePlayableCharacter = null;
            StartCoroutine(UnsetActiveAfterDelay(e.playableCharacter));
        }
    }

    private IEnumerator UnsetActiveAfterDelay(PlayableCharacter character)
    {
        yield return new WaitForSeconds(deathCameraDelay);

        if (activePlayableCharacter == character)
        {
            UnsetActivePlayableCharacter(character);
        }
    }

    public PlayableCharacter GetActivePlayableCharacter()
    {
        return activePlayableCharacter;
    }

    public void SetActivePlayableCharacter(PlayableCharacter playableCharacter)
    {
        if (activePlayableCharacter != null) return;

        StopAllCoroutines();
        StartCoroutine(SwitchCameraTargetRoutine(playableCharacter));

        InvokeOnActiveCharacterChangedEvent(playableCharacter);
    }

    public void UnsetActivePlayableCharacter(PlayableCharacter playableCharacter)
    {
        if (activePlayableCharacter != playableCharacter)
            return;

        activePlayableCharacter = null;
        topFollowCameraController.RevokeControl();

        overviewCamera.gameObject.SetActive(true);
        followCamera.Priority = FOLLOW_CAMERA_PRIORITY_DEFAULT;

        InvokeOnActiveCharacterChangedEvent(null);
    }

    private void InvokeOnActiveCharacterChangedEvent(PlayableCharacter playableCharacter)
    {
        OnActiveCharacterChanged?.Invoke(this, new OnActiveCharacterChangedEventArgs { playableCharacter = playableCharacter });
    }

    private IEnumerator SwitchCameraTargetRoutine(PlayableCharacter playableCharacter)
    {
        if (playableCharacter == null) yield break;
        if (followCameraTarget == null) yield break;

        followCameraTarget.position = playableCharacter.GetCameraLookAtPoint().position;
        followCamera.Priority = FOLLOW_CAMERA_PRIORITY_ACTIVE;

        // Allow camera blending
        yield return null;
        while (cinemachineBrain != null && cinemachineBrain.IsBlending)
        {
            yield return null;
        }

        activePlayableCharacter = playableCharacter;
        topFollowCameraController.AllowControl();
    }

    public bool HasActiveCharacter()
    {
        return activePlayableCharacter != null;
    }
}
