using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

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
public class Item
{
    public ItemType Type;
    public int Count;
}

public class Storage : MonoBehaviour {
    //raw item data
    private List<Item> Items;

    public UnityEvent OnStorageChangedEvent;

	// Use this for initialization
	void Start ()
    {
        Items = new List<Item>();

        OnStorageChangedEvent.AddListener(DebugStorage);
    }
    
    public void DebugStorage()
    {
        string DebugMessage = this + "[";

        for (int i = 0; i < Items.Count; i++)
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
            Items.Add(NewItem);
        }
        else
        {
            Items[CurrentItemIndex].Count += NewItem.Count;
        }
        
        OnStorageChangedEvent.Invoke();
    }

    public int GetItem(ItemType Type)
    {
        for(int i = 0; i < Items.Count; i++)
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
        int index = GetItem(Item.Type);

        if (index == -1)
            return;

        if(Items[index].Count >  Item.Count)
        {
            Items[index].Count -= Item.Count;
        }
        else
        {
            Items.RemoveAt(index);
        }

        OnStorageChangedEvent.Invoke();
    }

    public int GetItemCount(ItemType Type)
    {
        int ItemIndex = GetItem(Type);

        if (ItemIndex == -1)
            return 0;

        return Items[ItemIndex].Count;
    }

    public List<Item> GetItems()
    {
        return Items;
    }
}
