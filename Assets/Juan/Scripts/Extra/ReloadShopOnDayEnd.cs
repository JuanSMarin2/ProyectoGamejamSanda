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

    private void OnEnable()
    {
        ShopTimeGate.SetInShopScene(true);
        TrySubscribe();
    }

    private void OnDisable()
    {
        ShopTimeGate.SetInShopScene(false);

        if (subscribed && DayManager.Instance != null)
            DayManager.Instance.OnDayStarted -= HandleDayStarted;

        subscribed = false;
    }

    private void Start()
    {
        TrySubscribe();
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
        Scene activeScene = SceneManager.GetActiveScene();

        if (!string.IsNullOrEmpty(shopSceneName) &&
            activeScene.name != shopSceneName)
        {
            return;
        }

        SceneManager.LoadScene(activeScene.buildIndex);
    }
}