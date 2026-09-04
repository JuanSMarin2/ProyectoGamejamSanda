using UnityEngine;

public class PaintableObject : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Painting")]
    [Range(0f, 1f)]
    [SerializeField] private float paintAmount = 0f;

    private MaterialPropertyBlock propertyBlock;

    private int paintColorPropertyId;
    private int paintAmountPropertyId;

    private Color paintColor = Color.white;

    private bool interactionEnabled;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError(
                $"[PAINTING] {name}: no tiene SpriteRenderer."
            );

            enabled = false;
            return;
        }

        propertyBlock = new MaterialPropertyBlock();

        paintColorPropertyId =
            Shader.PropertyToID("_Paint_Color");

        paintAmountPropertyId =
            Shader.PropertyToID("_Paint_Amount");

        ApplyPaintProperties();
    }

    // =========================================================
    // INTERACCIÓN
    // =========================================================

    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;
    }

    // =========================================================
    // COLOR
    // =========================================================

    public void SetPaintColor(Color color)
    {
        paintColor = color;

        ApplyPaintProperties();
    }

    // =========================================================
    // CUBETA
    // =========================================================

    public bool FillWithPaint()
    {
        if (!interactionEnabled)
            return false;

        if (IsPainted())
            return false;

        paintAmount = 1f;

        ApplyPaintProperties();

        Debug.Log(
            $"[PAINTING] {name} pintado de {paintColor}"
        );

        return true;
    }

    // =========================================================
    // PROPIEDADES DEL SHADER
    // =========================================================

    private void ApplyPaintProperties()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetColor(
            paintColorPropertyId,
            paintColor
        );

        propertyBlock.SetFloat(
            paintAmountPropertyId,
            paintAmount
        );

        spriteRenderer.SetPropertyBlock(
            propertyBlock
        );
    }

    // =========================================================
    // ESTADO
    // =========================================================

    public bool IsPainted()
    {
        return paintAmount >= 0.99f;
    }
}