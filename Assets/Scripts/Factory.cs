using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Storage))]
public class Factory : MonoBehaviour {
    public FactoryData FactoryType;

    private Storage mStorage;

    void Awake()
    {
        mStorage = GetComponent<Storage>();
    }

    void Update()
    {
        for(int i = 0; i < FactoryType.Productions.Length; i++)
        {
            if(CheckRequirements(ref FactoryType.Productions[i]))
            {
                ProduceItem(ref FactoryType.Productions[i]);
            }
        }
    }

    bool CheckRequirements(ref ProductionData Production)
    {
        if(Time.time < Production.LastUpdateTime + Production.RequiredTime)
            return false;

        for(int i = 0; i < Production.RequiredItems.Length; i++)
        {
            Item mItem = mStorage.GetItem(Production.RequiredItems[i].Type);

            if (mItem.Count < Production.RequiredItems[i].Count)
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
