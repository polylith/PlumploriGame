using UnityEngine;

[System.Serializable]
public class DropdownOption
{
    [SerializeField]
    public Sprite icon;
    [SerializeField]
    public Language.LangKey langKey = Language.LangKey.Option;
    [SerializeField]
    public bool isEnabled = true;
    [HideInInspector]
    public int index;
}
