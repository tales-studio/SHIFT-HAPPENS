using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Shift Happens/Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public SoundId id;
        public SoundData sound;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    [NonSerialized] private Dictionary<SoundId, SoundData> lookup;

    public bool TryGet(SoundId id, out SoundData sound)
    {
        if (lookup == null) BuildLookup();
        return lookup.TryGetValue(id, out sound) && sound != null;
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<SoundId, SoundData>(entries.Count);
        foreach (Entry entry in entries)
        {
            if (entry == null || entry.id == SoundId.None) continue;
            if (lookup.ContainsKey(entry.id))
            {
                Debug.LogWarning($"SoundLibrary: {entry.id} is listed more than once. The first entry is used.", this);
                continue;
            }
            lookup.Add(entry.id, entry.sound);
        }
    }

    private void OnValidate()
    {
        lookup = null;
    }

#if UNITY_EDITOR
    public void EditorSet(SoundId id, SoundData sound)
    {
        Entry existing = entries.Find(e => e != null && e.id == id);
        if (existing != null) existing.sound = sound;
        else entries.Add(new Entry { id = id, sound = sound });
        entries.Sort((a, b) => ((int)a.id).CompareTo((int)b.id));
        lookup = null;
    }
#endif
}
