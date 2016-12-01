using UnityEngine;
using System.Collections;

public enum ItemType
{
    Stone,
    Wood,
    Iron,
    Coal,

    Paper,
    Cars,

    None
}

[System.Serializable]
public struct Item
{
    public ItemType Type;
    public int Count;
}

public class Storage : MonoBehaviour {
    // maximum number of items this sotrage area can hold
    public int MaxStorageSize;

    //raw item data
    private Item[] Items;

    private int NumberOfFreeItems;

	// Use this for initialization
	void Start ()
    {
        //check data is valid
	    if(MaxStorageSize <= 0)
        {
            Debug.LogWarning("Storage Size must be >= 0");
            MaxStorageSize = 1;
        }

        Items = new Item[MaxStorageSize];
        NumberOfFreeItems = MaxStorageSize;
	}

    public void AddItem(Item NewItem)
    {
        Debug.Log("Got Item " + NewItem.Type);

        //check if we already have some of this item
        Item CurrentItem = GetItem(NewItem.Type);

        if(CurrentItem.Type == ItemType.None)
        {
            //create a new item
            int FreeSpace = FindFreeItemSlot();

            if(FreeSpace != -1)
            {
                Items[FreeSpace] = NewItem;
            }
        }
        else
        {
            CurrentItem.Count += NewItem.Count;
        }
    }

    public Item GetItem(ItemType Type)
    {
        for(int i = 0; i < MaxStorageSize; i++)
        {
            if(Items[i].Type == Type)
            {
                return Items[i];
            }
        }

        Item NullItem = new Item();
        NullItem.Type = ItemType.None;

        return NullItem;
    }

    public void RemoveItem(Item Item)
    {

    }

    private int FindFreeItemSlot()
    {
        for(int i = 0; i < MaxStorageSize; i++)
        {
            if(Items[i].Type == ItemType.None)
            {
                return i;
            }
        }

        return -1;
    }
}
