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
    }

    private IEnumerator TeleportRoutine(Vector2 destination)
    {
        IsTeleporting = true;
        movement.AddLock(this);

        IScreenFader fader = SceneServices.Fader;

        OnTeleportStarted?.Invoke();
        yield return fader.FadeOut();
        body.position = destination;
        yield return fader.FadeIn();

        movement.RemoveLock(this);
        IsTeleporting = false;
    }
}
