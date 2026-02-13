using System;
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
    [SerializeField] private CinemachineCamera overviewCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;

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
            UnsetActivePlayableCharacter(e.playableCharacter);
        }
    }

    public PlayableCharacter GetActivePlayableCharacter()
    {
        return activePlayableCharacter;
    }

    public void SetActivePlayableCharacter(PlayableCharacter playableCharacter)
    {
        StopAllCoroutines();
        StartCoroutine(SwitchCameraTargetRoutine(playableCharacter));

        InvokeOnActiveCharacterChangedEvent(playableCharacter);
    }

    public void UnsetActivePlayableCharacter(PlayableCharacter playableCharacter)
    {
        if (activePlayableCharacter != playableCharacter)
            return;

        activePlayableCharacter = null;
        followCamera.Target.TrackingTarget = null;

        overviewCamera.gameObject.SetActive(true);
        followCamera.Priority = FOLLOW_CAMERA_PRIORITY_DEFAULT;

        InvokeOnActiveCharacterChangedEvent(playableCharacter);
    }

    private void InvokeOnActiveCharacterChangedEvent(PlayableCharacter playableCharacter)
    {
        OnActiveCharacterChanged?.Invoke(this, new OnActiveCharacterChangedEventArgs { playableCharacter = playableCharacter });
    }

    private System.Collections.IEnumerator SwitchCameraTargetRoutine(PlayableCharacter playableCharacter)
    {
        followCamera.Target.TrackingTarget = playableCharacter.GetCameraLookAtPoint();

        overviewCamera.gameObject.SetActive(false);
        followCamera.Priority = FOLLOW_CAMERA_PRIORITY_DEFAULT;
        activePlayableCharacter = null;

        yield return null;

        if (cinemachineBrain != null)
            yield return new WaitUntil(() => !cinemachineBrain.IsBlending);
        else
            yield return null; // fallback

        activePlayableCharacter = playableCharacter;
        

        followCamera.Priority = FOLLOW_CAMERA_PRIORITY_ACTIVE;
    }

    public bool HasActiveCharacter()
    {
        return activePlayableCharacter != null;
    }
}
