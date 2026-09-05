using UnityEngine;
using UnityEngine.SceneManagement;

public static class ShopTimeGate
{
    public static bool IsInShopScene { get; private set; }

    public static void SetInShopScene(bool value)
    {
        IsInShopScene = value;
    }
}

public class ReloadShopOnDayEnd : MonoBehaviour
{
    [SerializeField] private string shopSceneName = "Tienda";

    private bool subscribed;
    private bool timeSubscribed;
    private bool oneHourRemainingTriggered;

    private void OnEnable()
    {
        ShopTimeGate.SetInShopScene(true);
        TrySubscribe();
        CheckOneHourRemaining();
    }

    private void OnDisable()
    {
        ShopTimeGate.SetInShopScene(false);

        if (subscribed && DayManager.Instance != null)
            DayManager.Instance.OnDayStarted -= HandleDayStarted;

        if (TimeManager.Instance != null)
            TimeManager.Instance.OnTimeChanged -= HandleTimeChanged;

        subscribed = false;
        timeSubscribed = false;
    }

    private void Start()
    {
        TrySubscribe();
        CheckOneHourRemaining();
    }

    private void TrySubscribe()
    {
        if (!subscribed && DayManager.Instance != null)
        {
            DayManager.Instance.OnDayStarted += HandleDayStarted;
            subscribed = true;
        }

        if (!timeSubscribed && TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged += HandleTimeChanged;
            timeSubscribed = true;
        }
    }

    private void HandleTimeChanged(int hour, int minute)
    {
        CheckOneHourRemaining();
    }

    private void CheckOneHourRemaining()
    {
        if (oneHourRemainingTriggered || TimeManager.Instance == null ||
            !TimeManager.Instance.IsOneHourRemaining)
        {
            return;
        }

        oneHourRemainingTriggered = true;
        OnOneHourRemaining();
    }

    private void OnOneHourRemaining()
    {

           if(AudioManager.instance != null && FMODEvents.instance != null && !FMODEvents.instance.ClockTicking.IsNull)
           {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.ClockTicking, transform.position);
           }

    }

    private void HandleDayStarted(int day, int rent)
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (!string.IsNullOrEmpty(shopSceneName) &&
            activeScene.name != shopSceneName)
        {
            return;
        }

        SceneManager.LoadScene(activeScene.buildIndex);
    }
}