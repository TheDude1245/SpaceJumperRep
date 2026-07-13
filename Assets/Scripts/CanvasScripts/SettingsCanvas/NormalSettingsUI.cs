using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NormalSettingsUI : MonoBehaviour
{
    private const string MasterVolumeKey = "MasterVolume";

    [Header("Master Volume")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Text masterVolumeValueText;

    private void OnEnable()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);

        AudioListener.volume = savedVolume;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(savedVolume);
        }

        UpdateMasterVolumeText(savedVolume);
    }

    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        AudioListener.volume = volume;

        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        PlayerPrefs.Save();

        UpdateMasterVolumeText(volume);
    }

    private void UpdateMasterVolumeText(float volume)
    {
        if (masterVolumeValueText == null)
            return;

        int percentage = Mathf.RoundToInt(volume * 100f);
        masterVolumeValueText.text = percentage + "%";
    }
}