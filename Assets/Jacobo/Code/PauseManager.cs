using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private bool isPaused = false;
    private float previousMasterVolume = 1f;
    private const float volumeReductionFactor = 0.8f; // reduce to 80% (20% reduction)
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    private void Update()
    {
     //   if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
    }

    public void TogglePause()
    {
        AudioManager.instance?.PlayOneShot(FMODEvents.instance.pausa,transform.position);
        if (isPaused) Resume();
        else Pause();
        
    }

    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;
        Time.timeScale = 0f;

        if (AudioManager.instance != null)
        {
            previousMasterVolume = AudioManager.instance.masterVolume;
            AudioManager.instance.masterVolume = previousMasterVolume * volumeReductionFactor;
        }

        if (pausePanel != null) pausePanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;
        Time.timeScale = 1f;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.masterVolume = previousMasterVolume;
        }

        if (pausePanel != null) pausePanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
