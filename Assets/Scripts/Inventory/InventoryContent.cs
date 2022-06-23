using Creation;
using System.Collections.Generic;

/// <summary>
/// This class is the logical representation of a player's inventory.
/// </summary>
public class InventoryContent
{
    public static int Limit = 15;
    public int Count { get => count; }

    private List<Collectable> list;
    private int count;

    public InventoryContent()
    {
        list = new List<Collectable>();

        for (int i = 0; i < Limit; i++)
            list.Add(null);
    }

    public List<Collectable> GetItems()
    {
        return this.list;
    }

    public bool HasCapacity()
    {
        return count < Limit;
    }

    public bool Contains(Collectable collectable)
    {
        return list.Contains(collectable);
    }

    public bool Add(Collectable collectable)
    {
        if (collectable.IsCollected || Contains(collectable) || count == Limit)
            return false;

        for (int i = 0; i < Limit; i++)
        {
            if (null == list[i])
            {
                list[i] = collectable;
                break;
            }
        }

        collectable.SetInventoryContent(this);
        count++;
        return true;
    }

    public bool Remove(Collectable collectable)
    {
        if (!Contains(collectable))
            return false;

        for (int i = 0; i < Limit; i++)
        {
            if (collectable == list[i])
            {
                list[i] = null;
                break;
            }
        }

        collectable.SetInventoryContent(null);
        count--;
        return true;
    }

    public Key GetKeyFor(Lockable lockable)
    {
        for (int i = 0; i < Limit; i++)
        {
            if (null != list[i] && list[i] is Key)
            {
                Key key = (Key)list[i];

                if (key.CheckAccess(lockable))
                    return key;
            }
        }

        return null;
    }
}