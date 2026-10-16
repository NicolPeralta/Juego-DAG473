using UnityEngine;
using UnityEngine.UI;
            
/// <summary>
/// OptionsMenu actualizado: sliders sincronizados usando AudioManager singleton.
/// Asigna los sliders desde el inspector en ambos menús (inicio y pausa).
/// </summary>
public class OptionsMenu : MonoBehaviour
{
    [Header("Sliders (assign from Inspector)")]
    public Slider masterSlider; // opcional
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        // Esperar AudioManager
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("OptionsMenu: AudioManager no encontrado en la escena. Asegúrate de añadir AudioManager.");
        }

        // Inicializar sliders con valores actuales
        if (masterSlider != null)
            masterSlider.value = AudioManager.Instance != null ? AudioManager.Instance.MasterVolume : 1f;
        if (musicSlider != null)
            musicSlider.value = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume : 1f;
        if (sfxSlider != null)
            sfxSlider.value = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 1f;

        // Listeners
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }

    void OnDestroy()
    {
        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
    }

    void OnMasterChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMaster(v);
    }

    void OnMusicChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusic(v);
    }

    void OnSfxChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfx(v);
    }

    // Llamados por botones si los necesitas
    public void ApplyAndClose()
    {
        // los sliders ya aplican en tiempo real; aquí podrías cerrar el panel de opciones
        gameObject.SetActive(false);
    }
}