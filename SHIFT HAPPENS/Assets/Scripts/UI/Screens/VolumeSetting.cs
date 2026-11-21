using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    public enum Channel
    {
        Master,
        Music,
        Sfx,
        UI
    }

    [SerializeField] private Channel channel = Channel.Master;
    [SerializeField] private Slider slider;
    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text valueLabel;
    [SerializeField] private string valueFormat = "{0}%";
    [SerializeField, Range(0f, 1f)] private float dragStep = 0.1f;
    [SerializeField, Range(0.05f, 1f)] private float unmuteVolume = 0.5f;

    private IAudioVolumeService service;
    private SliderDragRelay dragRelay;
    private float lastPushed;
    private float lastAudible = -1f;
    private bool[] savedEnabled;

    private bool IsDragging => dragRelay != null && dragRelay.IsHeld;

    private void Awake()
    {
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.onValueChanged.AddListener(OnSliderChanged);

            dragRelay = slider.GetComponent<SliderDragRelay>();
            if (dragRelay == null) dragRelay = slider.gameObject.AddComponent<SliderDragRelay>();
            dragRelay.Released += FlushSlider;
            EnsureSelectOnHover(slider);
        }

        if (toggle != null)
        {
            EnsureSelectOnHover(toggle);
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    private static void EnsureSelectOnHover(Selectable target)
    {
        if (target.GetComponent<SelectOnHover>() == null) target.gameObject.AddComponent<SelectOnHover>();
    }

    private void OnEnable()
    {
        Bind();
    }

    private void Start()
    {
        if (!ReferenceEquals(service, AudioServices.Volume)) Bind();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void Bind()
    {
        Unbind();
        service = AudioServices.Volume;
        service.VolumeChanged += Refresh;
        Refresh();
    }

    private void Unbind()
    {
        if (service != null) service.VolumeChanged -= Refresh;
        service = null;
    }

    private void OnDestroy()
    {
        if (slider != null) slider.onValueChanged.RemoveListener(OnSliderChanged);
        if (toggle != null) toggle.onValueChanged.RemoveListener(OnToggleChanged);
        if (dragRelay != null) dragRelay.Released -= FlushSlider;
    }

    private void OnSliderChanged(float value)
    {
        UpdateLabel(value);
        bool crossesZero = (value <= 0f) != (lastPushed <= 0f);
        if (IsDragging && !crossesZero && Mathf.Abs(value - lastPushed) < dragStep) return;
        PushVolume(value);
    }

    private void FlushSlider()
    {
        if (slider != null && !Mathf.Approximately(slider.value, lastPushed)) PushVolume(slider.value);
    }

    private void PushVolume(float value)
    {
        lastPushed = value;
        if (value > 0f) lastAudible = value;
        SetVolume(value);

        if (value <= 0f && GetEnabled()) SetEnabled(false);
        else if (value > 0f && !GetEnabled()) SetEnabled(true);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn && GetVolume() <= 0f) PushVolume(lastAudible > 0f ? lastAudible : unmuteVolume);
        else SetEnabled(isOn);
    }

    private void Refresh()
    {
        float volume = GetVolume();
        bool isOn = GetEnabled();
        float shown = isOn ? volume : 0f;
        lastPushed = shown;
        if (volume > 0f) lastAudible = volume;

        if (toggle != null) toggle.SetIsOnWithoutNotify(isOn);

        if (IsDragging) return;

        if (slider != null) slider.SetValueWithoutNotify(shown);
        UpdateLabel(shown);
    }

    private void UpdateLabel(float volume)
    {
        if (valueLabel != null) valueLabel.text = string.Format(valueFormat, Mathf.RoundToInt(volume * 100f));
    }

    private IAudioVolumeService Volume => service ?? AudioServices.Volume;

    private float GetVolume()
    {
        switch (channel)
        {
            case Channel.Music: return Volume.MusicVolume;
            case Channel.Sfx: return Volume.SfxVolume;
            case Channel.UI: return Volume.UiVolume;
            default: return Volume.MasterVolume;
        }
    }

    private void SetVolume(float value)
    {
        switch (channel)
        {
            case Channel.Music: Volume.MusicVolume = value; break;
            case Channel.Sfx: Volume.SfxVolume = value; break;
            case Channel.UI: Volume.UiVolume = value; break;
            default: Volume.MasterVolume = value; break;
        }
    }

    private bool GetEnabled()
    {
        switch (channel)
        {
            case Channel.Music: return Volume.MusicEnabled;
            case Channel.Sfx: return Volume.SfxEnabled;
            case Channel.UI: return Volume.UiEnabled;
            default: return Volume.MusicEnabled || Volume.SfxEnabled || Volume.UiEnabled;
        }
    }

    private void SetEnabled(bool isOn)
    {
        switch (channel)
        {
            case Channel.Music: Volume.MusicEnabled = isOn; break;
            case Channel.Sfx: Volume.SfxEnabled = isOn; break;
            case Channel.UI: Volume.UiEnabled = isOn; break;
            default: SetAllEnabled(isOn); break;
        }
    }

    private void SetAllEnabled(bool isOn)
    {
        if (!isOn)
        {
            if (GetEnabled()) savedEnabled = new[] { Volume.MusicEnabled, Volume.SfxEnabled, Volume.UiEnabled };
            Volume.MusicEnabled = false;
            Volume.SfxEnabled = false;
            Volume.UiEnabled = false;
            return;
        }

        bool restore = savedEnabled != null && (savedEnabled[0] || savedEnabled[1] || savedEnabled[2]);
        Volume.MusicEnabled = !restore || savedEnabled[0];
        Volume.SfxEnabled = !restore || savedEnabled[1];
        Volume.UiEnabled = !restore || savedEnabled[2];
        savedEnabled = null;
    }
}
