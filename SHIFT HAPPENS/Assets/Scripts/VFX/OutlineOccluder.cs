using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OutlineOccluder : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float _alphaCutoff = 0.2f;

    private SpriteRenderer _spriteRenderer;
    private SpriteMask _mask;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject maskObject = new GameObject($"{name}_OutlineMask");
        maskObject.transform.SetParent(transform, false);
        _mask = maskObject.AddComponent<SpriteMask>();
        _mask.alphaCutoff = _alphaCutoff;
    }

    private void OnEnable()
    {
        if (_mask != null) _mask.enabled = true;
    }

    private void OnDisable()
    {
        if (_mask != null) _mask.enabled = false;
    }

    private void LateUpdate()
    {
        _mask.sprite = _spriteRenderer.sprite;
        _mask.enabled = _spriteRenderer.enabled;
        _mask.transform.localScale = new Vector3(
            _spriteRenderer.flipX ? -1f : 1f,
            _spriteRenderer.flipY ? -1f : 1f,
            1f);
    }
}
