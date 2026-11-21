using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Credits", menuName = "Shift Happens/Credits Data")]
public class CreditsData : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string role;
        public List<string> names = new List<string>();
    }

    [Serializable]
    public class Section
    {
        public string heading;
        public List<Entry> entries = new List<Entry>();
    }

    public List<Section> sections = new List<Section>();
    [TextArea(1, 4)] public string closingMessage = "Thanks for playing!";
}
