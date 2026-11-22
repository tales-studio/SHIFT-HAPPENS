public interface ITutorialProgress
{
    bool HasSeen { get; }

    void MarkSeen();
    void Reset();
}
