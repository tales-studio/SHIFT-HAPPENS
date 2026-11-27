using UnityEngine;

// Si la actividad necesita que Ramón haga algo para continuar.
public enum ActivityInteractionMode
{
    None,         // No pide nada
    MidActivity,  // Puede pedir algo a mitad y se para hasta recibirlo (apuntes)
    Blocking      // Lo pide al empezar y no termina hasta recibirlo (llorar)
}

[CreateAssetMenu(fileName = "NewActivity", menuName = "Shift Happens/Activity Data")]
public class ActivityData : ScriptableObject
{
    [Header("Destino")]
    [Tooltip("Tipo de destino al que va. Si es Bedroom y el estudiante tiene habitación propia, va a la suya.")]
    public NPCDestinationType destinationType = NPCDestinationType.Generic;

    [Header("Duración base (segundos)")]
    [Tooltip("Se multiplica por el durationMultiplier del perfil del estudiante.")]
    [Min(0f)] public float minDuration = 5f;
    [Min(0f)] public float maxDuration = 10f;

    [Header("Necesidad")]
    [Tooltip("Segundos sin hacerla para que la necesidad llegue al máximo. 0 = sin necesidad.")]
    [Min(0f)] public float timeToFullNeed = 0f;

    [Tooltip("Peso de la necesidad frente a la preferencia del perfil.")]
    [Range(0f, 1f)] public float needWeight = 0.85f;

    [Header("Repetición")]
    [Tooltip("Si es false, su utilidad baja mucho si fue la última actividad.")]
    public bool allowRepeatInARow = false;

    [Header("Interacción con Ramón")]
    public ActivityInteractionMode interactionMode = ActivityInteractionMode.None;

    [Tooltip("Tarea propia del estudiante que se activa (apuntes, consolar).")]
    public TaskData interactionTask;

    [Tooltip("Momento (0-1) en el que la pide en modo MidActivity.")]
    [Range(0f, 1f)] public float interactionAt = 0.5f;

    [Header("Otros")]
    [Tooltip("Bool del Animator que se activa mientras la realiza (opcional).")]
    public string animatorBool = "";

    [Tooltip("Mientras la realiza, su entidad no puede aparecer (Cry).")]
    public bool blocksEntity = false;

    public float RandomDuration(float multiplier)
    {
        return Random.Range(minDuration, Mathf.Max(minDuration, maxDuration)) * multiplier;
    }
}
