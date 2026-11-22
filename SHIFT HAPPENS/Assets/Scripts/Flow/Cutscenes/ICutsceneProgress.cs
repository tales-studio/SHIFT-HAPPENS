public interface ICutsceneProgress
{
    bool HasSeen(Cutscene cutscene);

    void MarkSeen(Cutscene cutscene);
    void Reset();
}
