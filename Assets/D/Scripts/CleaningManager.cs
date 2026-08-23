using System;
using UnityEngine;

public class CleaningManager : MonoBehaviour
{
    [SerializeField] private bool discoverActiveObjects = true;
    [SerializeField] private CleaningObject[] phaseObjects;

    public bool IsPhaseCompleted { get; private set; }
    public event Action PhaseCompleted;

    private void Start()
    {
        if (discoverActiveObjects)
            phaseObjects = FindObjectsByType<CleaningObject>(FindObjectsSortMode.None);

        CheckPhaseCompletion();
    }

    private void Update()
    {
        if (!IsPhaseCompleted)
            CheckPhaseCompletion();
    }

    public void CheckPhaseCompletion()
    {
        if (phaseObjects == null || phaseObjects.Length == 0)
            return;

        foreach (CleaningObject cleaningObject in phaseObjects)
        {
            if (cleaningObject == null || !cleaningObject.IsFullyCleaned)
            {
                return;
            }
        }

        IsPhaseCompleted = true;
        PhaseCompleted?.Invoke();
    }
}
