using System.Collections;

public interface IScreenFader
{
    bool IsFaded { get; }

    IEnumerator FadeOut(float duration = -1f);
    IEnumerator FadeIn(float duration = -1f);
}
