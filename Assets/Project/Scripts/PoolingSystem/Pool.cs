using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pool : MonoBehaviour
{
    public PoolItem item;
    public int initialItensAmount = 5;

    private Transform myTransform;
    private List<PoolItem> activedItems;
    private List<PoolItem> disabledItems;

    #region <--- MONOBEHAVIOURS --->

    private void Awake()
    {
        if (item == null)
        {
            Debug.Log("Error: PoolItem not found at " + gameObject.name);
            enabled = false;
            return;
        }

        myTransform = GetComponent<Transform>();
        activedItems = new List<PoolItem>();
        disabledItems = new List<PoolItem>();

        item.gameObject.SetActive(false);

        for (int i = 0; i < initialItensAmount; i++)
            InstantiateNewPoolItem();
    }

    #endregion <--- MONOBEHAVIOURS --->

    #region <--- PUBLIC METHODS --->

    public PoolItem GetItem()
    {
        PoolItem itemToReturn;

        itemToReturn = disabledItems.Count > 0 ? disabledItems[0] : InstantiateNewPoolItem();
        if (itemToReturn == null) return null;

        disabledItems.Remove(itemToReturn);
        activedItems.Add(itemToReturn);

        itemToReturn.SetActive(true);

        return itemToReturn;
    }

    public PoolItem GetItem(bool isActive)
    {
        PoolItem itemToReturn;

        itemToReturn = disabledItems.Count > 0 ? disabledItems[0] : InstantiateNewPoolItem();
        if (itemToReturn == null) return null;

        disabledItems.Remove(itemToReturn);
        activedItems.Add(itemToReturn);

        itemToReturn.SetActive(isActive);

        return itemToReturn;
    }

    public void ReturnItem(PoolItem itemToReturn)
    {
        if (itemToReturn == null) return;

        itemToReturn.IsActive = false;
        itemToReturn.OnDespawn();
        itemToReturn.transform.SetParent(myTransform);
        itemToReturn.gameObject.SetActive(false);

        disabledItems.Add(itemToReturn);
    }

    #endregion <--- PUBLIC METHODS --->

    #region <--- PRIVATE METHODS --->

    private PoolItem InstantiateNewPoolItem()
    {
        var newItem = Instantiate(item, myTransform, false);

        if (newItem.gameObject.activeSelf)
            newItem.gameObject.SetActive(false);

        newItem.myPool = this;
        newItem.transform.localPosition = Vector3.zero;
        disabledItems.Add(newItem);

        return newItem;
    }

    #endregion <--- PRIVATE METHODS --->
}
