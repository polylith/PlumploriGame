using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class holds the data for an entity.
/// </summary>
public class EntityData : AbstractData
{
    public string TypeName { get => typeName; }
    public Vector3 Position { get => position; }
    public Quaternion Rotation { get => rotation; }
    public string Prefix { get => prefix; }

    private string typeName;
    private Vector3 position;
    private Quaternion rotation;
    private string prefix;
    private readonly Dictionary<string, string> attributes;

    public EntityData(string typeName, Vector3 position, Quaternion rotation, string prefix)
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

    public void Load()
    {
        // TODO
    }

    public void Save()
    {
        // TODO
    }
}