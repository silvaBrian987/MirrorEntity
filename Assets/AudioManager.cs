using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager INSTANCE;

    private AudioSource audioSource;

    private void Awake()
    {
        if (INSTANCE == null)
        {
            INSTANCE = this;
            DontDestroyOnLoad(this);
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void StartMusic(AudioClip audioClip, bool loop = false, float volume = 1f)
    {
        if (audioClip != null)
        {
            StopMusic();
            audioSource.clip = audioClip;
            audioSource.loop = loop;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
