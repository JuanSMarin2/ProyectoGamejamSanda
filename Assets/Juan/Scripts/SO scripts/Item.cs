using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Junkseum/Item")]
public class Item : ScriptableObject
{
    public int id;
    public string itemName;
    public Sprite sprite;

    [Header("Stats")]
    [Range(0f, 100f)] public float rust;
    [Range(0f, 100f)] public float shine;
    [Range(0f, 100f)] public float weight;

    [Header("Part Stats")]
    [Range(0f, 100f)] public float head;
    [Range(0f, 100f)] public float body;
    [Range(0f, 100f)] public float basePart;
}