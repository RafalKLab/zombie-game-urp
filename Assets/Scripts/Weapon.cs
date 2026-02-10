using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponTypeSO weaponTypeSO;
    [SerializeField] private Transform muzzle;

    [SerializeField] private TracerProjectile tracerPrefab;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    public Transform GetMuzzle()
    {
        return muzzle;
    }

    public void PlayShot(ShotResult shot)
    {
        PlayAudio();

        if (tracerPrefab != null)
        {
            var tracerInstance = Instantiate(tracerPrefab);
            tracerInstance.Init(muzzle.position, shot.EndPoint);
        }
    }

    private void PlayAudio()
    {
        if (audioSource == null) return;
        if (weaponTypeSO == null) return;
        if (weaponTypeSO.shotClip == null) return;

        audioSource.PlayOneShot(weaponTypeSO.shotClip, weaponTypeSO.shotVolume);
    }

    public void PlayReload()
    {
        PlayReloadAudio();
    }

    private void PlayReloadAudio()
    {
        if (audioSource == null) return;
        if (weaponTypeSO == null) return;
        if (weaponTypeSO.reloadClip == null) return;

        audioSource.PlayOneShot(weaponTypeSO.reloadClip, weaponTypeSO.reloadVolume);
    }
}
