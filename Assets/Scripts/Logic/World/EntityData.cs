using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class holds the data for an entity.
/// </summary>
public class EntityData : AbstractData
{
    /// <summary>
    /// Type name is always GetType().ToString() of the entity
    /// </summary>
    public string TypeName { get => typeName; }

    /// <summary>
    /// Initial position of the entity
    /// </summary>
    public Vector3 Position { get => position; }

    /// <summary>
    /// Initial rotation of the entity
    /// </summary>
    public Quaternion Rotation { get => rotation; }

    /// <summary>
    /// Prefix used in logic formulas
    /// </summary>
    public string Prefix { get => prefix; }

    private string typeName;
    private Vector3 position;
    private Quaternion rotation;
    private string prefix;
    private readonly Dictionary<string, string> attributes;

    public EntityData(string typeName, Vector3 position,
        Quaternion rotation, string prefix)
    {
        this.typeName = typeName;
        this.position = position;
        this.rotation = rotation;
        this.prefix = prefix;
        attributes = new Dictionary<string, string>();
    }

    public bool HasAttribute(string key)
    {
        return !string.IsNullOrEmpty(key) && attributes.ContainsKey(key);
    }

    public void SetAttribute(string key, string value = null)
    {
        if (HasAttribute(key))
            attributes.Remove(key);

        Debug.Log(key + " = " + value);

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            return;

        attributes.Add(key, value);
    }

    public string GetAttribute(string key)
    {
        string value = "";

        if (HasAttribute(key))
            value = attributes[key];

        return value;
    }

    /// <summary>
    /// Removes all entries in attributes that start with
    /// a given prefix
    /// </summary>
    /// <param name="prefix">prefix to be removed</param>
    public void Clear(string prefix)
    {
        List<string> list = new List<string>();

        foreach (string key in attributes.Keys)
        {
            if (key.StartsWith(prefix))
                list.Add(key);
        }

        foreach (string key in list)
        {
            attributes.Remove(key);
        }
    }

    public void Load()
    {
        // TODO
    }

    public void Save()
    {
        // TODO
    }
}