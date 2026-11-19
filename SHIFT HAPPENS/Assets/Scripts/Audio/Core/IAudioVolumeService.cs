using System;

public interface IAudioVolumeService
{
    float MasterVolume { get; set; }

    bool MusicEnabled { get; set; }
    float MusicVolume { get; set; }

    bool SfxEnabled { get; set; }
    float SfxVolume { get; set; }

    bool UiEnabled { get; set; }
    float UiVolume { get; set; }

    event Action VolumeChanged;
}
