using System.Collections.Generic;
using UnityEngine;

public class CharacterSet : MonoBehaviour
{
    private static CharacterSet ins;

    public static CharacterSet GetInstance()
    {
        return ins;
    }

    public int Count { get => characters.Length; }
    public bool IsReady { get => isReady; }

    public Player[] characters;

    private readonly Dictionary<string, Player> characterMap = new Dictionary<string, Player>();
    private bool isReady;

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

    /// <summary>
    /// Gets a specific character/player by unique name.
    /// </summary>
    /// <param name="name">the unique name of the character</param>
    /// <returns>the character/player</returns>
    public Player GetPlayer(string name)
    {
        if (!characterMap.ContainsKey(name))
            return null;

        return characterMap[name];
    }

    /// <summary>
    /// Initialize all character.
    /// </summary>
    public void InitCharacters()
    {
        if (IsReady)
            return;

        foreach (Player playerPrefab in characters)
        {
            Player player = GameObject.Instantiate<Player>(playerPrefab);
            string name = playerPrefab.GetName();
            player.SetName(name);
            player.transform.name = name;
            characterMap.Add(name, player);
        }

        isReady = true;
    }
}
