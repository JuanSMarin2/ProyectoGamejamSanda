using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioConfig : MonoBehaviour
{
    [field: Header("Scene Audio Configuration")]
    [field: SerializeField] public EventReference sceneMusic { get; private set; }
    [field: SerializeField] public EventReference sceneAmbience { get; private set; }

    public static AudioConfig instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Audio Config instance in the scene.");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
}
