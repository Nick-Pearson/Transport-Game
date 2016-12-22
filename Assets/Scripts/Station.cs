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

        AddRoute(new Route());
    }

    public void RegisterStorageItem(Storage NewStorage)
    {
        if (!mWatchList.Contains(NewStorage))
        {
            mWatchList.Add(NewStorage);
            NewStorage.OnStorageChangedEvent.AddListener(UpdateStorageList);
        }
    }

    public void UnregisterStorageItem(Storage NewStorage)
    {
        mWatchList.Remove(NewStorage);
        NewStorage.OnStorageChangedEvent.RemoveListener(UpdateStorageList);
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
            List<Item> Items = mWatchList[i].GetItems();
            
            for (int k = 0; k < Items.Count; k++)
            {
                for (int j = 0; j < mConnectedRoutes.Count; j++)
                {
                    if (mConnectedRoutes[j].CanTransportItem(Items[k]))
                    {
                        // do some magic, move the item to the station
                        mStorage.AddItem(Items[k]);
                        mWatchList[i].RemoveItem(Items[k]);
                    }
                }
            }
        }
    }
}
