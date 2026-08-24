using UnityEngine;
using UnityEngine.InputSystem;
using FMOD.Studio;
using FMODUnity;

public class CleaningObject : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private EventInstance sandingInstance;
    private bool sandingInstanceCreated;
    private bool isSanding;
    private Vector2 lastMousePosition;

    [SerializeField] private RenderTexture cleaningMask;
    [SerializeField] private string cleaningMaskPropertyName = "_Cleaning_Mask";
    [SerializeField] private Texture2D brushTexture;

    [Range(0.001f, 0.5f)]
    public float brushSize = 0.1f;

    [Range(0f, 100f)]
    [SerializeField] private float cleaningPercentageThreshold = 97f;

    public float CleaningPercentage { get; private set; }
    public bool IsFullyCleaned => CleaningPercentage >= cleaningPercentageThreshold;

    private Camera mainCamera;
    private RenderTexture maskInstance;
    private RenderTexture tempMask;
    private MaterialPropertyBlock propertyBlock;
    private int cleaningMaskPropertyId;
    private int cleaningMaskSTPropertyId;
    private Texture2D maskReadback;
    private Texture2D workAreaMask;
    private Texture2D workAreaMaskForSpriteMask;
    private SpriteMask workAreaSpriteMask;
    private Sprite cachedShapeSprite;
    private Texture2D cachedReadableShapeTexture;
    private bool interactionEnabled = true;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        cleaningMaskPropertyId = Shader.PropertyToID(cleaningMaskPropertyName);
        cleaningMaskSTPropertyId = Shader.PropertyToID(cleaningMaskPropertyName + "_ST");

        if (mainCamera == null || spriteRenderer == null || cleaningMask == null || brushTexture == null)
        {
            enabled = false;
            return;
        }

        maskInstance = CreateMaskInstance(cleaningMask, "_Instance");
        tempMask = CreateMaskInstance(cleaningMask, "_Temp");
        ClearMask(maskInstance);
        ClearMask(tempMask);

        maskReadback = new Texture2D(maskInstance.width, maskInstance.height, TextureFormat.RGBA32, false);
        ConfigureMaskUV();
        InitializeSandingAudio();
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            StopSandingIfNeeded();
            return;
        }

        if (!interactionEnabled)
        {
            StopSandingIfNeeded();
            return;
        }

        if (!Mouse.current.leftButton.isPressed)
        {
            StopSandingIfNeeded();
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (!isSanding)
        {
            StartSanding();
        }

        UpdateSandingIntensity(mousePosition);
        PaintFromScreenPosition(mousePosition, mainCamera);
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        interactionEnabled = isEnabled;
    }

    private void OnDestroy()
    {
        StopSanding();

        if (maskInstance != null)
        {
            maskInstance.Release();
            Destroy(maskInstance);
        }

        if (tempMask != null)
        {
            tempMask.Release();
            Destroy(tempMask);
        }

        if (maskReadback != null)
            Destroy(maskReadback);

        if (workAreaMask != null)
            Destroy(workAreaMask);

        if (workAreaMaskForSpriteMask != null)
            Destroy(workAreaMaskForSpriteMask);

        if (workAreaSpriteMask != null)
        {
            if (workAreaSpriteMask.sprite != null)
                Destroy(workAreaSpriteMask.sprite);

            Destroy(workAreaSpriteMask.gameObject);
        }

        if (cachedReadableShapeTexture != null)
            Destroy(cachedReadableShapeTexture);
    }

    private void InitializeSandingAudio()
    {
        if (sandingInstanceCreated && sandingInstance.isValid())
            return;

        if (AudioManager.instance == null)
            return;

        if (FMODEvents.instance == null)
            return;

        if (FMODEvents.instance.lijar.IsNull)
            return;

        sandingInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.lijar);
        sandingInstanceCreated = sandingInstance.isValid();
    }

    private void StartSanding()
    {
        if (!sandingInstance.isValid())
            return;

        PLAYBACK_STATE playbackState;
        sandingInstance.getPlaybackState(out playbackState);

        if (isSanding || playbackState == PLAYBACK_STATE.PLAYING || playbackState == PLAYBACK_STATE.STARTING)
            return;

        isSanding = true;
        lastMousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        sandingInstance.start();
    }

    private void UpdateSandingIntensity(Vector2 currentMousePosition)
    {
        if (!sandingInstance.isValid())
            return;

        float mouseSpeed = (currentMousePosition - lastMousePosition).magnitude / Mathf.Max(Time.deltaTime, 0.01f);
        lastMousePosition = currentMousePosition;

        float intensity = Mathf.Clamp01(mouseSpeed / 1000f);
        sandingInstance.setParameterByName("DragIntensity", intensity);
    }

    private void StopSandingIfNeeded()
    {
        if (isSanding)
            StopSanding();
    }

    private void StopSanding()
    {
        if (!sandingInstance.isValid())
        {
            isSanding = false;
            return;
        }

        isSanding = false;
        sandingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void RefreshCleaningPercentage()
    {
        if (maskInstance == null || maskReadback == null)
            return;

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = maskInstance;
        maskReadback.ReadPixels(new Rect(0, 0, maskInstance.width, maskInstance.height), 0, 0, false);
        maskReadback.Apply(false, false);
        RenderTexture.active = previous;

        Color32[] pixels = maskReadback.GetPixels32();
        Color[] areaPixels = workAreaMask != null ? workAreaMask.GetPixels() : null;
        int paintedPixels = 0;
        int validPixels = 0;

        for (int index = 0; index < pixels.Length; index++)
        {
            bool isInsideWorkArea = areaPixels == null || areaPixels[index].a > 0f;

            if (isInsideWorkArea)
            {
                validPixels++;

                if (pixels[index].r > 0)
                    paintedPixels++;
            }
        }

        CleaningPercentage = validPixels == 0
            ? 0f
            : 100f * paintedPixels / validPixels;
    }

    public void ApplyWorkArea(Texture2D newWorkAreaMask)
    {
        if (newWorkAreaMask == null || maskInstance == null)
            return;

        if (workAreaMask != null)
            Destroy(workAreaMask);

        workAreaMask = newWorkAreaMask;
        ConfigureWorkAreaSpriteMask();
        RefreshCleaningPercentage();
    }

    public void ApplyWorkArea(CuttingShape shape, Vector2 center, float size)
    {
        if (maskInstance == null)
            return;

        Texture2D newWorkAreaMask = new Texture2D(
            maskInstance.width,
            maskInstance.height,
            TextureFormat.RGBA32,
            false
        );
        Color32[] pixels = new Color32[maskInstance.width * maskInstance.height];

        for (int y = 0; y < maskInstance.height; y++)
        {
            for (int x = 0; x < maskInstance.width; x++)
            {
                Vector2 normalizedPosition = new Vector2(
                    (x + 0.5f) / maskInstance.width,
                    (y + 0.5f) / maskInstance.height
                );
                bool isInside = IsInsideShape(normalizedPosition, center, size, shape);
                pixels[y * maskInstance.width + x] = isInside ? Color.white : Color.clear;
            }
        }

        newWorkAreaMask.SetPixels32(pixels);
        newWorkAreaMask.Apply(false, false);
        ApplyWorkArea(newWorkAreaMask);
    }

    public void ApplyWorkArea(Sprite shapeSprite, Vector2 center, float size)
    {
        if (maskInstance == null || shapeSprite == null)
            return;

        Texture2D readableShapeTexture = GetReadableShapeTexture(shapeSprite);

        if (readableShapeTexture == null)
            return;

        Texture2D newWorkAreaMask = new Texture2D(
            maskInstance.width,
            maskInstance.height,
            TextureFormat.RGBA32,
            false
        );
        Color32[] pixels = new Color32[maskInstance.width * maskInstance.height];

        for (int y = 0; y < maskInstance.height; y++)
        {
            for (int x = 0; x < maskInstance.width; x++)
            {
                Vector2 normalizedPosition = new Vector2(
                    (x + 0.5f) / maskInstance.width,
                    (y + 0.5f) / maskInstance.height
                );
                bool isInside = IsInsideSpriteShape(normalizedPosition, center, size, shapeSprite, readableShapeTexture);
                pixels[y * maskInstance.width + x] = isInside ? Color.white : Color.clear;
            }
        }

        newWorkAreaMask.SetPixels32(pixels);
        newWorkAreaMask.Apply(false, false);
        ApplyWorkArea(newWorkAreaMask);
    }

    private bool IsInsideShape(Vector2 position, Vector2 center, float size, CuttingShape shape)
    {
        float aspect = spriteRenderer.sprite.rect.width / spriteRenderer.sprite.rect.height;
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

    private bool IsInsideSpriteShape(Vector2 position, Vector2 center, float size, Sprite shapeSprite, Texture2D readableShapeTexture)
    {
        float baseAspect = spriteRenderer.sprite.rect.width / spriteRenderer.sprite.rect.height;
        float maskAspect = shapeSprite.rect.width / shapeSprite.rect.height;
        float halfWidth = size * 0.5f;
        float halfHeight = halfWidth * baseAspect / maskAspect;
        Vector2 localPosition = new Vector2(
            (position.x - center.x) / halfWidth,
            (position.y - center.y) / halfHeight
        );
        Vector2 shapeUv = new Vector2(localPosition.x * 0.5f + 0.5f, localPosition.y * 0.5f + 0.5f);

        if (shapeUv.x < 0f || shapeUv.x > 1f || shapeUv.y < 0f || shapeUv.y > 1f)
            return false;

        return readableShapeTexture.GetPixelBilinear(shapeUv.x, shapeUv.y).a > 0f;
    }

    private Texture2D GetReadableShapeTexture(Sprite shapeSprite)
    {
        if (shapeSprite == null)
            return null;

        if (cachedShapeSprite == shapeSprite && cachedReadableShapeTexture != null)
            return cachedReadableShapeTexture;

        if (cachedReadableShapeTexture != null)
            Destroy(cachedReadableShapeTexture);

        cachedReadableShapeTexture = BuildReadableSpriteTexture(shapeSprite);
        cachedShapeSprite = shapeSprite;
        return cachedReadableShapeTexture;
    }

    private Texture2D BuildReadableSpriteTexture(Sprite shapeSprite)
    {
        int width = Mathf.Max(1, Mathf.RoundToInt(shapeSprite.rect.width));
        int height = Mathf.Max(1, Mathf.RoundToInt(shapeSprite.rect.height));
        RenderTexture temporaryRt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        RenderTexture previous = RenderTexture.active;

        RenderTexture.active = temporaryRt;
        GL.Clear(true, true, Color.clear);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, width, 0, height);

        Rect sourceRect = shapeSprite.textureRect;
        Texture sourceTexture = shapeSprite.texture;
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

    private RenderTexture CreateMaskInstance(RenderTexture template, string suffix)
    {
        RenderTextureDescriptor descriptor = template.descriptor;
        descriptor.depthBufferBits = 0;

        RenderTexture instance = new RenderTexture(descriptor)
        {
            name = template.name + suffix,
            filterMode = template.filterMode,
            wrapMode = TextureWrapMode.Clamp
        };

        instance.Create();
        return instance;
    }

    private void ClearMask(RenderTexture target)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = target;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = previous;
    }

    public void PaintFromScreenPosition(Vector2 screenPosition, Camera camera)
    {
        float distance = transform.position.z - camera.transform.position.z;
        Vector3 worldPosition = camera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, distance)
        );

        if (!TryGetSpriteUV(worldPosition, out Vector2 uv))
            return;

        Paint(uv);
    }

    private void ConfigureMaskUV()
    {
        Sprite sprite = spriteRenderer.sprite;

        if (sprite == null || sprite.texture == null)
            return;

        Rect textureRect = sprite.textureRect;
        Vector2 textureSize = new Vector2(sprite.texture.width, sprite.texture.height);
        Vector2 maskScale = new Vector2(
            textureSize.x / textureRect.width,
            textureSize.y / textureRect.height
        );
        Vector2 maskOffset = new Vector2(
            -textureRect.x / textureRect.width,
            -textureRect.y / textureRect.height
        );

        propertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetTexture(cleaningMaskPropertyId, maskInstance);
        propertyBlock.SetVector(
            cleaningMaskSTPropertyId,
            new Vector4(maskScale.x, maskScale.y, maskOffset.x, maskOffset.y)
        );
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    private bool TryGetSpriteUV(Vector3 worldPosition, out Vector2 uv)
    {
        uv = Vector2.zero;
        Sprite sprite = spriteRenderer.sprite;

        if (sprite == null)
            return false;

        Vector2 localPosition = transform.InverseTransformPoint(worldPosition);
        Vector2 pivotPixels = sprite.pivot;
        float ppu = sprite.pixelsPerUnit;
        float width = sprite.rect.width / ppu;
        float height = sprite.rect.height / ppu;
        float xMin = -pivotPixels.x / ppu;
        float yMin = -pivotPixels.y / ppu;
        float xMax = xMin + width;
        float yMax = yMin + height;
        float x = Mathf.InverseLerp(xMin, xMax, localPosition.x);
        float y = Mathf.InverseLerp(yMin, yMax, localPosition.y);

        uv = new Vector2(x, y);
        return x >= 0f && x <= 1f && y >= 0f && y <= 1f;
    }

    private void Paint(Vector2 uv)
    {
        if (workAreaMask != null && workAreaMask.GetPixelBilinear(uv.x, uv.y).a <= 0f)
            return;

        Graphics.Blit(maskInstance, tempMask);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tempMask;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, tempMask.width, 0, tempMask.height);

        float size = tempMask.width * brushSize;
        float x = uv.x * tempMask.width;
        float y = uv.y * tempMask.height;
        Rect brushRect = new Rect(x - size * 0.5f, y - size * 0.5f, size, size);

        Graphics.DrawTexture(
            brushRect,
            brushTexture,
            new Rect(0, 0, 1, 1),
            0,
            0,
            0,
            0,
            Color.white
        );

        GL.PopMatrix();
        RenderTexture.active = previous;
        Graphics.Blit(tempMask, maskInstance);
        RefreshCleaningPercentage();
    }

    private void ConfigureWorkAreaSpriteMask()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        if (workAreaMaskForSpriteMask != null)
        {
            Destroy(workAreaMaskForSpriteMask);
            workAreaMaskForSpriteMask = null;
        }

        if (workAreaSpriteMask == null)
        {
            GameObject maskObject = new GameObject("WorkAreaMask");
            maskObject.transform.SetParent(transform, false);
            workAreaSpriteMask = maskObject.AddComponent<SpriteMask>();
            workAreaSpriteMask.frontSortingOrder = spriteRenderer.sortingOrder + 1;
            workAreaSpriteMask.backSortingOrder = spriteRenderer.sortingOrder - 1;
            spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
        else if (workAreaSpriteMask.sprite != null)
        {
            Destroy(workAreaSpriteMask.sprite);
        }

        Sprite sourceSprite = spriteRenderer.sprite;
        float worldWidth = sourceSprite.rect.width / sourceSprite.pixelsPerUnit;
        float baseAspect = sourceSprite.rect.width / sourceSprite.rect.height;
        int targetWidth = workAreaMask.width;
        int targetHeight = Mathf.Max(1, Mathf.RoundToInt(targetWidth / baseAspect));
        workAreaMaskForSpriteMask = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        Color32[] displayPixels = new Color32[targetWidth * targetHeight];

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                float u = (x + 0.5f) / targetWidth;
                float v = (y + 0.5f) / targetHeight;
                displayPixels[y * targetWidth + x] = workAreaMask.GetPixelBilinear(u, v);
            }
        }

        workAreaMaskForSpriteMask.SetPixels32(displayPixels);
        workAreaMaskForSpriteMask.Apply(false, false);

        float maskPixelsPerUnit = workAreaMaskForSpriteMask.width / worldWidth;
        Vector2 pivot = new Vector2(
            sourceSprite.pivot.x / sourceSprite.rect.width,
            sourceSprite.pivot.y / sourceSprite.rect.height
        );

        workAreaSpriteMask.sprite = Sprite.Create(
            workAreaMaskForSpriteMask,
            new Rect(0, 0, workAreaMaskForSpriteMask.width, workAreaMaskForSpriteMask.height),
            pivot,
            maskPixelsPerUnit
        );
    }
}

public class CleaningController : CleaningObject
{
}
