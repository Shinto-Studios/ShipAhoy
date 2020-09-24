using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // ----- Essentielle variabler ----- \\

    [SerializeField] private GameObject parentAudio = null;

    [SerializeField] private List<AudioClip> cannonSounds = new List<AudioClip>();

    // ----- API funktioner ----- \\

    public void PlayCannonSoundAtPos(Vector3 pos)
    {
        AudioClip audio = cannonSounds[Random.Range(0, cannonSounds.Count)];

        PlayAudioClipAtPos(audio, pos);
    }

    public void PlayAudioClipAtPos(AudioClip audio, Vector3 pos)
    {
        GameObject audioObject = new GameObject("AudioClip");
        audioObject.transform.position = pos;
        audioObject.transform.parent = parentAudio.transform;

        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.0f;
        audioSource.clip = audio;
        audioSource.Play();

        Destroy(audioObject, audio.length);
    }
}
