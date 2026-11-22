using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogueBox : MonoBehaviour, IDialoguePresenter, IPointerClickHandler
{
    private const int AllCharacters = 99999;

    [SerializeField] private GameObject box;
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private GameObject continueIndicator;
    [SerializeField, Min(1f)] private float charactersPerSecond = 40f;
    [SerializeField] private DialogueData previewDialogue;

    [Header("Input")]
    [SerializeField] private InputAction advanceAction = new InputAction("AdvanceDialogue", InputActionType.Button, "<Keyboard>/enter");

    private DialogueData current;
    private int lineIndex;
    private bool holdLastLine;
    private bool isTyping;
    private bool isHolding;
    private Action onFinished;
    private Coroutine typeRoutine;

    public bool IsShowing => current != null;

    private bool IsLastLine => lineIndex >= current.lines.Length - 1;

    private void Awake()
    {
        box.SetActive(false);
        DialogueServices.Register(this);
    }

    private void OnDestroy()
    {
        DialogueServices.Unregister(this);
    }

    private void OnEnable()
    {
        advanceAction.performed += HandleAdvance;
        advanceAction.Enable();
    }

    private void OnDisable()
    {
        advanceAction.performed -= HandleAdvance;
        advanceAction.Disable();

        typeRoutine = null;
        isTyping = false;
        if (lineText != null) lineText.maxVisibleCharacters = AllCharacters;
    }

    public void Play(DialogueData data, Action finished = null, bool holdLast = false)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            Hide();
            finished?.Invoke();
            return;
        }

        StopTyping();
        current = data;
        onFinished = finished;
        holdLastLine = holdLast;
        isHolding = false;
        lineIndex = 0;

        speakerText.text = data.speakerName;
        portrait.sprite = data.portrait;
        portrait.enabled = data.portrait != null;

        box.SetActive(true);
        ShowLine();
    }

    [ContextMenu("Play Preview")]
    private void PlayPreview()
    {
        if (Application.isPlaying) Play(previewDialogue);
    }

    public void Hide()
    {
        StopTyping();
        current = null;
        onFinished = null;
        isHolding = false;
        box.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) Advance();
    }

    private void HandleAdvance(InputAction.CallbackContext context)
    {
        Advance();
    }

    private void Advance()
    {
        if (current == null || isHolding) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        if (isTyping)
        {
            StopTyping();
            FinishTyping();
            return;
        }

        if (IsLastLine)
        {
            Action finished = onFinished;
            Hide();
            finished?.Invoke();
            return;
        }

        lineIndex++;
        ShowLine();
    }

    private void ShowLine()
    {
        lineText.text = current.lines[lineIndex];
        lineText.maxVisibleCharacters = 0;
        continueIndicator.SetActive(false);
        isTyping = true;
        typeRoutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        lineText.ForceMeshUpdate();
        int total = lineText.textInfo.characterCount;
        float shown = 0f;

        while (shown < total)
        {
            shown += charactersPerSecond * Time.deltaTime;
            lineText.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(shown));
            yield return null;
        }

        typeRoutine = null;
        FinishTyping();
    }

    private void StopTyping()
    {
        if (typeRoutine == null) return;

        StopCoroutine(typeRoutine);
        typeRoutine = null;
    }

    private void FinishTyping()
    {
        isTyping = false;
        lineText.maxVisibleCharacters = AllCharacters;

        if (current == null) return;

        if (holdLastLine && IsLastLine)
        {
            isHolding = true;
            Action finished = onFinished;
            onFinished = null;
            finished?.Invoke();
            return;
        }

        continueIndicator.SetActive(true);
    }
}
