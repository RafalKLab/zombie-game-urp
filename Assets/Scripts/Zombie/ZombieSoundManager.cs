using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSoundManager : MonoBehaviour
{
    [SerializeField] private List<AudioClip> basicSounds;
    [SerializeField] private AudioSource audioSource;

    private bool soundsEnabled = true;
    private Coroutine soundRoutine;

    public void EnableSounds()
    {
        soundsEnabled = true;

        if (soundRoutine == null)
            soundRoutine = StartCoroutine(PlaySequenceLoop());
    }

    public void DisableSounds()
    {
        soundsEnabled = false;

        if (soundRoutine != null)
        {
            StopCoroutine(soundRoutine);
            soundRoutine = null;
        }

        audioSource.Stop();
    }

    private IEnumerator PlaySequenceLoop()
    {
        while (soundsEnabled)
        {
            if (!audioSource.isPlaying)
            {
                int randomIndex = Random.Range(0, basicSounds.Count);
                AudioClip clip = basicSounds[randomIndex];

                audioSource.clip = clip;
                audioSource.Play();

                yield return new WaitForSeconds(clip.length + 1f);
            }

            yield return null;
        }
    }
}