using System;
using UnityEngine;

public abstract class SequenceStep : MonoBehaviour
{
    public bool IsRunning { get; private set; }

    public event Action<SequenceStep> Completed;

    public void Enter()
    {
        if (IsRunning) return;

        IsRunning = true;
        OnEnter();
    }

    public void Exit()
    {
        if (!IsRunning) return;

        IsRunning = false;
        OnExit();
    }

    public void FastForward()
    {
        OnFastForward();
    }

    protected abstract void OnEnter();

    protected virtual void OnExit() { }

    protected virtual void OnFastForward() { }

    protected void Complete()
    {
        if (!IsRunning) return;

        Exit();
        Completed?.Invoke(this);
    }
}
