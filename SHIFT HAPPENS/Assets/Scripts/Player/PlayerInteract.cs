using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private Collider2D interactionArea;

    [Tooltip("Layers whose colliders count as interactable. Exclude helper triggers like FadeZone.")]
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("Sign")]
    [SerializeField] private string signText = "E";
    [SerializeField] private TMP_FontAsset signFont;
    [SerializeField] private float signFontSize = 8f;
    [SerializeField] private Color signColor = Color.white;
    [SerializeField] private Material signMaterial;
    [SerializeField] private float signHeightOffset = 0.6f;
    [SerializeField] private int signSortingOrder = 500;
    [SerializeField] private Color signBackgroundColor = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private Vector2 signBackgroundPadding = new Vector2(0.3f, 0.18f);

    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private readonly HashSet<object> locks = new HashSet<object>();
    private ContactFilter2D filter;
    private TextMeshPro sign;

    public bool IsLocked => locks.Count > 0;

    public void AddLock(object source)
    {
        locks.Add(source);
    }

    public void RemoveLock(object source)
    {
        locks.Remove(source);
    }

    private void Awake()
    {
        filter = new ContactFilter2D().NoFilter();
        filter.SetLayerMask(interactableLayers);
        filter.useTriggers = true;

        CreateSign();
    }

    private void OnDisable()
    {
        if (sign != null) sign.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (sign != null) Destroy(sign.gameObject);
    }

    private void LateUpdate()
    {
        IInteractable closest = IsLocked ? null : GetClosestInteractable();

        if (closest == null)
        {
            sign.gameObject.SetActive(false);
            return;
        }

        sign.transform.position = closest.SignPosition + Vector3.up * signHeightOffset;
        sign.gameObject.SetActive(true);
    }

    public void OnInteract()
    {
        if (IsLocked) return;

        IInteractable closest = GetClosestInteractable();

        if (closest != null)
        {
            closest.Interact();
        }
    }

    private void CreateSign()
    {
        GameObject signObject = new GameObject("InteractSign");
        sign = signObject.AddComponent<TextMeshPro>();
        if (signFont != null) sign.font = signFont;
        sign.text = signText;
        sign.fontSize = signFontSize;
        sign.fontStyle = FontStyles.Bold;
        sign.color = signColor;
        sign.alignment = TextAlignmentOptions.Bottom;
        sign.textWrappingMode = TextWrappingModes.NoWrap;
        sign.rectTransform.sizeDelta = new Vector2(2f, 2f);
        sign.rectTransform.pivot = new Vector2(0.5f, 0f);
        if (signMaterial != null) sign.fontSharedMaterial = signMaterial;
        sign.sortingOrder = signSortingOrder;
        CreateSignBackground();
        signObject.SetActive(false);
    }

    private void CreateSignBackground()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

        GameObject background = new GameObject("Background");
        background.transform.SetParent(sign.transform, false);

        SpriteRenderer backgroundRenderer = background.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = sprite;
        backgroundRenderer.color = new Color(signBackgroundColor.r, signBackgroundColor.g, signBackgroundColor.b, Mathf.Min(signBackgroundColor.a, 0.9f));
        backgroundRenderer.sortingOrder = signSortingOrder - 1;

        sign.ForceMeshUpdate();
        Bounds textBounds = sign.textBounds;
        background.transform.localPosition = new Vector3(textBounds.center.x, textBounds.center.y, 0f);
        background.transform.localScale = new Vector3(
            textBounds.size.x + signBackgroundPadding.x * 2f,
            textBounds.size.y + signBackgroundPadding.y * 2f,
            1f);
    }

    private IInteractable GetClosestInteractable()
    {
        IInteractable closest = null;
        float minSqrDistance = float.MaxValue;
        Vector2 position = transform.position;

        interactionArea.Overlap(filter, overlaps);

        foreach (Collider2D overlap in overlaps)
        {
            IInteractable interactable = overlap.GetComponentInParent<IInteractable>();
            if (interactable == null || !interactable.CanInteract) continue;

            float sqrDistance = (position - (Vector2)interactable.GetTransform().position).sqrMagnitude;

            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                closest = interactable;
            }
        }
        return closest;
    }
}
