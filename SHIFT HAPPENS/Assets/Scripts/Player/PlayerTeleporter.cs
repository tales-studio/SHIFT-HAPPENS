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

    private IEnumerator TeleportRoutine(Vector2 destination)
    {
        IsTeleporting = true;
        OnTeleportStarted?.Invoke();
        SetMovementLocked(true);

        IScreenFader fader = SceneServices.Fader;

        yield return fader.FadeOut();
        body.position = destination;
        yield return fader.FadeIn();

        SetMovementLocked(false);
        IsTeleporting = false;
    }

    private void SetMovementLocked(bool locked)
    {
        movement.enabled = !locked;
        body.linearVelocity = Vector2.zero;
    }
}
