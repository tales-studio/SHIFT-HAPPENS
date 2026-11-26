using System.Collections;
using UnityEngine;

public interface ITransitionCard
{
    IEnumerator Play(Vector2 from, Vector2 to);
}
