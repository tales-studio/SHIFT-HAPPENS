using UnityEngine;

public class WaitForPhoneStep : PromptedStep
{
    [SerializeField] private bool open = true;

    private PhoneView phone;

    protected override void BeginWait()
    {
        phone = FindFirstObjectByType<PhoneView>();
        if (phone == null || phone.IsOpen == open)
        {
            Complete();
            return;
        }

        phone.OnOpenChanged += HandleOpenChanged;
    }

    protected override void EndWait()
    {
        if (phone != null) phone.OnOpenChanged -= HandleOpenChanged;
        phone = null;
    }

    private void HandleOpenChanged(bool isOpen)
    {
        if (isOpen == open) Complete();
    }
}
