using UnityEngine;

public sealed class NullFootstepSurfaceProvider : IFootstepSurfaceProvider
{
    public static readonly NullFootstepSurfaceProvider Instance = new NullFootstepSurfaceProvider();

    private NullFootstepSurfaceProvider() { }

    public FootstepSurface GetSurfaceAt(Vector3 position) => null;
}
