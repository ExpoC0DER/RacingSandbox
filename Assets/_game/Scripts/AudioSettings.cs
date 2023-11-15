using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSettings : MonoBehaviour
{
    FMOD.Studio.EventInstance SFXVolumeTestEvent;
    FMOD.Studio.Bus Music;
    FMOD.Studio.Bus SFX;

    float MusicVolume = 0.5f;
    float SFXVolume = 0.5f;

    
    void Awake()
    {
        Music = FMODUnity.RuntimeManager.GetBus("bus:/Music");
        SFX = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
    }

    void Update()
    {
        Music.setVolume (MusicVolume);
        SFX.setVolume (SFXVolume);
    }

    public void MusicVolumeLevel(float newMusicVolume){
        MusicVolume = newMusicVolume;
    }

    public void SFXVolumeLevel(float newSFXVolume){
        SFXVolume = newSFXVolume;
    }
}
