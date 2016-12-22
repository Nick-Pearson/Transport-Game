using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Storage))]
public class Factory : MonoBehaviour {
    public FactoryData FactoryType;

    public Station LocalStation;

    private Storage mStorage;

    private ProductionData[] Productions;

    void Awake()
    {
        mStorage = GetComponent<Storage>();

        Productions = new ProductionData[FactoryType.Productions.Length];
        for(int i = 0; i < FactoryType.Productions.Length; i++)
        {
            Productions[i] = FactoryType.Productions[i];
        }

        if (LocalStation != null)
        {
            //LocalStation.RegisterStorageItem(mStorage);
            mStorage = LocalStation.GetComponent<Storage>();

        }
    }

    void Update()
    {
        for (int i = 0; i < Productions.Length; i++)
        {
            if (CheckRequirements(ref Productions[i]))
            {
                ProduceItem(ref Productions[i]);
            }
        }
    }

    bool CheckRequirements(ref ProductionData Production)
    {
        if (Time.time < Production.LastUpdateTime + Production.RequiredTime)
            return false;

        for(int i = 0; i < Production.RequiredItems.Length; i++)
        {
            if (mStorage.GetItemCount(Production.RequiredItems[i].Type) < Production.RequiredItems[i].Count)
                return false;
        }
        
        return true;
    }

    void ProduceItem(ref ProductionData Production)
    {
        for (int i = 0; i < Production.RequiredItems.Length; i++)
        {
            mStorage.RemoveItem(Production.RequiredItems[i]);
        }

        mStorage.AddItem(Production.TargetItem);

        Production.LastUpdateTime = Time.time;
    }
}
