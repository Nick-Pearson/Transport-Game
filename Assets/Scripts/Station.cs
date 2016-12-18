using UnityEngine;
using System.Collections;

public struct ItemRequest
{
    Item Item;
    Storage Storage;
}


[RequireComponent(typeof(Storage))]
public class Station : MonoBehaviour {
    private Storage mStorage;

    private ArrayList mWatchList;

    private ArrayList mItemRequests;

    void Awake()
    {
        mStorage = GetComponent<Storage>();
        mWatchList = new ArrayList();
    }

    public void RegisterStorageItem(Storage NewStorage)
    {
        if (!mWatchList.Contains(NewStorage))
        {
            mWatchList.Add(NewStorage);
        }
    }

    public void UnregisterStorageItem(Storage NewStorage)
    {
        mWatchList.Remove(NewStorage);
    }

    public void RequestItem(ItemRequest Request)
    {
        mItemRequests.Add(Request);
    }
}
