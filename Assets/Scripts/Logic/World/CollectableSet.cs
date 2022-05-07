using System.Collections.Generic;
using UnityEngine;

public class CollectableSet : EntitySet
{
    private static CollectableSet ins;

    public static CollectableSet GetInstance()
    {
        return ins;
    }

    public override int Count { get => dict.Count; }

    public override bool IsReady { get => isReady; }

    public Collectable[] collectablePrefabs;
    private bool isReady;

    private readonly Dictionary<string, Collectable> dict = new Dictionary<string, Collectable>();


    private void Awake()
    {
        if (null == ins)
        {
            isReady = false;
            ins = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Init()
    {
        if (IsReady)
            return;

        foreach (Collectable collectablePrefab in collectablePrefabs)
        {
            string key = collectablePrefab.GetType().ToString();

            if (!dict.ContainsKey(key))
            {
                dict.Add(key, collectablePrefab);
            }
        }

        isReady = true;
    }

    public override Entity Instantiate(EntityData entityData)
    {
        Collectable collectable = null;
        string key = entityData.TypeName;

        if (dict.ContainsKey(key))
        {
            collectable = GameObject.Instantiate(dict[key]);
            collectable.transform.name = entityData.Prefix;
            collectable.SetPrefix(entityData.Prefix);

            collectable.transform.SetPositionAndRotation(
                entityData.Position,
                entityData.Rotation
            );
        }

        return collectable;
    }
}
