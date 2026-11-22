using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShotStep : SequenceStep
{
    [SerializeField] private CinemachineCamera shot;
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private bool returnToPlayer = true;
    [SerializeField, Min(0f)] private float fadeDuration = 0.4f;

    private static CinemachineCamera activeShot;

    private Coroutine routine;
    private bool finished;

    protected override void OnEnter()
    {
        finished = false;
        routine = StartCoroutine(CutTo(shot, PlayDialogue));
    }

    protected override void OnExit()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        DialogueServices.Presenter.Hide();

        if (!finished)
        {
            SetActiveShot(null);
            ClearFade();
        }
    }

    protected override void OnFastForward()
    {
        SetActiveShot(null);
        ClearFade();
    }

    private void PlayDialogue()
    {
        DialogueServices.Presenter.Play(dialogue, HandleDialogueFinished);
    }

    private void HandleDialogueFinished()
    {
        if (!IsRunning) return;

        if (returnToPlayer) routine = StartCoroutine(CutTo(null, Finish));
        else Finish();
    }

    private void Finish()
    {
        finished = true;
        Complete();
    }

    private IEnumerator CutTo(CinemachineCamera target, System.Action then)
    {
        IScreenFader fader = SceneServices.Fader;

        yield return fader.FadeOut(fadeDuration);
        SetActiveShot(target);
        yield return null;
        yield return fader.FadeIn(fadeDuration);

        routine = null;
        then();
    }

    private static void SetActiveShot(CinemachineCamera target)
    {
        if (activeShot == target) return;

        if (activeShot != null) activeShot.gameObject.SetActive(false);
        activeShot = target;
        if (activeShot != null) activeShot.gameObject.SetActive(true);

        Camera main = Camera.main;
        CinemachineBrain brain = main != null ? main.GetComponent<CinemachineBrain>() : null;
        if (brain != null) brain.ResetState();
    }

    private static void ClearFade()
    {
        IEnumerator fade = SceneServices.Fader.FadeIn(0f);
        while (fade.MoveNext()) { }
    }
}
