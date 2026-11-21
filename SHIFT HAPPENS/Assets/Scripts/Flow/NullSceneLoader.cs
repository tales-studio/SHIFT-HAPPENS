public sealed class NullSceneLoader : ISceneLoader
{
    public static readonly NullSceneLoader Instance = new NullSceneLoader();

    private NullSceneLoader() { }

    public bool IsLoading => false;

    public void Load(SceneId id) { }
    public void Reload() { }
}
