using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [System.Serializable]
    private class CursorProfile
    {
        public Texture2D texture;
        public Vector2 hotspot;
        public CursorMode mode = CursorMode.ForceSoftware;
    }

    [Header("Placement")]
    [SerializeField] private CursorProfile placementCursor;

    [Header("Cleaning")]
    [SerializeField] private CursorProfile cleaningCursor;

    [Header("Cutting")]
    [SerializeField] private CursorProfile cuttingCursor;

    [Header("Welding")]
    [SerializeField] private CursorProfile weldingCursor;

    [Header("Painting")]
    [SerializeField] private CursorProfile paintingCursor;

    [Header("Completed")]
    [SerializeField] private CursorProfile completedCursor;

    private void OnDisable()
    {
        SetNormalCursor();
    }

    public void SetPhaseCursor(GamePhase phase)
    {
        CursorProfile profile = GetProfile(phase);

        if (profile == null || profile.texture == null)
        {
            SetNormalCursor();
            return;
        }

        Cursor.SetCursor(profile.texture, profile.hotspot, profile.mode);
    }

    public void SetNormalCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private CursorProfile GetProfile(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Placement:
                return placementCursor;
            case GamePhase.Cleaning:
                return cleaningCursor;
            case GamePhase.Cutting:
                return cuttingCursor;
            case GamePhase.Welding:
                return weldingCursor;
            case GamePhase.Painting:
                return paintingCursor;
            case GamePhase.Completed:
                return completedCursor;
            default:
                return null;
        }
    }
}