using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider master;
    [SerializeField] Slider music;
    [SerializeField] Slider sfx;


    private void Start()
    {
        LoadVolume(); // Load volume settings at the start
    }

    public void SetMasterVolume() // Set the master volume
    {
        float volume = master.value;
        mixer.SetFloat("master", volume);
        PlayerPrefs.SetFloat("master_volume", volume); // Save the master volume to PlayerPrefs (saves between sessions)
    }

    public void SetMusicVolume() // Set the music volume
    {
        float volume = music.value;
        mixer.SetFloat("music", volume);
        PlayerPrefs.SetFloat("music_volume", volume);
    }

    public void SetSFXVolume() // Set the sound effects volume
    {
        float volume = sfx.value;
        mixer.SetFloat("sfx", volume);
        PlayerPrefs.SetFloat("sfx_volume", volume);
    }

    private void LoadVolume() // Load the volume settings from PlayerPrefs
    {
        if (PlayerPrefs.HasKey("master_volume"))
        {
            master.value = PlayerPrefs.GetFloat("master_volume");
        }
        if (PlayerPrefs.HasKey("music_volume"))
        {
            music.value = PlayerPrefs.GetFloat("music_volume");
        }
        if (PlayerPrefs.HasKey("sfx_volume"))
        {
            sfx.value = PlayerPrefs.GetFloat("sfx_volume");
        }

        SetMasterVolume();
        SetMusicVolume();
        SetSFXVolume();
    }
}
