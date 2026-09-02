using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class VolumeSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private enum VolumeType {
        MASTER,
        MUSIC,
        SFX,
        Ambience
    }

    [Header("Type")]
    [SerializeField] private VolumeType volumeType;

    private Slider volumeSlider;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI percentText;
    private bool valueChangedDuringDrag = false;

    private void Awake()
    {
        volumeSlider = this.GetComponentInChildren<Slider>();
    }

    private void Update()
    {
        switch (volumeType)
        {
            case VolumeType.MASTER:
                volumeSlider.value = AudioManager.instance.masterVolume;
                break;
            case VolumeType.MUSIC:
                volumeSlider.value = AudioManager.instance.musicVolume;
                break;
            case VolumeType.SFX:
                volumeSlider.value = AudioManager.instance.SFXVolume;
                break;
                case VolumeType.Ambience:
                volumeSlider.value = AudioManager.instance.ambienceVolume;
                break;
            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
                break;
        }
        UpdatePercentText();
    }

    public void OnSliderValueChanged()
    {
        switch (volumeType)
        {
            case VolumeType.MASTER:
                AudioManager.instance.masterVolume = volumeSlider.value;
                break;
            case VolumeType.MUSIC:
                AudioManager.instance.musicVolume = volumeSlider.value;
                break;
            case VolumeType.SFX:
                AudioManager.instance.SFXVolume = volumeSlider.value;
                break;
            case VolumeType.Ambience:
            AudioManager.instance.ambienceVolume = volumeSlider.value;
                break;
            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
                break;
        }
        UpdatePercentText();
        valueChangedDuringDrag = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        valueChangedDuringDrag = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (volumeType == VolumeType.SFX && valueChangedDuringDrag)
        {
            if (FMODEvents.instance != null && !FMODEvents.instance.reactivarAudio.IsNull)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.reactivarAudio, transform.position);
            }
        }
        valueChangedDuringDrag = false;
    }

    private void UpdatePercentText()
    {
        if (percentText == null || volumeSlider == null) return;
        int percent = Mathf.RoundToInt(volumeSlider.value * 100f);
        percentText.text = percent + "%";
    }
}