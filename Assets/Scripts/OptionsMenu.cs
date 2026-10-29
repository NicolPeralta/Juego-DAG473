using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    void OnEnable()
    {
        SyncSliders();
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }

    void Start() => SyncSliders();

    void SyncSliders()
    {
        var audio = AudioManager.Instance;
        if (audio == null) return;
        if (masterSlider != null) masterSlider.SetValueWithoutNotify(audio.MasterVolume);
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(audio.MusicVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(audio.SfxVolume);
    }

    void OnDisable()
    {
        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
    }

    void OnMasterChanged(float value) => AudioManager.Instance?.SetMaster(value);
    void OnMusicChanged(float value) => AudioManager.Instance?.SetMusic(value);
    void OnSfxChanged(float value) => AudioManager.Instance?.SetSfx(value);

    public void ApplyAndClose() => gameObject.SetActive(false);
}
