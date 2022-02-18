using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>
/// Entity is the root abstract class that inherits
/// from the Unity class MonoBehaviour.
/// </para>
/// <para>
/// All methods and functions of the game logic are
/// encapsulated in this class. An entity is actually
/// derived from the predicate logic. However, the
/// logic used is based on propositional logic.
/// Essentially, predicate logic is no more powerful
/// than propositional logic. Instead of using logical
/// formulas with free or bound variables, a specific
/// propositional logical formula is used for each entity
/// that is registered in the world database. 
/// </para>
/// <para>
/// Each Interactable is an entity. There could also
/// be non-interactive GameObjects inheriting from the
/// Entity class, which can be activated by the logic
/// to just perform animations.
/// </para>
/// </summary>
public abstract class Entity : MonoBehaviour
{
    /*
     * In this dictionary we manage the technical names 
     * of entities in a one-to-one way. There must be no
     * entity without a unique name, and no two different
     * entities may have the same name.
     */
    private static Dictionary<string, int> dict = new Dictionary<string, int>();

    /// <summary>
    /// Get a unique id for an entity.
    /// </summary>
    /// <param name="entity">Entity to register</param>
    /// <returns>unique id string</returns>
    private static string GetID(Entity entity)
    {
        string s = entity.transform.name;

        if (!dict.ContainsKey(s))
            dict.Add(s, 0);

        int count = dict[s];
        dict[s] = count + 1;
        s += "(" + count + ")";
        return s;
    }

    /// <summary>
    /// Language key as string
    /// </summary>
    public string LangKey { get => langKey; }

    /// <summary>
    /// Prefix used in atomic formulas of this entity
    /// </summary>
    public string Prefix { get => GetPrefix(); }

    /// <summary>
    /// Dictionary containing atomic formulas used in
    /// the consequent of an inference rule to pass its
    /// truth value to the entity when it changes.
    /// </summary>
    private Dictionary<string, System.Action<bool>> delegates;

    public string langKey;
    protected string prefix;

    /// <summary>
    /// Bounds of the collider
    /// </summary>
    protected Bounds bounds;
    protected bool hasBounds = false;

    /// <summary>
    /// Attributes used in combination with the prefix
    /// to build logic formulas.
    /// </summary>
    /// <returns></returns>
    public abstract List<string> GetAttributes();

    /// <summary>
    /// This Method doesn't need to be called in subclasses.
    /// It just needs to be overridden if needed.
    /// The base method doesn't do anything.
    /// </summary>
    public virtual void Initialize()
    {
        // Nothing to do in default case.
    }

    public void Register()
    {
        InitFormulas();
        RegisterAtoms();
    }

    /// <summary>
    /// Finding the extent of the entity
    /// </summary>
    /// <returns>bounds of all attached colliders</returns>
    public virtual Bounds GetBounds()
    {
        if (!hasBounds)
        {
            bounds = new Bounds();

            Collider[] colliders = transform.GetComponents<Collider>();

            if (null != colliders)
                foreach (Collider c in colliders)
                    if (!c.isTrigger)
                        bounds.Encapsulate(c.bounds);

            colliders = transform.GetComponentsInChildren<Collider>();

            if (null != colliders)
                foreach (Collider c in colliders)
                    if (!c.isTrigger)
                        bounds.Encapsulate(c.bounds);

            hasBounds = true;
        }

        return bounds;
    }

    /// <summary>
    /// Gives the natural language text of the entity
    /// depending on the currently set language.
    /// It returns at runtime the translation of the
    /// specified LangKey.
    /// This function could be overridden in subclasses,
    /// but actually it is not necessary.
    /// </summary>
    /// <returns>language dependent identifier of the entity</returns>
    public virtual string GetText()
    {
        return Language.LanguageManager.GetText(LangKey);
    }

    /// <summary>
    /// Shows a detailed natural language description of
    /// the entity and the states of its attributes.
    /// </summary>
    public void ShowDescription()
    {
        string s = GetDescription();

        if (null == s)
            return;

        // TODO display text
        Debug.Log(s);
    }

    /// <summary>
    /// Provides a detailed natural language description of
    /// the entity and the states of its attributes.
    /// </summary>
    /// <returns>description used for the LookAction</returns>
    public abstract string GetDescription();

    private string GetPrefix()
    {
        if (null == prefix)
            prefix = GetID(this);

        return prefix;
    }

    public void SetPrefix(string prefix)
    {
        if (null != this.prefix || null == prefix)
            return;

        transform.name = prefix;
        this.prefix = prefix;
    }

    private void InitFormulas()
    {
        if (null != delegates && delegates.Count > 0)
            return;

        delegates = new Dictionary<string, System.Action<bool>>();
    }

    protected void RegisterCurrentState(string token, bool state)
    {
        string formulaId = Prefix + token;

        if (!delegates.ContainsKey(formulaId))
            return;
        
        WorldDB.RegisterCurrentState(formulaId, state);
    }

    public bool HasDelegate(string formulaId)
    {
        return null != delegates && delegates.ContainsKey(formulaId);
    }
        
    public System.Action<bool> GetDelegate(string formulaId)
    {
        if (null == delegates || !delegates.ContainsKey(formulaId))
            return null;

        return delegates[formulaId];
    }

    protected void SetDelegate(string token, System.Action<bool> action)
    {
        if (null == delegates)
            return;


        string formulaId = Prefix + token;

        if (null == action)
        {
            if (delegates.ContainsKey(formulaId))
                delegates.Remove(formulaId);

            return;
        }

        if (!delegates.ContainsKey(formulaId))
            delegates.Add(formulaId, action);
        else
            delegates[formulaId] = action;
    }

    public void SetLayer(int layer)
    {
        transform.gameObject.layer = layer;

        foreach (Transform trans in transform)
            trans.gameObject.layer = layer;
    }

    public void Notify(string formulaId, LogicValue lv)
    {
        Debug.Log("Notify " + formulaId + " " + lv);

       if (!delegates.ContainsKey(formulaId))
         return;

        bool value = lv.IsDesignated();

        Debug.Log("\tDelegate " + formulaId + " " + value);

        delegates[formulaId].Invoke(value);
    }

    protected void Fire(string token, bool isDesignated)
    {
        string formulaId = Prefix + token;
        Debug.Log("Fire " + formulaId + " " + isDesignated);
        WorldDB.EvaluateState(formulaId, isDesignated);
    }

    protected void RegisterAtoms(List<string> attributes)
    {
        foreach (string s in attributes)
        {
            string formulaId = Prefix + s;
            WorldDB.RegisterAtom(formulaId, this);
        }
    }

    public abstract void RegisterGoals();

    // TODO: abstract
    public virtual void RegisterCurrentState()
    {

    }

    [System.Obsolete("This function might no longer be usefull.", true)]
    public List<ObjectPlace> GetObjectPlaces()
    {
        List<ObjectPlace> list = new List<ObjectPlace>();
        ObjectPlace[] places = transform.GetComponents<ObjectPlace>();

        if (null != places)
            list.AddRange(places);

        places = transform.GetComponentsInChildren<ObjectPlace>();

        if (null != places)
            list.AddRange(places);

        if (list.Count > 0)
            return list;

        return null;
    }

    protected abstract void RegisterAtoms();
}