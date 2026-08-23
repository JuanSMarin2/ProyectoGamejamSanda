using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CuttingShape
{
    Triangle,
    Square,
    Circle
}

public class CuttingController : MonoBehaviour
{
    [SerializeField] private CleaningObject baseCleaningObject;
    [SerializeField] private Sprite triangleCutMask;
    [SerializeField] private Sprite squareCutMask;
    [SerializeField] private Sprite circleCutMask;
    [SerializeField] private CuttingShape selectedShape = CuttingShape.Square;
    [Range(0.05f, 1f)]
    [SerializeField] private float cutSize = 0.25f;
    [Range(0.01f, 0.2f)]
    [SerializeField] private float cutSizeStep = 0.05f;
    [Range(0.05f, 1f)]
    [SerializeField] private float minimumCutSize = 0.05f;
    [Range(32, 512)]
    [SerializeField] private int previewResolution = 256;
    [SerializeField] private Color previewColor = new Color(1f, 1f, 1f, 0.35f);

    public event Action OnCuttingCompleted;
    public bool IsCuttingCompleted { get; private set; }

    private Camera mainCamera;
    private SpriteRenderer baseRenderer;
    private SpriteRenderer previewRenderer;
    private Texture2D previewTexture;
    private Sprite previewSprite;
    private Vector2 lastValidUv = new Vector2(0.5f, 0.5f);
    private bool hasValidPosition;
    private bool shapeSelected;
    private float lastPreviewSize;
    private bool ignoreCurrentClick;
    private Sprite selectedMaskSprite;
    private Sprite cachedMaskSprite;
    private Texture2D cachedReadableMaskTexture;

    private void OnEnable()
    {
        mainCamera = Camera.main;
        ResolveBase();
        IsCuttingCompleted = false;
        hasValidPosition = false;
        shapeSelected = false;
    }

