using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeldingManager : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private bool discoverSelectedObjects = true;
    [SerializeField] private ObjectData[] weldableObjects;

    [Header("Assembly")]
    [SerializeField] private Transform assemblyParent;

    public event Action OnWeldingCompleted;

    public bool IsWeldingCompleted { get; private set; }

    private Camera mainCamera;

    private ObjectData currentObject;
    private Vector3 currentObjectOffset;

    private readonly List<ObjectData> selectedObjects =
        new List<ObjectData>();

    private bool isDragging;

    private void OnEnable()
    {
        mainCamera = Camera.main;

        IsWeldingCompleted = false;
        currentObject = null;
        isDragging = false;

        DiscoverObjects();
    }

    private void Update()
    {
        if (IsWeldingCompleted)
            return;

        if (Mouse.current == null || mainCamera == null)
            return;

        HandleMouseInput();
    }

    private void DiscoverObjects()
    {
        selectedObjects.Clear();

        if (discoverSelectedObjects)
        {
            weldableObjects =
                FindObjectsByType<ObjectData>(FindObjectsSortMode.None);
        }

        if (weldableObjects == null)
            return;

        foreach (ObjectData objectData in weldableObjects)
        {
            if (objectData == null)
                continue;

            if (!objectData.IsSelected)
                continue;

            selectedObjects.Add(objectData);
        }

        Debug.Log(
            $"[WELDING] Objetos encontrados: {selectedObjects.Count}"
        );
    }

    private void HandleMouseInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartDragging();
        }

        if (isDragging &&
            Mouse.current.leftButton.isPressed)
        {
            DragCurrentObject();
        }

        if (isDragging &&
            Mouse.current.leftButton.wasReleasedThisFrame)
        {
            StopDragging();
        }
    }



    private void StartDragging()
    {
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    mousePosition.x,
                    mousePosition.y,
                    Mathf.Abs(
                        mainCamera.transform.position.z
                    )
                )
            );

        worldPosition.z = 0f;

        Collider2D hit =
            Physics2D.OverlapPoint(worldPosition);

        if (hit == null)
            return;

        ObjectData objectData =
            hit.GetComponentInParent<ObjectData>();

        if (objectData == null)
            return;

        if (!objectData.IsSelected)
            return;

        if (!selectedObjects.Contains(objectData))
            return;

        currentObject = objectData;

        currentObjectOffset =
            currentObject.transform.position -
            worldPosition;

        isDragging = true;
    }

    private void DragCurrentObject()
    {
        if (currentObject == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    mousePosition.x,
                    mousePosition.y,
                    Mathf.Abs(
                        mainCamera.transform.position.z
                    )
                )
            );

        worldPosition.z =
            currentObject.transform.position.z;

        currentObject.transform.position =
            worldPosition + currentObjectOffset;
    }
    private void StopDragging()
    {
        currentObject = null;
        isDragging = false;
    }

    public void ConfirmWelding()
    {
        if (IsWeldingCompleted)
            return;

        if (selectedObjects.Count == 0)
        {
            Debug.LogWarning(
                "[WELDING] No hay objetos para soldar."
            );

            return;
        }

        if (isDragging)
        {
            StopDragging();
        }

        CreateWeldedAssembly();

        IsWeldingCompleted = true;

        Debug.Log("[WELDING] Objetos soldados.");

        OnWeldingCompleted?.Invoke();
    }

    private void CreateWeldedAssembly()
{
    GameObject assembly;

    if (assemblyParent != null)
    {
        assembly = assemblyParent.gameObject;
    }
    else
    {
        assembly = new GameObject("WeldedAssembly");
    }

    WeldedAssembly weldedAssembly =
        assembly.GetComponent<WeldedAssembly>();

    if (weldedAssembly == null)
    {
        weldedAssembly =
            assembly.AddComponent<WeldedAssembly>();
    }

    foreach (ObjectData objectData in selectedObjects)
    {
        if (objectData == null)
            continue;

        objectData.transform.SetParent(
            assembly.transform,
            true
        );
    }

    weldedAssembly.CompleteWelding();

    Debug.Log(
        $"[WELDING] {selectedObjects.Count} piezas convertidas en ensamblaje."
    );
}
}