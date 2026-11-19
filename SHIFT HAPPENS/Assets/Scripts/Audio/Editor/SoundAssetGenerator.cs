using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SoundAssetGenerator
{
    private const string AudioRoot = "Assets/Audio";
    private const string DataRoot = AudioRoot + "/SoundData";
    private const string LibraryPath = AudioRoot + "/SoundLibrary.asset";

    private static readonly HashSet<SoundId> Loops = new HashSet<SoundId>
    {
        SoundId.PlayerBreathing,
        SoundId.TaskPhoneRing,
        SoundId.Electricity,
        SoundId.ApplianceHum,
        SoundId.LiftMove,
        SoundId.NPCConversation,
        SoundId.NPCParty,
        SoundId.NPCCrying,
        SoundId.NPCStudyingStressed,
        SoundId.EntityBreathing
    };

    private static readonly HashSet<SoundId> Always2D = new HashSet<SoundId>
    {
        SoundId.PlayerBreathing,
        SoundId.Blackout,
        SoundId.Jumpscare
    };

    [MenuItem("Shift Happens/Audio/Generate Sound Library")]
    public static void GenerateLibrary()
    {
        EnsureFolder(AudioRoot);
        EnsureFolder(DataRoot);

        SoundLibrary library = AssetDatabase.LoadAssetAtPath<SoundLibrary>(LibraryPath);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<SoundLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
        }

        int created = 0;
        foreach (SoundId id in Enum.GetValues(typeof(SoundId)))
        {
            if (id == SoundId.None) continue;
            if (library.TryGet(id, out _)) continue;

            SoundCategory category = GetCategory(id);
            string folder = $"{DataRoot}/{category}";
            EnsureFolder(folder);

            string path = $"{folder}/{id}.asset";
            SoundData data = AssetDatabase.LoadAssetAtPath<SoundData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<SoundData>();
                ApplyDefaults(data, id, category);
                AssetDatabase.CreateAsset(data, path);
                created++;
            }

            library.EditorSet(id, data);
        }

        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        Selection.activeObject = library;
        EditorGUIUtility.PingObject(library);
        Debug.Log($"Sound library ready at {LibraryPath}. {created} new SoundData asset(s) created in {DataRoot}. " +
                  "Assign the audio clips to each one.");
    }

    [MenuItem("Shift Happens/Audio/Create Sound Manager In Scene")]
    public static void CreateManagerInScene()
    {
        SoundManager existing = UnityEngine.Object.FindFirstObjectByType<SoundManager>();
        if (existing != null)
        {
            Selection.activeObject = existing;
            Debug.Log("There is already a SoundManager in the scene.", existing);
            return;
        }

        GameObject go = new GameObject("SoundManager");
        Undo.RegisterCreatedObjectUndo(go, "Create Sound Manager");
        SoundManager manager = go.AddComponent<SoundManager>();
        go.AddComponent<GamePhaseAudio>();
        go.AddComponent<TaskAudio>();
        go.AddComponent<AnxietyAudio>();

        SoundLibrary library = AssetDatabase.LoadAssetAtPath<SoundLibrary>(LibraryPath);
        if (library != null)
        {
            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("library").objectReferenceValue = library;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning("No SoundLibrary found. Run Shift Happens > Audio > Generate Sound Library and assign it.");
        }

        Selection.activeObject = go;
    }

    private static SoundCategory GetCategory(SoundId id)
    {
        int block = (int)id / 100;
        switch (block)
        {
            case 1: return SoundCategory.Music;
            case 2: return SoundCategory.Ambience;
            case 3: return SoundCategory.Player;
            case 4: return SoundCategory.Interaction;
            case 5: return SoundCategory.Interaction;
            case 6: return SoundCategory.Environment;
            case 7: return SoundCategory.NPC;
            case 8: return SoundCategory.Entity;
            case 9: return SoundCategory.UI;
            case 10: return SoundCategory.Music;
            default: return SoundCategory.Environment;
        }
    }

    private static void ApplyDefaults(SoundData data, SoundId id, SoundCategory category)
    {
        bool isStinger = (int)id >= 1000;
        switch (category)
        {
            case SoundCategory.Music:
                data.EditorSetDefaults(category, !isStinger, 0f, false, 1f, 1f);
                break;
            case SoundCategory.Ambience:
                data.EditorSetDefaults(category, true, 0f, false, 1f, 1f);
                break;
            case SoundCategory.UI:
                data.EditorSetDefaults(category, false, 0f, true, 0.97f, 1.03f);
                break;
            default:
                bool loop = Loops.Contains(id);
                float spatial = Always2D.Contains(id) ? 0f : 1f;
                data.EditorSetDefaults(category, loop, spatial, true, loop ? 0.97f : 0.9f, loop ? 1.03f : 1.1f);
                break;
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
    }
}