    private void Update()
    {
        if (IsCuttingCompleted || mainCamera == null)
            return;

        if (shapeSelected)
        {
            UpdateCutSizeFromMouseWheel();
            UpdatePreviewPosition(Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero);
        }

        if (Mouse.current != null && !Mouse.current.leftButton.isPressed)
            ignoreCurrentClick = false;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            shapeSelected &&
            !ignoreCurrentClick)
            ConfirmCut();
    }

    private void OnDisable()
    {
        if (previewRenderer != null)
            previewRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        DestroyPreviewAssets();

        if (cachedReadableMaskTexture != null)
            Destroy(cachedReadableMaskTexture);
    }

    public void SelectTriangle()
    {
        SelectShape(CuttingShape.Triangle);
    }

    public void SelectSquare()
    {
        SelectShape(CuttingShape.Square);
    }

    public void SelectCircle()
    {
        SelectShape(CuttingShape.Circle);
    }

    public void SelectShape(CuttingShape shape)
    {
        if (IsCuttingCompleted)
            return;

        ResolveBase();
        selectedShape = shape;
        selectedMaskSprite = GetMaskSprite(shape);
        shapeSelected = baseRenderer != null;
        hasValidPosition = false;
        lastPreviewSize = cutSize;
        ignoreCurrentClick = Mouse.current != null && Mouse.current.leftButton.isPressed;
        CreatePreview();
    }

    public void ConfirmCut()
    {
        if (IsCuttingCompleted || !shapeSelected || !hasValidPosition || baseCleaningObject == null)
            return;

        if (selectedMaskSprite != null)
            baseCleaningObject.ApplyWorkArea(selectedMaskSprite, lastValidUv, cutSize);
        else
            baseCleaningObject.ApplyWorkArea(selectedShape, lastValidUv, cutSize);

        IsCuttingCompleted = true;
        shapeSelected = false;

        if (previewRenderer != null)
            previewRenderer.enabled = false;

        OnCuttingCompleted?.Invoke();
    }

    private void ResolveBase()
    {
        if (baseCleaningObject != null)
        {
            baseRenderer = baseCleaningObject.GetComponent<SpriteRenderer>();
            return;
        }

        ObjectData[] objects = FindObjectsByType<ObjectData>(FindObjectsSortMode.None);

        foreach (ObjectData objectData in objects)
        {
            if (!objectData.IsSelected || objectData.Category != PieceCategory.Base)
                continue;

            baseCleaningObject = objectData.GetComponentInParent<CleaningObject>();
            baseRenderer = baseCleaningObject != null
                ? baseCleaningObject.GetComponent<SpriteRenderer>()
                : null;
            return;
        }
    }

    private void UpdatePreviewPosition(Vector2 screenPosition)
    {
        if (baseRenderer == null)
            return;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(
            screenPosition.x,
            screenPosition.y,
            baseRenderer.transform.position.z - mainCamera.transform.position.z
        ));
        Vector2 localPosition = baseRenderer.transform.InverseTransformPoint(worldPosition);
        Bounds spriteBounds = baseRenderer.sprite.bounds;
        Vector2 uv = new Vector2(
            Mathf.InverseLerp(spriteBounds.min.x, spriteBounds.max.x, localPosition.x),
            Mathf.InverseLerp(spriteBounds.min.y, spriteBounds.max.y, localPosition.y)
        );

        float aspect = GetActiveMaskAspect(spriteBounds.size.x / spriteBounds.size.y);
        float halfWidth = cutSize * 0.5f;
        float halfHeight = halfWidth * (spriteBounds.size.x / spriteBounds.size.y) / aspect;
        float minimumX = halfWidth;
        float maximumX = 1f - halfWidth;
        float minimumY = halfHeight;
        float maximumY = 1f - halfHeight;

        if (minimumX > maximumX || minimumY > maximumY)
            return;

        if (uv.x >= minimumX && uv.x <= maximumX &&
            uv.y >= minimumY && uv.y <= maximumY)
        {
            lastValidUv = uv;
            hasValidPosition = true;
            if (previewRenderer != null)
            {
                previewRenderer.transform.localPosition = new Vector3(
                    Mathf.Lerp(spriteBounds.min.x, spriteBounds.max.x, uv.x),
                    Mathf.Lerp(spriteBounds.min.y, spriteBounds.max.y, uv.y),
                    0f
                );
                previewRenderer.enabled = true;
            }
        }
    }

    private void UpdateCutSizeFromMouseWheel()
    {
        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Approximately(scroll, 0f))
            return;

        cutSize = Mathf.Clamp(
            cutSize + Mathf.Sign(scroll) * cutSizeStep,
            minimumCutSize,
            1f
        );

        if (!Mathf.Approximately(cutSize, lastPreviewSize))
        {
            lastPreviewSize = cutSize;
            CreatePreview();
        }
    }

    private void CreatePreview()
    {
        if (!shapeSelected || baseRenderer == null || baseRenderer.sprite == null)
            return;

        DestroyPreviewAssets();
        if (selectedMaskSprite == null)
        {
            previewTexture = CreateShapeTexture(baseRenderer.sprite, selectedShape, cutSize, previewResolution);
        }
        else
        {
            previewTexture = CreateSpriteMaskPreviewTexture(
                baseRenderer.sprite,
                selectedMaskSprite,
                cutSize,
                previewResolution
            );
        }

        previewSprite = Sprite.Create(
            previewTexture,
            new Rect(0f, 0f, previewTexture.width, previewTexture.height),
            new Vector2(0.5f, 0.5f),
            previewTexture.width / baseRenderer.sprite.bounds.size.x
        );

        GameObject previewObject = new GameObject("CutPreview");
        previewObject.transform.SetParent(baseRenderer.transform, false);
        previewRenderer = previewObject.AddComponent<SpriteRenderer>();
        previewRenderer.sprite = previewSprite;
        previewRenderer.color = previewColor;
        previewRenderer.sortingLayerID = baseRenderer.sortingLayerID;
        previewRenderer.sortingOrder = baseRenderer.sortingOrder + 1;
        previewRenderer.enabled = hasValidPosition;
    }

    private Sprite GetMaskSprite(CuttingShape shape)
    {
        switch (shape)
        {
            case CuttingShape.Triangle:
                return triangleCutMask;
            case CuttingShape.Square:
                return squareCutMask;
            case CuttingShape.Circle:
                return circleCutMask;
            default:
                return null;
        }
    }

    private float GetActiveMaskAspect(float fallbackAspect)
    {
        Sprite activeSprite = selectedMaskSprite;

        if (activeSprite == null)
            return fallbackAspect;

        return activeSprite.rect.width / activeSprite.rect.height;
    }

    private Texture2D CreateSpriteMaskPreviewTexture(Sprite baseSprite, Sprite maskSprite, float size, int resolution)
    {
        float baseAspect = baseSprite.bounds.size.x / baseSprite.bounds.size.y;
        int height = Mathf.Max(1, Mathf.RoundToInt(resolution / baseAspect));
        Texture2D texture = new Texture2D(resolution, height, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[resolution * height];
        Texture2D readableMaskTexture = GetReadableMaskTexture(maskSprite);

        if (readableMaskTexture == null)
            return texture;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 position = new Vector2(
                    (x + 0.5f) / resolution,
                    (y + 0.5f) / height
                );
                bool inside = IsInsideSpriteMaskShape(
                    position,
                    new Vector2(0.5f, 0.5f),
                    size,
                    baseAspect,
                    maskSprite.rect.width / maskSprite.rect.height,
                    readableMaskTexture
                );
                pixels[y * resolution + x] = inside ? Color.white : Color.clear;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        return texture;
    }

    private bool IsInsideSpriteMaskShape(
        Vector2 position,
        Vector2 center,
        float size,
        float baseAspect,
        float maskAspect,
        Texture2D readableMaskTexture)
    {
        float halfWidth = size * 0.5f;
        float halfHeight = halfWidth * baseAspect / maskAspect;
        Vector2 localPosition = new Vector2(
            (position.x - center.x) / halfWidth,
            (position.y - center.y) / halfHeight
        );
        Vector2 shapeUv = new Vector2(localPosition.x * 0.5f + 0.5f, localPosition.y * 0.5f + 0.5f);

        if (shapeUv.x < 0f || shapeUv.x > 1f || shapeUv.y < 0f || shapeUv.y > 1f)
            return false;

        return readableMaskTexture.GetPixelBilinear(shapeUv.x, shapeUv.y).a > 0f;
    }

    private Texture2D GetReadableMaskTexture(Sprite maskSprite)
    {
        if (maskSprite == null)
            return null;

        if (cachedMaskSprite == maskSprite && cachedReadableMaskTexture != null)
            return cachedReadableMaskTexture;

        if (cachedReadableMaskTexture != null)
            Destroy(cachedReadableMaskTexture);

        cachedMaskSprite = maskSprite;
        cachedReadableMaskTexture = BuildReadableMaskTexture(maskSprite);
        return cachedReadableMaskTexture;
    }

    private Texture2D BuildReadableMaskTexture(Sprite maskSprite)
    {
        int width = Mathf.Max(1, Mathf.RoundToInt(maskSprite.rect.width));
        int height = Mathf.Max(1, Mathf.RoundToInt(maskSprite.rect.height));
        RenderTexture temporaryRt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        RenderTexture previous = RenderTexture.active;

        RenderTexture.active = temporaryRt;
        GL.Clear(true, true, Color.clear);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, width, 0, height);

        Rect sourceRect = maskSprite.textureRect;
        Texture sourceTexture = maskSprite.texture;
        Rect sourceUv = new Rect(
            sourceRect.x / sourceTexture.width,
            sourceRect.y / sourceTexture.height,
            sourceRect.width / sourceTexture.width,
            sourceRect.height / sourceTexture.height
        );

        Graphics.DrawTexture(
            new Rect(0, 0, width, height),
            sourceTexture,
            sourceUv,
            0,
            0,
            0,
            0,
            Color.white
        );

        GL.PopMatrix();

        Texture2D readable = new Texture2D(width, height, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        readable.Apply(false, false);

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temporaryRt);
        return readable;
    }

    private Texture2D CreateShapeTexture(Sprite sourceSprite, CuttingShape shape, float size, int resolution)
    {
        float aspect = sourceSprite.bounds.size.x / sourceSprite.bounds.size.y;
        int height = Mathf.Max(1, Mathf.RoundToInt(resolution / aspect));
        Texture2D texture = new Texture2D(resolution, height, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[resolution * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 position = new Vector2(
                    (x + 0.5f) / resolution,
                    (y + 0.5f) / height
                );
                bool inside = IsInsideShape(position, new Vector2(0.5f, 0.5f), size, aspect, shape);
                pixels[y * resolution + x] = inside ? Color.white : Color.clear;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        return texture;
    }

    private bool IsInsideShape(Vector2 position, Vector2 center, float size, float aspect, CuttingShape shape)
    {
        float halfWidth = size * 0.5f;
        float halfHeight = halfWidth / aspect;
        Vector2 localPosition = new Vector2(
            (position.x - center.x) / halfWidth,
            (position.y - center.y) / halfHeight
        );

        switch (shape)
        {
            case CuttingShape.Triangle:
                return IsInsideTriangle(
                    localPosition,
                    new Vector2(0f, -1f),
                    new Vector2(-1f, 1f),
                    new Vector2(1f, 1f)
                );
            case CuttingShape.Square:
                return Mathf.Abs(localPosition.x) <= 1f && Mathf.Abs(localPosition.y) <= 1f;
            case CuttingShape.Circle:
                return localPosition.sqrMagnitude <= 1f;
            default:
                return false;
        }
    }

    private bool IsInsideTriangle(Vector2 point, Vector2 first, Vector2 second, Vector2 third)
    {
        float firstSide = Cross(third - first, point - first);
        float secondSide = Cross(third - second, point - second);
        float thirdSide = Cross(second - first, point - first);
        bool hasNegative = firstSide < 0f || secondSide < 0f || thirdSide < 0f;
        bool hasPositive = firstSide > 0f || secondSide > 0f || thirdSide > 0f;
        return !(hasNegative && hasPositive);
    }

    private float Cross(Vector2 first, Vector2 second)
    {
        return first.x * second.y - first.y * second.x;
    }

    private void DestroyPreviewAssets()
    {
        if (previewRenderer != null)
            Destroy(previewRenderer.gameObject);

        if (previewSprite != null)
            Destroy(previewSprite);

        if (previewTexture != null)
            Destroy(previewTexture);

        previewRenderer = null;
        previewSprite = null;
        previewTexture = null;
    }
}
