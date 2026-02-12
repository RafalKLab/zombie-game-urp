using System;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterCore))]
public class PlayableCharacter : MonoBehaviour, ICharacterController
{
    // EntityId
    private string instanceGuid;

    // Events
    public event EventHandler OnKilled;

    private Transform cameraLookAtPoint;
    
    private CharacterCore characterCore;

    private float deathDelay = 1f;

    private void Awake()
    {
        characterCore = GetComponent<CharacterCore>();
        cameraLookAtPoint = characterCore.GetCameraLookAtPoint();
    }

    private void OnEnable()
    {
        characterCore.OnKilled += CharacterCore_OnDied;
    }

    private void OnDisable()
    {
        characterCore.OnKilled -= CharacterCore_OnDied;
    }

    private void CharacterCore_OnDied(object sender, System.EventArgs e)
    {
        OnKilled?.Invoke(this, EventArgs.Empty);
        StartCoroutine(HandleDeath());
    }

    public void MoveTo(Vector3 target)
    {
        characterCore.MoveTo(target);
    }

    public void SetAttackTarget(AiTarget aiTarget)
    {
        characterCore.SetAttackTarget(aiTarget);
    }

    public void ClearAttackTarget()
    {
        characterCore.ClearAttackTarget();
    }

    public void SetInstanceGuid(string newId)
    {
        if (string.IsNullOrWhiteSpace(newId))
            throw new ArgumentException("newId is null/empty");

        if (!string.IsNullOrWhiteSpace(instanceGuid) && instanceGuid != newId)
            throw new InvalidOperationException($"PlayableCharacter ID already set: {instanceGuid}");

        instanceGuid = newId;
    }

    public string GetInstanceGuid()
    {
        return instanceGuid;
    }

    public Transform GetCameraLookAtPoint()
    {
        return cameraLookAtPoint;
    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(deathDelay);

        Destroy(gameObject);
    }
}
