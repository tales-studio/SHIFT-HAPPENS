using UnityEngine;

public class PlacePlayerStep : SequenceStep
{
    [SerializeField] private Transform destination;

    protected override void OnEnter()
    {
        if (destination != null && Player.Instance != null)
            Player.Instance.Teleporter.PlaceAt(destination.position);

        Complete();
    }
}
