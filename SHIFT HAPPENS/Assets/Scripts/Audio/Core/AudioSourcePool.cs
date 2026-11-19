using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AudioSourcePool
{
    private readonly Transform root;
    private readonly int maxSize;
    private readonly Func<Vector3, Vector3> project;
    private readonly Stack<PooledAudioSource> available = new Stack<PooledAudioSource>();
    private readonly List<PooledAudioSource> active = new List<PooledAudioSource>();
    private readonly List<PooledAudioSource> stopBuffer = new List<PooledAudioSource>();
    private int created;

    public IReadOnlyList<PooledAudioSource> Active => active;

    public AudioSourcePool(Transform root, int initialSize, int maxSize, Func<Vector3, Vector3> project)
    {
        this.root = root;
        this.maxSize = Mathf.Max(1, maxSize);
        this.project = project;

        int prewarm = Mathf.Clamp(initialSize, 0, this.maxSize);
        for (int i = 0; i < prewarm; i++)
            available.Push(Create());
    }

    public PooledAudioSource Get()
    {
        PooledAudioSource pooled;
        if (available.Count > 0)
        {
            pooled = available.Pop();
        }
        else if (created < maxSize)
        {
            pooled = Create();
        }
        else
        {
            PooledAudioSource victim = FindOldestOneShot();
            if (victim == null) return null;
            victim.Stop(0f);
            pooled = available.Pop();
        }

        active.Add(pooled);
        return pooled;
    }

    public int CountPlaying(SoundData sound)
    {
        int count = 0;
        foreach (PooledAudioSource pooled in active)
        {
            if (pooled.Sound == sound) count++;
        }
        return count;
    }

    public void StopAll(float fadeOut)
    {
        stopBuffer.Clear();
        stopBuffer.AddRange(active);
        foreach (PooledAudioSource pooled in stopBuffer)
            pooled.Stop(fadeOut);
    }

    private PooledAudioSource FindOldestOneShot()
    {
        PooledAudioSource oldest = null;
        foreach (PooledAudioSource pooled in active)
        {
            if (pooled.IsLooping) continue;
            if (oldest == null || pooled.StartedAt < oldest.StartedAt) oldest = pooled;
        }
        return oldest;
    }

    private void Release(PooledAudioSource pooled)
    {
        active.Remove(pooled);
        available.Push(pooled);
    }

    private PooledAudioSource Create()
    {
        GameObject go = new GameObject($"SFX Source {created:00}");
        go.transform.SetParent(root, false);
        go.AddComponent<AudioSource>();
        PooledAudioSource pooled = go.AddComponent<PooledAudioSource>();
        pooled.Initialize(Release, project);
        created++;
        return pooled;
    }
}
