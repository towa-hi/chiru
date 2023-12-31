using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioSource bodyAudioSource;
    public AudioSource weaponAudioSource;
    
    public AudioClip parry;
    public AudioClip riposte;
    public AudioClip swordClash;
    public AudioClip whiff;
    public AudioClip playerWhiff;
    public AudioClip hurt;
    public AudioClip death;
    public AudioClip playerDeath;
    public AudioClip gunshot;
    
    public void PlaySound(string clip)
    {
        PlaySoundFromSource(0, clip);
    }

    public void PlayWeaponSound(string clip)
    {
        PlaySoundFromSource(1, clip);
    }
    
    void PlaySoundFromSource(int sourceIndex, string clip)
    {
        AudioSource source;
        if (sourceIndex == 0)
        {
            source = bodyAudioSource;
        }
        else
        {
            source = weaponAudioSource;
        }
        
        source.pitch = 1f;
        switch (clip)
        {
            case "parry":
                source.clip = parry;
                break;
            case "riposte":
                source.clip = riposte;
                break;
            case "swordClash":
                source.clip = swordClash;
                break;
            case "whiff":
                source.clip = whiff;
                break;
            case "slowWhiff":
                source.pitch = 0.8f;
                source.clip = whiff;
                break;
            case "shieldWhiff":
                source.pitch = 2f;
                source.clip = whiff;
                break;
            case "playerWhiff":
                source.clip = playerWhiff;
                break;
            case "hurt":
                source.clip = hurt;
                break;
            case "death":
                source.clip = death;
                break;
            case "playerDeath":
                source.clip = playerDeath;
                break;
            case "gunshot":
                source.clip = gunshot;
                break;
            default:
                // Optionally handle unknown clip names
                Debug.LogWarning("SoundPlayer: Unknown clip name '" + clip + "'");
                return; // Exit the method if the clip name is unknown
        }
        source.Play();
    }
}
