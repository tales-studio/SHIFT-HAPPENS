using UnityEngine;

public static class ServicesBootstrapper
{
    private const string PrefabPath = "Services";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateServices()
    {
        GameObject prefab = Resources.Load<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("ServicesBootstrapper: no prefab found at Resources/" + PrefabPath + ".");
            return;
        }

        GameObject services = Object.Instantiate(prefab);
        services.name = prefab.name;
        Object.DontDestroyOnLoad(services);
    }
}
