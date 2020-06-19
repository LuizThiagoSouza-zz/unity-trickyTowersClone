using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownAudio : MonoBehaviour
{
    public AudioSource source;
    public List<AudioClip> countdownClips;

    public void PlayAudio(int index)
    {
        if (index < 0 || index >= countdownClips.Count) return;

        source.PlayOneShot(countdownClips[index]);
    }
}
