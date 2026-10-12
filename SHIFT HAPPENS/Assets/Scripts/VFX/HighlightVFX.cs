using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HighlightVFX : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float _activationDistance = 10f;
    [SerializeField] private Transform _playerTransform;

    [Header("Outline Appearance")]
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private float _outlineThickness = 20f;

    [Header("Performance")]
    [SerializeField] private float _checkInterval = 0.2f;

    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _propertyBlock;
    private bool _isOutlined = true;
    private float _timer;

    private static readonly int _ColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int _ThicknessID = Shader.PropertyToID("_OutlineThickness");
    private static readonly int _EnabledID = Shader.PropertyToID("_OutlineEnabled");

    public void DisableHighlight()
    {
        _isOutlined = false;
        UpdateOutlineState(); 
        enabled = false;     
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _propertyBlock = new MaterialPropertyBlock();

        TryFindPlayer();
        EvaluateDistance(true);
    }

    private void Update()
    {
        if (_playerTransform == null)
        {
            TryFindPlayer();
            if (_playerTransform == null) return;
        }

        _timer += Time.deltaTime;
        if (_timer < _checkInterval) return;
        _timer = 0f;

        EvaluateDistance(false);
    }

    private void TryFindPlayer()
    {
        if (_playerTransform != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }
    }

    private void EvaluateDistance(bool forceUpdate)
    {
        if (_playerTransform == null) return;

        Vector2 currentObjPos = _spriteRenderer != null ? (Vector2)_spriteRenderer.bounds.center : (Vector2)transform.position;

        float sqrDistance = (currentObjPos - (Vector2)_playerTransform.position).sqrMagnitude;
        bool shouldOutline = sqrDistance <= (_activationDistance * _activationDistance);

        if (shouldOutline != _isOutlined || forceUpdate)
        {
            _isOutlined = shouldOutline;
            UpdateOutlineState();
        }
    }

    private void UpdateOutlineState()
    {
        if (_spriteRenderer == null) return;

        _spriteRenderer.GetPropertyBlock(_propertyBlock);

        if (_isOutlined)
        {
            _propertyBlock.SetColor(_ColorID, _outlineColor);
            _propertyBlock.SetFloat(_ThicknessID, _outlineThickness);
            _propertyBlock.SetFloat(_EnabledID, 1f);
        }
        else
        {
            _propertyBlock.SetFloat(_EnabledID, 0f);
        }

        _spriteRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void OnValidate()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_propertyBlock == null)
            _propertyBlock = new MaterialPropertyBlock();

        if (_spriteRenderer != null)
        {
            UpdateOutlineState();
        }
    }

    private void OnDrawGizmosSelected()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 drawCenter = sr != null ? sr.bounds.center : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(drawCenter, _activationDistance);
    }
}