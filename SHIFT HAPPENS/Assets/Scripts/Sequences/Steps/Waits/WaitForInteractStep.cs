using UnityEngine;

public class WaitForInteractStep : PromptedStep
{
    [SerializeField] private InteractableObject target;
    [SerializeField] private bool disableAfter = true;

    protected override void BeginWait()
    {
        if (target == null)
        {
            Complete();
            return;
        }

        target.SetCanInteract(true);
        target.onInteract.AddListener(HandleInteract);
    }

    protected override void EndWait()
    {
        if (target == null) return;

        target.onInteract.RemoveListener(HandleInteract);
        if (disableAfter) target.SetCanInteract(false);
    }

    private void HandleInteract()
    {
        Complete();
    }
}
