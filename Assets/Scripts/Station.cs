using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Storage))]
public class Station : MonoBehaviour {
    private Storage mStorage;

    private List<Storage> mWatchList;

    private List<Route> mConnectedRoutes;

    void Awake()
    {
        mStorage = GetComponent<Storage>();

        mWatchList = new List<Storage>();
        mConnectedRoutes = new List<Route>();
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

    public void AddRoute(Route NewRoute)
    {
        mConnectedRoutes.Add(NewRoute);

        UpdateStorageList();
    }

    private void UpdateStorageList()
    {
        for(int i = 0; i < mWatchList.Count; i++)
        {
            Item[] Items = mWatchList[i].GetItems();

            for(int j = 0; j < mConnectedRoutes.Count; j++)
            {
                if(mConnectedRoutes[j].CanTransportItem(ref Items[i]))
                {
                    // do some magic, move the item to the station
                }
            }
        }
    }
}
