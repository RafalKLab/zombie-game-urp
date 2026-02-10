using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDeathManager : MonoBehaviour
{
    public event EventHandler<CharacterKilledEventArgs> OnCharacterKilled;
    public class CharacterKilledEventArgs : EventArgs
    {
        public PlayableCharacter playableCharacter;
    }

    public static CharacterDeathManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SubscribeToPlayableCharacterOnKilled(PlayableCharacter playableCharacter)
    {
        playableCharacter.OnKilled += PlayableCharacter_OnKilled;
    }

    private void PlayableCharacter_OnKilled(object sender, System.EventArgs e)
    {
        PlayableCharacter playableCharacter = sender as PlayableCharacter;
        // we will inform spawner, ui, camera and all other
        Debug.Log("Gracz zostal zabity!!!");
        OnCharacterKilled?.Invoke(this, new CharacterKilledEventArgs { playableCharacter = playableCharacter });
    }
}
