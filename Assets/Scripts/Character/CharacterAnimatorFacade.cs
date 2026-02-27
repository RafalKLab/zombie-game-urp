using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class CharacterAnimatorFacade : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string aimAnimationIndexParam = "AimAnimationIndex";
    [SerializeField] private string meleeDeathParam = "MeleeDeath";
    [SerializeField] private string weaponDeathParam = "WeaponDeath";
    [SerializeField] private string rifleIdle = "RifleIdle";

    [SerializeField] private float movingSpeedThreshold = 0.05f;
    [SerializeField] private float dampTime = 0.10f;

    [Header("Tune")]
    [SerializeField] private float aimLerpSpeed = 8f;
    [SerializeField]  private int animatorAimLayer = 1;

    [Header("Static Rigs")]
    [SerializeField] private Rig rifleIdleRig;

    [Header("Animation Config")]
    [SerializeField] private List<WeaponAimConfig> weaponAimConfigs;

    private int speedHash;
    private int aimAnimationIndexHash;
    private int meleeDeathParamHash;
    private int weaponDeathParamHash;
    private int rifleIdleHash;

    private Dictionary<WeaponType, WeaponAimConfig> aimConfigMap;

    private Rig currentRig;
    private Rig previousRig;

    private Animator animator;
    private NavMeshAgent agent;

    private bool isAiming = false;
    private bool isRifleIdle = false;

    public bool IsAiming => isAiming;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        speedHash = Animator.StringToHash(speedParam);
        aimAnimationIndexHash = Animator.StringToHash(aimAnimationIndexParam);
        meleeDeathParamHash = Animator.StringToHash(meleeDeathParam);
        weaponDeathParamHash = Animator.StringToHash(weaponDeathParam);
        rifleIdleHash = Animator.StringToHash(rifleIdle);

        SetupAimConfigDictionary();
    }

    private void Update()
    {
        float targetSpeed = 0f;

        if (!agent.pathPending)
        {
            float agentVelocity = agent.velocity.magnitude;
            if (agentVelocity > movingSpeedThreshold)
                targetSpeed = agentVelocity;
        }

        animator.SetFloat(speedHash, targetSpeed, dampTime, Time.deltaTime);

        // --- AIM LAYER (guard na layer index)
        if (animatorAimLayer >= 0 && animatorAimLayer < animator.layerCount)
        {
            float aimLayer = animator.GetLayerWeight(animatorAimLayer);
            float targetLayerWeight = isAiming ? 1f : 0f;

            animator.SetLayerWeight(
                animatorAimLayer,
                Mathf.MoveTowards(aimLayer, targetLayerWeight, aimLerpSpeed * Time.deltaTime)
            );
        }

        float currentRigTarget = isAiming ? 1f : 0f;

        BlendRig(currentRig, currentRigTarget);

        if (previousRig != null && previousRig != currentRig)
        {
            BlendRig(previousRig, 0f);
            if (previousRig.weight <= 0.001f)
                previousRig = null;
        }
    }


    public void EnableAim(WeaponTypeSO weaponTypeSO)
    {
        if (aimConfigMap == null) return;
        if (weaponTypeSO == null) return;

        previousRig = currentRig;

        if (aimConfigMap.TryGetValue(weaponTypeSO.weaponType, out var cfg))
        {
            currentRig = cfg.aimRig;
            animator.SetFloat(aimAnimationIndexHash, cfg.animationIndex);
        }
        else
        {
            currentRig = null;
            Debug.LogWarning($"No aim config for {weaponTypeSO.weaponType}");
        }

        isAiming = true;
    }

    public void DisableAim()
    {
        isAiming = false;
    }

    private void SetupAimConfigDictionary()
    {
        if (weaponAimConfigs == null) return;
        aimConfigMap = new Dictionary<WeaponType, WeaponAimConfig>();

        foreach (var cfg in weaponAimConfigs)
        {
            if (aimConfigMap.ContainsKey(cfg.weaponType))
            {
                Debug.LogWarning($"Duplicate aim config for {cfg.weaponType}");
                continue;
            }
            aimConfigMap.Add(cfg.weaponType, cfg);
        }
    }

    private void BlendRig(Rig rig, float targetWeight)
    {
        if (rig == null) return;
        rig.weight = Mathf.MoveTowards(rig.weight, targetWeight, aimLerpSpeed * Time.deltaTime);
    }

    public void PlayMeleeDeath()
    {
        if (!animator) return;

        animator.SetTrigger(meleeDeathParamHash);
    }

    public void PlayWeaponDeath()
    {
        if (!animator) return;

        animator.SetTrigger(weaponDeathParamHash);
    }

    public void EnableRifleIdle()
    {
        if (isRifleIdle) return;

        isRifleIdle = true;
        animator.SetBool(rifleIdleHash, true);

        if (rifleIdleRig != null)
        {
            rifleIdleRig.weight = 1;
        }
    }

    public void DisableRifleIdle()
    {
        if (!isRifleIdle) return;

        isRifleIdle = false;
        animator.SetBool(rifleIdleHash, false);

        if (rifleIdleRig != null)
        {
            rifleIdleRig.weight = 0;
        }
    }
}

[System.Serializable]
public class WeaponAimConfig
{
    public WeaponType weaponType;
    public float animationIndex;
    public Rig aimRig;
}
