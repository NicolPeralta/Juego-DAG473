using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-100)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioMixer audioMixer;
    public string masterParamName = "MasterParam";
    public string musicParamName = "MusicaParam";
    public string sfxParamName = "EfectosParam";
    public AudioClip backgroundMusic;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    public AudioSource musicSource;
    [Range(0f, 1f)] public float defaultMaster = 1f;
    [Range(0f, 1f)] public float defaultMusic = 1f;
    [Range(0f, 1f)] public float defaultSfx = 1f;

    public float MasterVolume { get; private set; } = 1f;
    public float MusicVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    const string PREF_MASTER = "AM_Master";
    const string PREF_MUSIC = "AM_Music";
    const string PREF_SFX = "AM_SFX";
    AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MASTER, defaultMaster));
        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_MUSIC, defaultMusic));
        SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_SFX, defaultSfx));

        // Both sources belong to the persistent manager, never to a scene canvas.
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.ignoreListenerPause = true;
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.clip = backgroundMusic;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.outputAudioMixerGroup = sfxGroup;
    }

    void Start()
    {
        if (Instance != this) return;
        // Unity mixer parameters must be applied after Awake.
        ApplyVolumes();
        if (musicSource.clip != null) musicSource.Play();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SetMaster(float linear)
    {
        MasterVolume = Mathf.Clamp01(linear);
        PlayerPrefs.SetFloat(PREF_MASTER, MasterVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetMusic(float linear)
    {
        MusicVolume = Mathf.Clamp01(linear);
        PlayerPrefs.SetFloat(PREF_MUSIC, MusicVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetSfx(float linear)
    {
        SfxVolume = Mathf.Clamp01(linear);
        PlayerPrefs.SetFloat(PREF_SFX, SfxVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    void ApplyVolumes()
    {
        bool routed = audioMixer != null && musicGroup != null && sfxGroup != null;
        if (routed)
        {
            audioMixer.SetFloat(masterParamName, LinearToDb(MasterVolume));
            audioMixer.SetFloat(musicParamName, LinearToDb(MusicVolume));
            audioMixer.SetFloat(sfxParamName, LinearToDb(SfxVolume));
        }

        // Each gain is applied once, either by the mixer or by the sources.
        if (musicSource != null)
        {
            musicSource.volume = routed ? 1f : MasterVolume * MusicVolume;
            musicSource.mute = MasterVolume <= 0f || MusicVolume <= 0f;
        }
        if (sfxSource != null)
        {
            sfxSource.volume = routed ? 1f : MasterVolume * SfxVolume;
            sfxSource.mute = MasterVolume <= 0f || SfxVolume <= 0f;
        }
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }

    static float LinearToDb(float linear)
    {
        return Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
    }
}
