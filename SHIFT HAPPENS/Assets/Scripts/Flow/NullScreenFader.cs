using System.Collections;

public sealed class NullScreenFader : IScreenFader
{
    public static readonly NullScreenFader Instance = new NullScreenFader();

    private NullScreenFader() { }

    public bool IsFaded => false;

    public IEnumerator FadeOut(float duration = -1f) { yield break; }
    public IEnumerator FadeIn(float duration = -1f) { yield break; }
}
