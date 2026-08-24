using System.Collections;
using UnityEngine;


public class CraftingManager : MonoBehaviour
{
    [SerializeField] private Camera artworkCamera;
    [SerializeField] private RenderTexture artworkRenderTexture;
    [SerializeField] private float artworkOffset = 500f;




    public void FinishArtwork()
    {
        StartCoroutine(CreateArtwork());
    }


    private IEnumerator CreateArtwork()
    {
        GameObject[] placedItems = GameObject.FindGameObjectsWithTag("ItemDraggablePlaced");

        Vector3[] originalPositions = new Vector3[placedItems.Length];

        for (int i = 0; i < placedItems.Length; i++)
        {
            originalPositions[i] = placedItems[i].transform.position;

            placedItems[i].transform.position += new Vector3(artworkOffset, 0f, 0f);
        }


        yield return null;


        Sprite artworkSprite = CaptureArtwork();


        for (int i = 0; i < placedItems.Length; i++)
        {
            placedItems[i].transform.position = originalPositions[i];
        }


        if (artworkSprite == null)
            yield break;


        ArtworkData artwork = new ArtworkData();

        artwork.artworkSprite = artworkSprite;

        if (CurrentArtworkData.Instance != null)
        {
            artwork.rust = CurrentArtworkData.Instance.Artwork.rust;
            artwork.shine = CurrentArtworkData.Instance.Artwork.shine;
            artwork.weight = CurrentArtworkData.Instance.Artwork.weight;
            artwork.baseValue = CurrentArtworkData.Instance.Artwork.baseValue;
        }


        if (ArtworkDisplayData.Instance != null)
        {
            bool added = ArtworkDisplayData.Instance.AddArtwork(artwork);

            if (added)
            {
                CurrentArtworkData.Instance.ClearArtwork();

                if (InventoryData.Instance != null)
                    InventoryData.Instance.ClearInventory();
            }
        }
    }


    private Sprite CaptureArtwork()
    {
        if (artworkCamera == null || artworkRenderTexture == null)
            return null;


        RenderTexture currentTexture = RenderTexture.active;

        RenderTexture.active = artworkRenderTexture;

        artworkCamera.targetTexture = artworkRenderTexture;

        artworkCamera.Render();


        Texture2D texture = new Texture2D(
            artworkRenderTexture.width,
            artworkRenderTexture.height,
            TextureFormat.RGBA32,
            false
        );


        texture.ReadPixels(
            new Rect(
                0,
                0,
                artworkRenderTexture.width,
                artworkRenderTexture.height
            ),
            0,
            0
        );


        texture.Apply();


        artworkCamera.targetTexture = null;
        RenderTexture.active = currentTexture;


        return Sprite.Create(
            texture,
            new Rect(
                0,
                0,
                texture.width,
                texture.height
            ),
            new Vector2(0.5f, 0.5f)
        );
    }
}