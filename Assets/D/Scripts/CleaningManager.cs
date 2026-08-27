using System;
using System.Collections.Generic;
using UnityEngine;

public class CleaningManager : MonoBehaviour
{
    public bool IsPhaseCompleted { get; private set; }

    public event Action PhaseCompleted;

    private CleaningObject[] phaseObjects;

    private void OnEnable()
    {
        IsPhaseCompleted = false;

        TryPopulatePhaseObjects();
        CheckPhaseCompletion();
    }

    private void Update()
    {
        if (IsPhaseCompleted)
            return;

        if (phaseObjects == null || phaseObjects.Length == 0)
            TryPopulatePhaseObjects();

        CheckPhaseCompletion();
    }

    private void TryPopulatePhaseObjects()
    {
        CraftingInventoryManager craftingInventory = FindFirstObjectByType<CraftingInventoryManager>();

        List<CleaningObject> found = new List<CleaningObject>();

        if (craftingInventory != null)
        {
            if (craftingInventory.SpawnedPieces.Count == 0)
                return;

            foreach (GameObject piece in craftingInventory.SpawnedPieces)
            {
                if (piece == null)
                    continue;

                CleaningObject cleaningObject = piece.GetComponentInChildren<CleaningObject>();

                if (cleaningObject != null)
                    found.Add(cleaningObject);
            }
        }
        else
        {
            found.AddRange(FindObjectsByType<CleaningObject>(FindObjectsSortMode.None));
        }

        phaseObjects = found.ToArray();

        Debug.Log($"[CLEANING] Objetos a limpiar: {phaseObjects.Length}");
    }

    public void CheckPhaseCompletion()
    {
        if (phaseObjects == null || phaseObjects.Length == 0)
            return;

        foreach (CleaningObject cleaningObject in phaseObjects)
        {
            if (cleaningObject == null)
                continue;

            if (!cleaningObject.IsFullyCleaned)
                return;
        }

        IsPhaseCompleted = true;
        PhaseCompleted?.Invoke();
    }
}