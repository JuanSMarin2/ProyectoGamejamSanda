using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaintingManager : MonoBehaviour
{
    [Header("Palette - 6 Colors")]
    [SerializeField]
    private Color[] palette = new Color[6];

    [Header("Paintable Objects")]
    [SerializeField]
    private bool discoverPaintableObjects = true;

    [SerializeField]
    private PaintableObject[] paintableObjects;

    public event Action OnPaintingCompleted;

    public bool IsPaintingCompleted { get; private set; }

    private Camera mainCamera;

    // Este es SOLO el color actualmente seleccionado
    // en la paleta.
    private Color selectedColor = Color.white;

    private bool paintingEnabled;

    private PaintableObject currentObject;

    private void OnEnable()
    {
        mainCamera = Camera.main;

        IsPaintingCompleted = false;
        paintingEnabled = true;
        currentObject = null;

        DiscoverObjects();

        // Seleccionar automáticamente el primer color
        // de la paleta al comenzar la fase.
        if (palette != null && palette.Length > 0)
        {
            selectedColor = palette[0];
        }

        // IMPORTANTE:
        // Aquí NO asignamos el color a los objetos.
        //
        // Cada objeto conservará su propio color.
        foreach (PaintableObject paintable in paintableObjects)
        {
            if (paintable != null)
            {
                paintable.SetInteractionEnabled(true);
            }
        }

        Debug.Log(
            $"[PAINTING] Fase activada. " +
            $"Color inicial: {selectedColor}"
        );
    }

    private void OnDisable()
    {
        paintingEnabled = false;
        currentObject = null;

        if (paintableObjects == null)
            return;

        foreach (PaintableObject paintable in paintableObjects)
        {
            if (paintable != null)
            {
                paintable.SetInteractionEnabled(false);
            }
        }
    }

    private void Update()
    {
        if (!paintingEnabled || IsPaintingCompleted)
            return;

        if (Mouse.current == null)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            BeginPainting();
        }
    }

    // =========================================================
    // BUSCAR OBJETO CORRECTO
    // =========================================================

    private void BeginPainting()
    {
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            mainCamera.ScreenPointToRay(
                mousePosition
            );

        Plane plane =
            new Plane(
                Vector3.forward,
                Vector3.zero
            );

        if (!plane.Raycast(
            ray,
            out float enter))
        {
            currentObject = null;
            return;
        }

        Vector3 world =
            ray.GetPoint(enter);

        Collider2D[] hits =
            Physics2D.OverlapPointAll(
                new Vector2(
                    world.x,
                    world.y
                )
            );

        if (hits == null || hits.Length == 0)
        {
            currentObject = null;
            return;
        }

        PaintableObject bestObject = null;

        int bestSortingOrder =
            int.MinValue;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            PaintableObject paintable =
                hit.GetComponentInParent<PaintableObject>();

            if (paintable == null)
                continue;

            SpriteRenderer renderer =
                paintable.GetComponent<SpriteRenderer>();

            if (renderer == null)
                continue;

            int sortingOrder =
                renderer.sortingOrder;

            if (
                bestObject == null ||
                sortingOrder > bestSortingOrder
            )
            {
                bestObject = paintable;
                bestSortingOrder = sortingOrder;
            }
        }

        currentObject = bestObject;

        // =====================================================
        // PINTAR OBJETO
        // =====================================================

        if (currentObject != null)
        {
            // Primero asignamos al objeto el color que
            // está actualmente seleccionado.
            currentObject.SetPaintColor(
                selectedColor
            );

            // Después lo pintamos completamente.
            currentObject.FillWithPaint();
        }
    }

    // =========================================================
    // DESCUBRIR OBJETOS
    // =========================================================

    private void DiscoverObjects()
    {
        if (!discoverPaintableObjects)
            return;

        paintableObjects =
            FindObjectsByType<PaintableObject>(
                FindObjectsSortMode.None
            );

        Debug.Log(
            $"[PAINTING] Objetos encontrados: " +
            $"{paintableObjects.Length}"
        );
    }

    // =========================================================
    // COLORES
    // =========================================================

    public void SelectColor0()
    {
        SelectColor(0);
    }

    public void SelectColor1()
    {
        SelectColor(1);
    }

    public void SelectColor2()
    {
        SelectColor(2);
    }

    public void SelectColor3()
    {
        SelectColor(3);
    }

    public void SelectColor4()
    {
        SelectColor(4);
    }

    public void SelectColor5()
    {
        SelectColor(5);
    }

    public void SelectColor(int index)
    {
        if (
            palette == null ||
            index < 0 ||
            index >= palette.Length
        )
        {
            return;
        }

        // =====================================================
        // IMPORTANTE
        // =====================================================
        //
        // Solo cambiamos el color que se utilizará
        // para FUTURAS pinturas.
        //
        // NO modificamos ningún PaintableObject que
        // ya haya sido pintado.
        // =====================================================

        selectedColor =
            palette[index];

        Debug.Log(
            $"[PAINTING] Color seleccionado: " +
            $"{selectedColor}"
        );
    }

    // =========================================================
    // TERMINAR
    // =========================================================

    public void ConfirmPainting()
    {
        if (IsPaintingCompleted)
            return;

        IsPaintingCompleted = true;
        paintingEnabled = false;
        currentObject = null;

        foreach (PaintableObject paintable in paintableObjects)
        {
            if (paintable != null)
            {
                paintable.SetInteractionEnabled(false);
            }
        }

        Debug.Log(
            "[PAINTING] Pintura completada."
        );

        OnPaintingCompleted?.Invoke();
    }
}