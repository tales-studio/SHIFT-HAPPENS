using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HighlightVFX : MonoBehaviour, IHighlightable
{
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField, Range(0f, 16f)] private float _outlineThickness = 3f;
    [SerializeField] private bool _previewInEditor;

    private static readonly int _ColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int _ThicknessID = Shader.PropertyToID("_OutlineThickness");
    private static readonly int _EnabledID = Shader.PropertyToID("_OutlineEnabled");

    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _propertyBlock;
    private bool _isHighlighted;

    private void OnEnable()
    {
        SetHighlighted(false);
    }

    public void SetHighlighted(bool highlighted)
    {
        _isHighlighted = highlighted;
        Apply();
    }

    private void Apply()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();

        bool visible = Application.isPlaying ? _isHighlighted : _previewInEditor;

        _spriteRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(_ColorID, _outlineColor);
        _propertyBlock.SetFloat(_ThicknessID, _outlineThickness);
        _propertyBlock.SetFloat(_EnabledID, visible ? 1f : 0f);
        _spriteRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void OnValidate()
    {
        Apply();
    }
}
