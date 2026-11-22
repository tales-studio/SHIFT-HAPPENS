using UnityEngine;

public class PlacePlayerStep : SequenceStep
{
    [SerializeField] private Transform destination;

    protected override void OnEnter()
    {
        Place();
        Complete();
    }

    protected override void OnFastForward()
    {
        Place();
    }

    private void Place()
    {
        if (destination != null && Player.Instance != null)
            Player.Instance.Teleporter.PlaceAt(destination.position);
    }
}
