using System;
using UnityEngine;

public static class NoiseEvents
{
    public static event Action<Vector2, float, GameObject> Emitted;

    public static void Emit(Vector2 position, float radius, GameObject source)
    {
        Emitted?.Invoke(position, radius, source);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Emitted = null;
    }
}
