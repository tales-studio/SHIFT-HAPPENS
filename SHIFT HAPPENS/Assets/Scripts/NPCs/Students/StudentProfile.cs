using System;
using UnityEngine;

public enum StudentArchetype
{
    Nerd,         // Empollón
    Bully,
    Emo,
    Sleepwalker   // Sonámbulo
}

[Serializable]
public class ActivityEntry
{
    public ActivityData activity;
    [Range(0f, 1f)] public float preference = 0.5f;
}

// Cómo es un ARQUETIPO. Un asset por arquetipo, compartido por sus estudiantes.
// Las actividades comunes (baño, cocina, deambular) las tiene todo el mundo:
// aquí solo se ajusta cuánto le gustan. Las exclusivas se añaden a la lista.
[CreateAssetMenu(fileName = "NewStudentProfile", menuName = "Shift Happens/Student Profile")]
public class StudentProfile : ScriptableObject
{
    public StudentArchetype archetype;

    [Header("Actividades comunes (preferencia)")]
    [Range(0f, 1f)] public float bathroomPreference = 0.3f;
    [Range(0f, 1f)] public float kitchenPreference = 0.3f;
    [Range(0f, 1f)] public float wanderPreference = 0.3f;

    [Header("Actividades exclusivas")]
    public ActivityEntry[] exclusiveActivities = new ActivityEntry[0];

    [Header("Variación")]
    [Tooltip("Multiplica la duración de todas sus actividades.")]
    [Min(0.1f)] public float durationMultiplier = 1f;

    [Tooltip("Ruido máximo (+/-) que se suma a cada utilidad en cada decisión.")]
    [Range(0f, 0.3f)] public float decisionNoise = 0.08f;

    [Header("Tareas")]
    [Tooltip("Ignora la probabilidad de la TaskData y genera siempre la tarea (sonámbulo).")]
    public bool alwaysGeneratesTasks = false;
}
