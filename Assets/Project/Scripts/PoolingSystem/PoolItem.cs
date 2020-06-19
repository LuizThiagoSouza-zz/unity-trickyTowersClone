using UnityEngine;

public abstract class PoolItem : MonoBehaviour
{
    public Pool myPool;

    public bool IsActive { get; set; }

    public void Despawn()
    {
        if (!IsActive) return;

        myPool.ReturnItem(this);
    }

    public void SetActive(bool isActive)
    {
        if (isActive == IsActive) return;

        IsActive = isActive;
        gameObject.SetActive(isActive);

        if (isActive)
            OnSpawn();
    }

    public abstract void OnSpawn();
    public abstract void OnDespawn();
}
