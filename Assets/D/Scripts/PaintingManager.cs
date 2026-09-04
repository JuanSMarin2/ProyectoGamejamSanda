using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaintingManager : MonoBehaviour
{
    [Header("Palette - 6 Colors")]
    [SerializeField]
    private Color[] palette = new Color[6];

    public event Action OnPaintingCompleted;

    public bool IsPaintingCompleted { get; private set; }

    private Camera mainCamera;

    private PaintableObject[] paintableObjects;

    private Color selectedColor = Color.white;

    private bool paintingEnabled;

    private bool objectsReady;

    private PaintableObject currentObject;

    private void OnEnable()
    {
        mainCamera = Camera.main;

        IsPaintingCompleted = false;
        paintingEnabled = true;
        objectsReady = false;
        currentObject = null;

        if (palette != null && palette.Length > 0)
        {
            selectedColor = palette[0];
        }

        TryPrepareObjects();

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

        if (!objectsReady)
            TryPrepareObjects();

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

    private void TryPrepareObjects()
    {
        CraftingInventoryManager craftingInventory = FindFirstObjectByType<CraftingInventoryManager>();

        if (craftingInventory != null && craftingInventory.SpawnedPieces.Count == 0)
            return;

        PopulatePaintableObjects(craftingInventory);

        if (paintableObjects == null || paintableObjects.Length == 0)
            return;

        objectsReady = true;

        foreach (PaintableObject paintable in paintableObjects)
        {
            if (paintable != null)
            {
                paintable.SetInteractionEnabled(true);
            }
        }
    }

    private void PopulatePaintableObjects(CraftingInventoryManager craftingInventory)
    {
        List<PaintableObject> found = new List<PaintableObject>();

        if (craftingInventory != null)
        {
            foreach (GameObject piece in craftingInventory.SpawnedPieces)
            {
                if (piece == null)
                    continue;

                PaintableObject paintable = piece.GetComponentInChildren<PaintableObject>();

                if (paintable != null)
                    found.Add(paintable);
            }
        }
        else
        {
            found.AddRange(FindObjectsByType<PaintableObject>(FindObjectsSortMode.None));
        }

        paintableObjects = found.ToArray();

        Debug.Log(
            $"[PAINTING] Objetos encontrados: " +
            $"{paintableObjects.Length}"
        );
    }

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

        if (currentObject != null)
        {
            currentObject.SetPaintColor(
                selectedColor
            );

            if (currentObject.FillWithPaint())
            {
                PlayApplyColorSound(currentObject.transform.position);
            }
        }
    }

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

        selectedColor =
            palette[index];

        Debug.Log(
            $"[PAINTING] Color seleccionado: " +
            $"{selectedColor}"
        );

        PlaySelectColorSound();
    }

    public void ConfirmPainting()
    {
        if (IsPaintingCompleted)
            return;

        IsPaintingCompleted = true;
        paintingEnabled = false;
        currentObject = null;

        if (paintableObjects != null)
        {
            foreach (PaintableObject paintable in paintableObjects)
            {
                if (paintable != null)
                {
                    paintable.SetInteractionEnabled(false);
                }
            }
        }

        Debug.Log(
            "[PAINTING] Pintura completada."
        );

        OnPaintingCompleted?.Invoke();
    }

    private void PlaySelectColorSound()
    {
        if (AudioManager.instance == null ||
            FMODEvents.instance == null ||
            FMODEvents.instance.SeleccionarColor.IsNull)
        {
            return;
        }

        AudioManager.instance.PlayOneShot(
            FMODEvents.instance.SeleccionarColor,
            transform.position
        );
    }

    private void PlayApplyColorSound(Vector3 worldPosition)
    {
        if (AudioManager.instance == null ||
            FMODEvents.instance == null ||
            FMODEvents.instance.AplicarColor.IsNull)
        {
            return;
        }

        AudioManager.instance.PlayOneShot(
            FMODEvents.instance.AplicarColor,
            worldPosition
        );
    }
}