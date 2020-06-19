using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceImpactAudio : MonoBehaviour
{
    public static PieceImpactAudio Instance;

    public List<AudioClip> audioClips;
    public AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        Instance = this;
    }

    public static void PlayRandomImpactAudio()
    {
        Instance.audioSource.PlayOneShot(Instance.audioClips[Random.Range(0, Instance.audioClips.Count)]);
    }
}
