using System.Collections;
using UnityEngine;

public sealed class NullTransitionCard : ITransitionCard
{
    public static readonly NullTransitionCard Instance = new NullTransitionCard();

    private NullTransitionCard() { }

    public IEnumerator Play(Vector2 from, Vector2 to) { yield break; }
}
