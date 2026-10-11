using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutlineEffect : MonoBehaviour
{
    [Header("Configuración de Iluminación (Bordes)")]
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 1f;

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        SetOutlineActive(false);
    }

    public void SetOutlineActive(bool active)
    {
        if (spriteRenderer == null) return;

        spriteRenderer.GetPropertyBlock(propertyBlock);

        if (active)
        {
            propertyBlock.SetColor("_OutlineColor", outlineColor);
            propertyBlock.SetFloat("_OutlineWidth", outlineWidth);
        }
        else
        {
            propertyBlock.SetFloat("_OutlineWidth", 0f);
        }

        spriteRenderer.SetPropertyBlock(propertyBlock);
    }
}