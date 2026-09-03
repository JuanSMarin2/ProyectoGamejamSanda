using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSetupPerDay : MonoBehaviour
{
    [Serializable]
    private class SpecificDayGroup
    {
        public int day;
        public List<GameObject> objects = new();
    }

    [Serializable]
    private class FromDayGroup
    {
        public int fromDay;
        public List<GameObject> objects = new();
    }

    [Header("Objetos que solo existen un dia especifico")]
    [SerializeField] private List<SpecificDayGroup> specificDayGroups = new();

    [Header("Objetos que existen desde cierto dia en adelante")]
    [SerializeField] private List<FromDayGroup> fromDayGroups = new();

    private bool subscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (subscribed && DayManager.Instance != null)
            DayManager.Instance.OnDayStarted -= HandleDayStarted;

        subscribed = false;
    }

    private void Start()
    {
        TrySubscribe();
        ApplySetup();
    }

    private void TrySubscribe()
    {
        if (subscribed || DayManager.Instance == null)
            return;

        DayManager.Instance.OnDayStarted += HandleDayStarted;
        subscribed = true;
    }

    private void HandleDayStarted(int day, int rent)
    {
        ApplySetup();
    }

    public void ApplySetup()
    {
        int currentDay =
            DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;

        foreach (SpecificDayGroup group in specificDayGroups)
        {
            if (group == null)
                continue;

            SetGroupActive(group.objects, currentDay == group.day);
        }

        foreach (FromDayGroup group in fromDayGroups)
        {
            if (group == null)
                continue;

            SetGroupActive(group.objects, currentDay >= group.fromDay);
        }
    }

    private void SetGroupActive(List<GameObject> objects, bool active)
    {
        if (objects == null)
            return;

        foreach (GameObject target in objects)
        {
            if (target != null)
                target.SetActive(active);
        }
    }
}