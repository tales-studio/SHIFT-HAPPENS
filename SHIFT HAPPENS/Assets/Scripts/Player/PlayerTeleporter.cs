using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerMovement))]
public class PlayerTeleporter : MonoBehaviour
{
    private Rigidbody2D body;
    private PlayerMovement movement;

    public bool IsTeleporting { get; private set; }

    public event Action OnTeleportStarted;
    public event Action OnTeleported;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
    }

    public void Teleport(Transform destination)
    {
        if (IsTeleporting || destination == null) return;

        StartCoroutine(TeleportRoutine(destination.position));
    }

    public void PlaceAt(Vector2 position)
    {
        OnTeleportStarted?.Invoke();
        body.position = position;
        body.linearVelocity = Vector2.zero;
        OnTeleported?.Invoke();
    }

    private IEnumerator TeleportRoutine(Vector2 destination)
    {
        IsTeleporting = true;
        movement.AddLock(this);

        IScreenFader fader = SceneServices.Fader;
        Vector2 origin = body.position;

        OnTeleportStarted?.Invoke();
        yield return fader.FadeOut();
        body.position = destination;
        OnTeleported?.Invoke();
        yield return SceneServices.TransitionCard.Play(origin, destination);
        yield return fader.FadeIn();

        movement.RemoveLock(this);
        IsTeleporting = false;
    }
}
