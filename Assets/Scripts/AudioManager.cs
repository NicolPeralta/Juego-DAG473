using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer (opcional)")]
    public AudioMixer audioMixer; // si usas mixer, dejarlo asignado

    [Header("Exposed parameter names (coincidir con tu AudioMixer)")]
    public string masterParamName = "MasterParam";
    public string musicParamName = "MusicaParam";
    public string sfxParamName = "EfectosParam";

    [Header("Fallback (si no usas AudioMixer)")]
    [Tooltip("AudioSource que reproducirá la música (asignar en inspector preferentemente).")]
    public AudioSource musicSource;
    public string musicSourcesTag = "Music"; // solo fallback si musicSource es null

    [Header("Defaults")]
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

        // AudioSource para SFX
        sfxSource = gameObject.GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f; // 2D

        // Si no hay musicSource asignada, intentar encontrar un AudioSource con el tag
        if (musicSource == null && !string.IsNullOrEmpty(musicSourcesTag))
        {
            var go = GameObject.FindWithTag(musicSourcesTag);
            if (go != null)
                musicSource = go.GetComponent<AudioSource>();
        }

        // Cargar prefs
        MasterVolume = PlayerPrefs.GetFloat(PREF_MASTER, defaultMaster);
        MusicVolume = PlayerPrefs.GetFloat(PREF_MUSIC, defaultMusic);
        SfxVolume = PlayerPrefs.GetFloat(PREF_SFX, defaultSfx);

        ApplyMaster(MasterVolume);
        ApplyMusic(MusicVolume);
        ApplySfx(SfxVolume);
    }

    // API pública
    public void SetMaster(float linear)
    {
        MasterVolume = Mathf.Clamp01(linear);
        PlayerPrefs.SetFloat(PREF_MASTER, MasterVolume);
        PlayerPrefs.Save();
        ApplyMaster(MasterVolume);
    }

    public void SetMusic(float linear)
    {
        MusicVolume = Mathf.Clamp01(linear);
        PlayerPrefs.SetFloat(PREF_MUSIC, MusicVolume);
        PlayerPrefs.Save();
        ApplyMusic(MusicVolume);
    }

    public void SetSfx(float linear)
    {
        SfxVolume = Mathf.Clamp01(linear);
        PlayerPrefs.SetFloat(PREF_SFX, SfxVolume);
        PlayerPrefs.Save();
        ApplySfx(SfxVolume);
    }

    void ApplyMaster(float linear)
    {
        if (audioMixer != null && !string.IsNullOrEmpty(masterParamName))
        {
            audioMixer.SetFloat(masterParamName, LinearToDb(Mathf.Max(0.0001f, linear)));
        }
        else
        {
            AudioListener.volume = linear;
            // Si usas musicSource fallback, mantener su volumen relativo a Master cuando se aplique Music
            if (musicSource != null)
                musicSource.volume = MusicVolume * MasterVolume;
        }
    }

    void ApplyMusic(float linear)
    {
        // Priorizar AudioMixer si está asignado
        if (audioMixer != null && !string.IsNullOrEmpty(musicParamName))
        {
            audioMixer.SetFloat(musicParamName, LinearToDb(Mathf.Max(0.0001f, linear)));
        }
        else if (musicSource != null)
        {
            // Usar musicSource si lo tienes asignado (más seguro)
            musicSource.volume = linear * MasterVolume;
            // Si no está reproduciendo, arrancar la reproducción (asegura que no se "destruya")
            if (!musicSource.isPlaying)
            {
                if (musicSource.clip != null)
                    musicSource.Play();
            }
        }
        else
        {
            // Fallback antiguo: ajustar AudioSources por tag
            var gos = GameObject.FindGameObjectsWithTag(musicSourcesTag);
            foreach (var go in gos)
            {
                var a = go.GetComponent<AudioSource>();
                if (a != null)
                {
                    a.volume = linear * MasterVolume;
                    if (!a.isPlaying && a.clip != null) a.Play();
                }
            }
        }
    }

    void ApplySfx(float linear)
    {
        if (audioMixer != null && !string.IsNullOrEmpty(sfxParamName))
        {
            audioMixer.SetFloat(sfxParamName, LinearToDb(Mathf.Max(0.0001f, linear)));
        }

        SfxVolume = linear;
    }

    // Reproducir SFX (en 2D, respetando Master * Sfx)
    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        float vol = SfxVolume * MasterVolume;
        sfxSource.PlayOneShot(clip, vol);
    }

    float LinearToDb(float linear)
    {
        linear = Mathf.Clamp(linear, 0.0001f, 1f);
        return Mathf.Log10(linear) * 20f;
    }
}