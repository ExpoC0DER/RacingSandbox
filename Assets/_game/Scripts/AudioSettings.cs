using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class AudioSettings : MonoBehaviour
{
    [SerializeField] private FMOD.Studio.Bus _music;
    [SerializeField] private FMOD.Studio.Bus _sfx;
    [SerializeField] private FMOD.Studio.Bus _master;

    [SerializeField] private float _musicVolume = 0.5f;
    [SerializeField] private float _sfxVolume = 0.5f;
    [SerializeField] private float _masterVolume = 1f;
    [SerializeField] private TextMeshProUGUI _masterVolumeText;
    [SerializeField] private TextMeshProUGUI _musicVolumeText;
    [SerializeField] private TextMeshProUGUI _sfxVolumeText;
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    
    private void Awake()
    {
        _music = FMODUnity.RuntimeManager.GetBus("bus:/Music");
        _sfx = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
        _master = FMODUnity.RuntimeManager.GetBus("bus:/");
        LoadSettings();
    }

    private void Update()
    {
        _master.setVolume (_masterVolume);
        _music.setVolume (_musicVolume);
        _sfx.setVolume (_sfxVolume);

        _masterVolumeText.text = Mathf.Round(_masterVolume * 100) + "%";
        _musicVolumeText.text = Mathf.Round(_musicVolume * 100) + "%";
        _sfxVolumeText.text = Mathf.Round(_sfxVolume * 100) + "%";
    }
    public void MasterVolumeLevel(float _newMasterVolume){
        _masterVolume = _newMasterVolume;
    }
    public void MusicVolumeLevel(float _newMusicVolume){
        _musicVolume = _newMusicVolume;
    }

    public void SFXVolumeLevel(float _newSFXVolume){
        _sfxVolume = _newSFXVolume;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        _masterVolume = PlayerPrefs.GetFloat("MasterVolume", _masterVolume);
        _musicVolume = PlayerPrefs.GetFloat("MusicVolume", _musicVolume);
        _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", _sfxVolume);

        _masterVolumeSlider.value = _masterVolume;
        _musicVolumeSlider.value = _musicVolume;
        _sfxVolumeSlider.value = _sfxVolume;
    }
}
