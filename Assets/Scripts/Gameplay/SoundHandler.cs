using System;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClipPick;
    [SerializeField] private AudioClip audioClipMistake;
    [SerializeField] private AudioClip audioClipPlaced;

    public void PlaySound(SoundType soundName)
    {
        switch (soundName)
        {
            case SoundType.Pick:
                audioSource.PlayOneShot(audioClipPick);
                break;
            case SoundType.Placed:
                audioSource.PlayOneShot(audioClipPlaced);
                break;
            case SoundType.Failed:
                audioSource.PlayOneShot(audioClipMistake);
                break;
        }
    }
}

public enum SoundType
{
    Pick,
    Placed,
    Failed
}
