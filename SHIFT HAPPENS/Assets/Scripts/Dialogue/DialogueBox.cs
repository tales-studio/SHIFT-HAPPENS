using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogueBox : MonoBehaviour, IDialoguePresenter, IPointerClickHandler
{
    private const int AllCharacters = 99999;
    private static readonly Regex HighlightPattern = new Regex(@"\*(.+?)\*");
    private static readonly Regex TokenPattern = new Regex(@"\{(\w+)\}");

    [SerializeField] private GameObject box;
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private GameObject continueIndicator;
    [SerializeField, Min(1f)] private float charactersPerSecond = 40f;
    [SerializeField] private Color highlightColor = new Color(0.45f, 0.85f, 1f, 1f);
    [SerializeField] private DialogueData previewDialogue;

    [Header("Input")]
    [SerializeField] private InputActionReference advanceAction;

    private DialogueData current;
    private int lineIndex;
    private bool holdLastLine;
    private bool isTyping;
    private bool isHolding;
    private Action onFinished;
    private Coroutine typeRoutine;
    private InputPrompts prompts;
    private bool promptsChanged;

    public bool IsShowing => current != null;

    private bool IsLastLine => lineIndex >= current.Lines.Length - 1;

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
        advanceAction.action.performed += HandleAdvance;

        prompts = InputPrompts.Instance;
        if (prompts != null) prompts.Changed += HandlePromptsChanged;
    }

    private void OnDisable()
    {
        advanceAction.action.performed -= HandleAdvance;
        if (prompts != null) prompts.Changed -= HandlePromptsChanged;

        typeRoutine = null;
        isTyping = false;
        if (lineText != null) lineText.maxVisibleCharacters = AllCharacters;
    }

    public void Play(DialogueData data, Action finished = null, bool holdLast = false)
    {
        if (data == null || data.Lines == null || data.Lines.Length == 0)
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
        lineText.text = FormatLine(current.Lines[lineIndex]);
        promptsChanged = false;
        lineText.maxVisibleCharacters = 0;
        continueIndicator.SetActive(false);
        isTyping = true;
        typeRoutine = StartCoroutine(TypeLine());
    }

    private string FormatLine(string line)
    {
        return ApplyHighlights(ApplyTokens(line));
    }

    private string ApplyTokens(string line)
    {
        if (prompts == null) return line;

        return TokenPattern.Replace(line, match =>
            prompts.TryGetLabel(match.Groups[1].Value, out string label) ? "*" + label + "*" : match.Value);
    }

    private void HandlePromptsChanged()
    {
        if (current == null) return;

        if (isTyping)
        {
            promptsChanged = true;
            return;
        }

        lineText.text = FormatLine(current.Lines[lineIndex]);
    }

    private string ApplyHighlights(string line)
    {
        string hex = ColorUtility.ToHtmlStringRGBA(highlightColor);
        return HighlightPattern.Replace(line, "<color=#" + hex + ">$1</color>");
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

        if (promptsChanged)
        {
            promptsChanged = false;
            lineText.text = FormatLine(current.Lines[lineIndex]);
        }

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
