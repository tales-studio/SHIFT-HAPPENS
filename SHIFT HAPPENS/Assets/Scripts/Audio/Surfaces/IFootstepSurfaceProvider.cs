using UnityEngine;

public interface IFootstepSurfaceProvider
{
    FootstepSurface GetSurfaceAt(Vector3 position);
}
