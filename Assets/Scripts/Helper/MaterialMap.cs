using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class maps the materials of a GameObject.
/// </summary>
public class MaterialMap
{
    private static Dictionary<string, int> matCount = new Dictionary<string, int>();

    private static string MapName(string s)
    {
        string name = NormalizeName(s);

        if (!matCount.ContainsKey(name))
            matCount.Add(name, 0);

        int count = matCount[name];
        matCount[name] = count + 1;
        name += count.ToString();
        return name;
    }

    private ArrayList searchList;
    private Dictionary<string, int> map;

    public int Count { get => map.Count; }

    public MaterialMap()
    {
        searchList = new ArrayList();
        map = new Dictionary<string, int>();
    }

    public void Init(Transform parent)
    {
        Renderer renderer = parent.GetComponent<Renderer>();
        AddMaterials(renderer);

        foreach (Transform child in parent)
            Init(child);
    }

    public Material[] RemapMaterials(Transform parent)
    {
        Material[] materials = ToArray();
        Material[] newmats = new Material[materials.Length];

        for (int i = 0; i < newmats.Length; i++)
        {
            newmats[i] = Material.Instantiate<Material>(materials[i]) as Material;
            newmats[i].name = MapName(materials[i].name);
        }

        Remap(parent, newmats);
        return newmats;
    }

    private void Remap(Transform parent, Material[] newmats)
    {
        Renderer renderer = parent.GetComponent<Renderer>();
        
        if (null != renderer)
        {
            Material[] mats = new Material[renderer.materials.Length];

            for (int i = 0; i < renderer.materials.Length; i++)
            {
                string searchName = NormalizeName(renderer.materials[i].name);
                int index = ContainsMaterial(searchName);
                Material mat = renderer.materials[i];

                if (index >= 0)
                    mat = newmats[index];

                mats[i] = mat;
            }

            renderer.materials = mats;
        }

        foreach (Transform child in parent)
            Remap(child, newmats);
    }

    public void ShowList()
    {
        string s = "Materials: " + Count;
        int i = 0;

        foreach (string searchName in map.Keys)
        {
            s += "\n\t" + i + " " + searchName;
            i++;
        }

        Debug.Log(s);
    }

    public string[] GetKeys()
    {
        string[] keys = new string[map.Keys.Count];
        int i = 0;

        foreach (string key in map.Keys)
        {
            keys[i] = key;
            i++;
        }

        return keys;
    }

    public Material[] ToArray()
    {
        Material[] materialsArray = searchList.ToArray(typeof(Material)) as Material[];
        return materialsArray;
    }

    public Color GetMaterialColor(int index)
    {
        return ((Material)searchList[index]).color;
    }

    public void ApplyMaterialColor(int index, Color color)
    {
        ((Material)searchList[index]).color = color;
    }

    public void AddMaterials(Renderer renderer)
    {
        if (null == renderer)
            return;

        foreach (Material mat in renderer.sharedMaterials)
            CheckAddMaterial(mat);
    }

    public void AddMaterials(Material[] materials)
    {
        if (null == materials || materials.Length == 0)
            return;

        foreach (Material mat in materials)
            CheckAddMaterial(mat);
    }

    public int CheckAddMaterial(Material mat, bool bAdd = true)
    {
        string searchName = mat.name;
        searchName = NormalizeName(searchName);
        int index = ContainsMaterial(searchName);

        if (index >= 0 || !bAdd)
            return index;
        
        searchList.Add(Material.Instantiate<Material>(mat));
        index = searchList.Count - 1;
        map.Add(searchName, index);
        return index;
    }

    public int ContainsMaterial(string searchName)
    {
        if (!map.ContainsKey(searchName))
            return -1;

        return map[searchName];
    }

    private static string NormalizeName(string name)
    {
        string s = name.Replace(" ", "");
        s = s.Replace("(Instance)", "");
        s = s.Replace("(Clone)", "");
        return s;
    }
}