using UnityEngine;

public class ObjectOutlineHighlighter : MonoBehaviour, IHighlightable
{
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private float _outlineWidth = 2f; 

    private Renderer _objectRenderer;
    private MaterialPropertyBlock _propBlock;
    private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthProperty = Shader.PropertyToID("_OutlineWidth");

    private void Awake()
    {
        _objectRenderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    public void SetHighlight(bool active)
    {
        if (_objectRenderer == null) return;

        _objectRenderer.GetPropertyBlock(_propBlock);

        _propBlock.SetColor(OutlineColorProperty, active ? _outlineColor : Color.clear);
        _propBlock.SetFloat(OutlineWidthProperty, active ? _outlineWidth : 0f);

        _objectRenderer.SetPropertyBlock(_propBlock);
    }

    private void OnDisable()
    {
        SetHighlight(false);
    }
}