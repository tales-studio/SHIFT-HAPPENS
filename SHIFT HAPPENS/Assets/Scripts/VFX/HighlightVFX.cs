using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HighlightVFX : MonoBehaviour, IHighlightable
{
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField, Range(0f, 64f)] private float _outlineThickness = 8f;
    [SerializeField] private int _outlineSortingOrderOffset = 50;
    [SerializeField] private bool _previewInEditor;

    private static readonly int _ColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int _ThicknessID = Shader.PropertyToID("_OutlineThickness");
    private static readonly int _EnabledID = Shader.PropertyToID("_OutlineEnabled");
    private static readonly int _OnlyID = Shader.PropertyToID("_OutlineOnly");

    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _outlineRenderer;
    private MaterialPropertyBlock _propertyBlock;
    private bool _isHighlighted;

    private void OnEnable()
    {
        SetHighlighted(false);
    }

    private void OnDisable()
    {
        if (_outlineRenderer != null) _outlineRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        if (_outlineRenderer != null) Destroy(_outlineRenderer.gameObject);
    }

    public void SetHighlighted(bool highlighted)
    {
        _isHighlighted = highlighted;
        Apply();
    }

    private void LateUpdate()
    {
        if (_isHighlighted && _outlineRenderer != null) SyncOutlineRenderer();
    }

    private void Apply()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();

        if (Application.isPlaying)
        {
            SetOutlineProperties(_spriteRenderer, false, false);

            if (_isHighlighted && _outlineRenderer == null) CreateOutlineRenderer();
            if (_outlineRenderer == null) return;

            _outlineRenderer.enabled = _isHighlighted;
            if (!_isHighlighted) return;

            SyncOutlineRenderer();
            SetOutlineProperties(_outlineRenderer, true, true);
        }
        else
        {
            SetOutlineProperties(_spriteRenderer, _previewInEditor, false);
        }
    }

    private void SetOutlineProperties(SpriteRenderer target, bool enabled, bool outlineOnly)
    {
        target.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(_ColorID, _outlineColor);
        _propertyBlock.SetFloat(_ThicknessID, _outlineThickness);
        _propertyBlock.SetFloat(_EnabledID, enabled ? 1f : 0f);
        _propertyBlock.SetFloat(_OnlyID, outlineOnly ? 1f : 0f);
        target.SetPropertyBlock(_propertyBlock);
    }

    private void CreateOutlineRenderer()
    {
        GameObject outline = new GameObject($"{name}_Outline");
        outline.transform.SetParent(transform, false);
        _outlineRenderer = outline.AddComponent<SpriteRenderer>();
        _outlineRenderer.sharedMaterial = _spriteRenderer.sharedMaterial;
        _outlineRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
    }

    private void SyncOutlineRenderer()
    {
        _outlineRenderer.sprite = _spriteRenderer.sprite;
        _outlineRenderer.flipX = _spriteRenderer.flipX;
        _outlineRenderer.flipY = _spriteRenderer.flipY;
        _outlineRenderer.drawMode = _spriteRenderer.drawMode;
        _outlineRenderer.size = _spriteRenderer.size;
        _outlineRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
        _outlineRenderer.sortingOrder = _spriteRenderer.sortingOrder + _outlineSortingOrderOffset;
    }

    private void OnValidate()
    {
        Apply();
    }
}
