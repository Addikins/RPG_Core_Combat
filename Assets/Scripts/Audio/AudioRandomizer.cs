﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioRandomizer : MonoBehaviour
{
    [SerializeField] AudioClip[] audioClips = null;
    [SerializeField] GameObject parentObject = null;
    [SerializeField] float volume = .5f;

    int randomClip;

    public void PlayRandomClip()
    {
        randomClip = Random.Range(0, audioClips.Length);
        AudioSource.PlayClipAtPoint(audioClips[randomClip], parentObject.transform.position, volume);
    }
}
