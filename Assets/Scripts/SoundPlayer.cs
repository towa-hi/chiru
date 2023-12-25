using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip parry;
    public AudioClip riposte;
    public AudioClip swordClash;

    public void PlaySound(string clip)
    {
        switch (clip)
        {
            case "parry":
                audioSource.clip = parry;
                break;
            case "riposte":
                audioSource.clip = riposte;
                break;
            case "swordClash":
                audioSource.clip = swordClash;
                break;
            default:
                // Optionally handle unknown clip names
                Debug.LogWarning("SoundPlayer: Unknown clip name '" + clip + "'");
                return; // Exit the method if the clip name is unknown
        }

        Debug.Log("Played sound: " + clip);
        audioSource.Play();
    }

    public void PlayWeaponSound(string clip)
    {
        
    }
}
