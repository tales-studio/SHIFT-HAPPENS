using System;
using System.Collections;

public sealed class NullCutscenePlayer : ICutscenePlayer
{
    public static readonly NullCutscenePlayer Instance = new NullCutscenePlayer();

    private NullCutscenePlayer() { }

    public bool IsPlaying => false;

    public event Action<Cutscene> Started { add { } remove { } }
    public event Action<Cutscene> Finished { add { } remove { } }

    public IEnumerator Play(Cutscene cutscene) { yield break; }
    public void Skip() { }
}
