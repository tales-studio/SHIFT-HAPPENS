using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Sequence : MonoBehaviour
{
    private readonly List<SequenceStep> steps = new List<SequenceStep>();
    private SequenceStep current;
    private int index = -1;
    private Action onFinished;

    public bool IsPlaying { get; private set; }

    public event Action Started;
    public event Action Ended;

    public void Play(Action finished = null)
    {
        if (IsPlaying) return;

        CollectSteps();
        onFinished = finished;
        IsPlaying = true;
        index = -1;
        Started?.Invoke();
        Next();
    }

    public void FastForward(Action finished = null)
    {
        int start;

        if (IsPlaying)
        {
            start = Mathf.Max(index, 0);
            ReleaseCurrent(true);
        }
        else
        {
            CollectSteps();
            start = 0;
            onFinished = finished;
        }

        for (int i = start; i < steps.Count; i++)
        {
            steps[i].FastForward();
        }

        Finish();
    }

    public void Stop()
    {
        if (!IsPlaying) return;

        IsPlaying = false;
        onFinished = null;
        ReleaseCurrent(true);
        Ended?.Invoke();
    }

    private void CollectSteps()
    {
        steps.Clear();

        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf) continue;
            steps.AddRange(child.GetComponents<SequenceStep>());
        }
    }

    private void Next()
    {
        ReleaseCurrent(false);

        index++;
        if (index >= steps.Count)
        {
            Finish();
            return;
        }

        current = steps[index];
        current.Completed += HandleStepCompleted;
        current.Enter();
    }

    private void ReleaseCurrent(bool exit)
    {
        if (current == null) return;

        current.Completed -= HandleStepCompleted;
        if (exit) current.Exit();
        current = null;
    }

    private void HandleStepCompleted(SequenceStep step)
    {
        if (IsPlaying && step == current) Next();
    }

    private void Finish()
    {
        IsPlaying = false;

        Action finished = onFinished;
        onFinished = null;
        Ended?.Invoke();
        finished?.Invoke();
    }
}
