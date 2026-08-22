using System.Collections.Generic;
using UnityEngine;


public class DraggableManager : MonoBehaviour
{
    public static DraggableManager Instance { get; private set; }


    [SerializeField] private List<ItemDraggable> itemDraggables = new();

    [SerializeField] private BoxCollider2D areaWorkSpace;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
        UpdateDraggables();
    }


    private void Update()
    {
        UpdateDraggables();
    }


    private void UpdateDraggables()
    {
        if (InventoryData.Instance == null)
            return;


        for (int i = 0; i < itemDraggables.Count; i++)
        {
            if (i < InventoryData.Instance.Items.Count)
            {
                itemDraggables[i].SetItem(
                    InventoryData.Instance.Items[i]
                );
            }
            else
            {
                itemDraggables[i].SetItem(null);
            }
        }
    }


    public bool IsCompletelyInsideWorkspace(ItemDraggable itemDraggable)
    {
        if (areaWorkSpace == null)
            return false;


        Collider2D itemCollider = itemDraggable.GetComponent<Collider2D>();


        if (itemCollider == null)
            return false;


        Bounds workspaceBounds = areaWorkSpace.bounds;
        Bounds itemBounds = itemCollider.bounds;


        return workspaceBounds.Contains(itemBounds.min) &&
               workspaceBounds.Contains(itemBounds.max);
    }


    private void OnDrawGizmos()
    {
        if (areaWorkSpace == null)
            return;


        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            areaWorkSpace.bounds.center,
            areaWorkSpace.bounds.size
        );
    }
}