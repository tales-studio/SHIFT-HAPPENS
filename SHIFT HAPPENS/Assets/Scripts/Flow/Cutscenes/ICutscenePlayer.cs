using System;
using System.Collections;

public interface ICutscenePlayer
{
    bool IsPlaying { get; }

    event Action<Cutscene> Started;
    event Action<Cutscene> Finished;

    IEnumerator Play(Cutscene cutscene);
    void Skip();
}
