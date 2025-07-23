using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Player1 Sounds")]
    public AudioClip player1Jump;
    public AudioClip player1DoubleJump;

    public AudioClip player1Dash;
    public AudioClip player1Attack;
    public AudioClip player1Boomerang;
    public AudioClip player1Hurt;
    public AudioClip player1Walk;
    public AudioClip player1Heal;

    [Header("Player2 Sounds")]
    public AudioClip player2Jump;
    public AudioClip player2Dash;
    public AudioClip player2DoubleJump;
    public AudioClip player2Attack;
    public AudioClip player2ChargedAttack;
    public AudioClip player2Block;
    public AudioClip player2Hurt;
    public AudioClip player2Walk;
    public AudioClip player2Heal;

    public AudioClip player2BattleCry;

    [Header("Enemy Sounds")]
    public AudioClip enemyFly;
    public AudioClip enemyFlyHurt;
    public AudioClip enemyJump;
    public AudioClip enemyHurt;
    public AudioClip enemyWalk;

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
    public AudioClip explosionSound;

    public AudioSource sfxSource;
    public AudioSource musicSource;

    void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            sfxSource = GetComponent<AudioSource>();
            musicSource = GetComponentInChildren<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlayUIClick()
    {
        Debug.Log("Intentant reproduir UI Click. Mutejat? " + sfxSource.mute);
        if (uiClick != null && sfxSource != null)
            sfxSource.PlayOneShot(uiClick);
        else
            Debug.LogWarning("uiClick o sfxSource no assignats!");
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
        Debug.Log("Àudio silenciat: " + mute);
        musicSource.mute = mute;
        sfxSource.mute = mute; // també silencia els SFX si la música està silenciada
    }
    public void MuteSFX(bool mute)
    {
        sfxSource.mute = mute;
    }
    public bool IsMusicMuted()
    {
        return musicSource.mute;
    }
}
