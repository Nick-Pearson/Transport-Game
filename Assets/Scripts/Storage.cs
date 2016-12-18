using UnityEngine;
using System.Collections;

public enum ItemType
{
    None,

    Stone,
    Wood,
    Iron,
    Coal,

    Paper,
    Cars
}

[System.Serializable]
public struct Item
{
    public ItemType Type;
    public int Count;
}

public class Storage : MonoBehaviour {
    // maximum number of items this storage area can hold
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

        DebugStorage();
    }
    
    public void DebugStorage()
    {
        string DebugMessage = "[";

        for (int i = 0; i < MaxStorageSize; i++)
        {
            DebugMessage += Items[i].Type + "(" + Items[i].Count + "),";
        }

        DebugMessage += "]";

        Debug.Log(DebugMessage);
    }

    public void AddItem(Item NewItem)
    {
        //check if we already have some of this item
        int CurrentItemIndex = GetItem(NewItem.Type);

        if(CurrentItemIndex == -1)
        {
            //create a new item
            int FreeSpace = FindFreeItemSlot();

            if(FreeSpace != -1)
            {
                Items[FreeSpace] = NewItem;
            }

            NumberOfFreeItems--;
        }
        else
        {
            Items[CurrentItemIndex].Count += NewItem.Count;
        }

        DebugStorage();
    }

    public int GetItem(ItemType Type)
    {
        for(int i = 0; i < MaxStorageSize; i++)
        {
            if(Items[i].Type == Type)
            {
                return i;
            }
        }
        
        return -1;
    }

    public void RemoveItem(Item Item)
    {
        for (int i = 0; i < MaxStorageSize; i++)
        {
            if(Items[i].Type == Item.Type)
            {
                Items[i].Count -= Item.Count;

                if(Items[i].Count <= 0)
                {
                    //remove from the list
                    int LastFreeItem = FindFreeItemSlot();

                    if(LastFreeItem == -1)
                    {
                        LastFreeItem = MaxStorageSize;
                    }

                    LastFreeItem--;

                    if (i != LastFreeItem)
                    {
                        Items[i] = Items[LastFreeItem];
                    }
                    else
                    {
                        Items[i].Type = ItemType.None;
                        Items[i].Count = 0;
                    }

                    NumberOfFreeItems++;
                }

                break;
            }
        }

        DebugStorage();
    }

    private int FindFreeItemSlot()
    {
        if(NumberOfFreeItems > 0)
        {
            return MaxStorageSize - NumberOfFreeItems;
        }

        return -1;
    }

    public int GetItemCount(ItemType Type)
    {
        int ItemIndex = GetItem(Type);

        if (ItemIndex == -1)
            return 0;

        return Items[ItemIndex].Count;
    }

    public Item[] GetItems()
    {
        Item[] ReturnList = new Item[MaxStorageSize - NumberOfFreeItems];

        for(int i = 0; i < ReturnList.Length; i++)
        {
            ReturnList[i] = Items[i];
        }

        return ReturnList;
    }
}
