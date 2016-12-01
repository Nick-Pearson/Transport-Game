using UnityEngine;
using System.Collections;

[System.Serializable]
public struct ProductionData
{
    // item we want to make
    public Item TargetItem;

    // Items needed to make this item
    public Item[] RequiredItems;

    //How long it will take
    public float RequiredTime;

    //Time we last got items from this production
    [HideInInspector]
    public float LastUpdateTime;
}

[CreateAssetMenu (menuName ="Factory")]
public class FactoryData : ScriptableObject {

    public ProductionData[] Productions;
}
