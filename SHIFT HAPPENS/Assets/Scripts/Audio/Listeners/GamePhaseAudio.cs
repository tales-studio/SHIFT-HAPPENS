using System;
using System.Collections.Generic;
using UnityEngine;

public class GamePhaseAudio : MonoBehaviour
{
    [Serializable]
    private class PhaseAudio
    {
        public GamePhase phase;
        public SoundId music = SoundId.None;
        public SoundId[] stingers = new SoundId[0];
        public SoundId[] ambience = new SoundId[0];
        public bool distorted;
    }

    [SerializeField] private PhaseAudio[] phases =
    {
        new PhaseAudio
        {
            phase = GamePhase.Menu,
            music = SoundId.MusicMenu
        },
        new PhaseAudio
        {
            phase = GamePhase.Normal,
            music = SoundId.MusicNormal,
            stingers = new[] { SoundId.StingerShiftStart },
            ambience = new[] { SoundId.AmbienceWind, SoundId.AmbienceRain, SoundId.AmbienceDistantConversations }
        },
        new PhaseAudio
        {
            phase = GamePhase.Blackout,
            music = SoundId.None,
            stingers = new[] { SoundId.Blackout, SoundId.StingerOutbreak },
            ambience = new[] { SoundId.AmbienceWind, SoundId.AmbienceRain, SoundId.AmbienceUnsettlingNoises },
            distorted = true
        },
        new PhaseAudio
        {
            phase = GamePhase.Outbreak,
            music = SoundId.MusicTension,
            ambience = new[] { SoundId.AmbienceWind, SoundId.AmbienceRain, SoundId.AmbienceUnsettlingNoises, SoundId.AmbienceDistantNoises },
            distorted = true
        },
        new PhaseAudio
        {
            phase = GamePhase.Ended,
            music = SoundId.MusicEndScreen,
            stingers = new[] { SoundId.StingerShiftEnd }
        }
    };

    [SerializeField, Min(0f)] private float ambienceFade = 2f;

    private readonly List<SoundHandle> ambienceHandles = new List<SoundHandle>();
    private GamePhase? currentPhase;
    private GameManager subscribedTo;

    private void Start()
    {
        subscribedTo = GameManager.Instance;
        if (subscribedTo == null)
        {
            Debug.LogWarning("GamePhaseAudio: no GameManager in the scene.", this);
            return;
        }

        subscribedTo.OnPhaseChanged += Apply;
        Apply(subscribedTo.CurrentPhase);
    }

    private void OnDestroy()
    {
        if (subscribedTo != null) subscribedTo.OnPhaseChanged -= Apply;
        StopAmbience(0f);
    }

    private void Apply(GamePhase phase)
    {
        if (currentPhase == phase) return;
        currentPhase = phase;

        PhaseAudio config = Array.Find(phases, p => p.phase == phase);
        StopAmbience(ambienceFade);
        if (config == null) return;

        ISfxService sfx = AudioServices.Sfx;
        sfx.Distorted = config.distorted;
        AudioServices.Music.PlayMusic(config.music);

        foreach (SoundId stinger in config.stingers)
            sfx.Play(stinger);

        foreach (SoundId loop in config.ambience)
        {
            SoundHandle handle = sfx.Play(loop);
            if (handle.IsPlaying) ambienceHandles.Add(handle);
        }
    }

    private void StopAmbience(float fade)
    {
        foreach (SoundHandle handle in ambienceHandles)
            handle.Stop(fade);
        ambienceHandles.Clear();
    }
}
