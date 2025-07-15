using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public void SetMusicVolume(float value)
    {
        musicSource.volume = value;
    }
    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value;
    }
    public void MuteMusic(bool mute)
    {
        Debug.Log("Música silenciada: " + mute);
        musicSource.mute = mute;
        sfxSource.mute = mute; // Opcional: també silencia els SFX si la música està silenciada
    }
    public void MuteSFX(bool mute)
    {
        sfxSource.mute = mute;
    }
}
