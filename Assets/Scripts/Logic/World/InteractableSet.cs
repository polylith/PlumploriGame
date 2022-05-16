using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableSet : EntitySet
{
    private static InteractableSet ins;

    public static InteractableSet GetInstance()
    {
        return ins;
    }

    public override int Count { get => dict.Count; }
    public override bool IsReady { get => isReady; }

    public Interactable[] interactablePrefabs;
    private bool isReady;

    private readonly Dictionary<string, Interactable> dict = new Dictionary<string, Interactable>();


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

        foreach (Interactable interactablePrefab in interactablePrefabs)
        {
            string key = interactablePrefab.GetType().ToString();

            if (!dict.ContainsKey(key))
            {
                dict.Add(key, interactablePrefab);
            }
        }

        isReady = true;
    }

    public override Entity Instantiate(EntityData entityData)
    {
        Interactable interactable = null;
        string key = entityData.TypeName;

        if (dict.ContainsKey(key))
        {
            interactable = GameObject.Instantiate(dict[key]);
            interactable.transform.name = entityData.Prefix;
            interactable.SetPrefix(entityData.Prefix);
            interactable.EntityData = entityData;
            interactable.transform.SetPositionAndRotation(
                entityData.Position,
                entityData.Rotation
            );
        }

        return interactable;
    }
}
