using UnityEngine;
using UnityEngine.UI;
public class OptionsMenu : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("No hay AudioManager");
        }

        if (masterSlider != null)
            masterSlider.value = AudioManager.Instance != null ? AudioManager.Instance.MasterVolume : 1f;
        if (musicSlider != null)
            musicSlider.value = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume : 1f;
        if (sfxSlider != null)
            sfxSlider.value = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 1f;
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

    public void ApplyAndClose()
    {
        gameObject.SetActive(false);
    }
}