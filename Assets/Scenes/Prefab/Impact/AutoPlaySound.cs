using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlaySound : NetworkBehaviour
{
    [Header("Audio")]
    public AudioClip Shot;

    private AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        PlayAudio(Shot, false);
    }

    // Update is called once per frame
    void PlayAudio(AudioClip clip, bool loop)
    {
        source.loop = loop;
        source.clip = clip;
        source.Play();
    }
}
