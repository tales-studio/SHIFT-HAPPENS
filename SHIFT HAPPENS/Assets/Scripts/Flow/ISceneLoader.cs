public interface ISceneLoader
{
    bool IsLoading { get; }

    void Load(SceneId id);
    void Reload();
}
