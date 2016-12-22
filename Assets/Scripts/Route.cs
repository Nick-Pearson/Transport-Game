using System.Collections.Generic;
using UnityEngine;

public class Route {
    //do any of the stations want this item
    public bool CanTransportItem(Item NewItem)
    {
        return true;
    }

    // Get all stations that want this item
    public List<Station> GetStationsForItemType(ItemType Type)
    {
        return new List<Station>();
    }
}
