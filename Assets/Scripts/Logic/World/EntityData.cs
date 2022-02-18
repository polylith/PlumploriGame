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

    public EntityData(string typeName, Vector3 position, Quaternion rotation, string prefix)
    {
        this.typeName = typeName;
        this.position = position;
        this.rotation = rotation;
        this.prefix = prefix;
    }

    public void Load()
    {

    }

    public void Save()
    {

    }
}