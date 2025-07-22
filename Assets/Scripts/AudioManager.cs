using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Player1 Sounds")]
    public AudioClip player1Jump;
    public AudioClip player1DoubleJump;
    public AudioClip player1Attack;
    public AudioClip player1Hit;

    [Header("Player2 Sounds")]
    public AudioClip player2Jump;
    public AudioClip player2DoubleJump;
    public AudioClip player2Attack;
    public AudioClip player2Block;

    [Header("UI Sounds")]
    public AudioClip uiClick;
    public AudioClip uiPause;
    public AudioClip checkpointSound;
    public AudioClip gameOverSound;

    [Header("Environment Sounds")]
    public AudioClip trapSound;
    public AudioClip leverSound;
    public AudioClip doorSound;
    public AudioClip pressurePlateSound;

    private AudioSource sfxSource;
    public AudioSource musicSource;

    void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            sfxSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
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
