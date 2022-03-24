using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>
/// This static class implements the logic and progress of the game.
/// </para>
/// <para>
/// The game logic is essentially based on a propositional logic in
/// combination with inference rules and goals.
/// </para>
/// <para>
/// Each entity has attributes that are registered in combination with
/// their individual prefix as atomic logical formulas.
/// The states of these attributes are reflected in the states of the
/// object and are used with the help of an interpretation to bring the
/// game into different states.
/// The state of the objects is changed either by interaction of the
/// current character or by evaluation of inference rules.
/// When a state specified in the goals is reached, the game has reached
/// one of several possible endings.
/// </para>
/// <para>
/// The interpretation of logical formulas is indeed abstract, and there
/// are different implementations of interpretations. So far, however,
/// the WorldDB has been designed for the extended Boolean logic.
/// </para>
/// </summary>
public static class WorldDB
{
    public static bool HasGoals { get => null != goals && goals.Count > 0; }
    public static bool HasRules { get => null != rules && rules.Count > 0; }
    public static bool HasFormulas{ get => null != formulas && formulas.Count > 0; }
    public static List<Entity> Entities { get => entities; }

    public static Dictionary<string, Formula> formulas;
    public static Dictionary<string, Implication> rules;
    public static Dictionary<string, Assignment<BooleanValue>> goals;
    public static GraphNode<Assignment<BooleanValue>> currentState;

    private static Dictionary<string, Formula> formulaDB;
    private static List<Entity> entities;
    private static Interpretation<BooleanValue> interpretation;
    private static Graph<Assignment<BooleanValue>> states;    

    public static void InitDB()
    {
        Debug.Log("Init DB");
        formulas = new Dictionary<string, Formula>();
        rules = new Dictionary<string, Implication>();
        entities = new List<Entity>();
        formulaDB = new Dictionary<string, Formula>();
        goals = new Dictionary<string, Assignment<BooleanValue>>();
        states = new Graph<Assignment<BooleanValue>>();
        Assignment<BooleanValue> ass = new Assignment<BooleanValue>();
        currentState = states.Get(ass);
        interpretation = new BooleanLogic(ass);
    }
    
    public static void RegisterCurrentState(string formulaId, bool IsDesignated)
    {
        Assignment<BooleanValue> ass = currentState.GetT();
        Formula f = Get(formulaId);

        if (null == f || !(f is Atom))
            return;

        BooleanValue bv = BooleanValue.ResolveValue(IsDesignated);

        if (ass.Contains(formulaId))
            ass.Remove(formulaId);

        ass.Set(formulaId, bv);
    }

    public static void ShowCurrentState()
    {
        if (null == interpretation)
        {
            Debug.LogError("Missing Interpretation");
            return;
        }

        string s = "Current " + currentState.ToString();
        Debug.Log(s);
    }

    public static void EvaluateState(string formulaId, bool isDesignated)
    {
        GraphNode<Assignment<BooleanValue>> tmpState = currentState;
        BooleanValue bv2 = BooleanValue.ResolveValue(isDesignated);

        Assignment<BooleanValue> ass = currentState.GetT();
        // Get the interpretation of this formula from the assignment
        BooleanValue bv1 = ass.Get(formulaId);
        bool isNewState = false;

        // if bv1 is not null, the formula is already known
        if (null != bv1)
        {
            /*
             * if the interpretation has changed, that might
             * be a new state or an already existing state
             */
            if (!bv1.Equals(bv2))
            {
                isNewState = SwitchState(formulaId, bv2);
            }
        }
        else
        {
            /*
             * if bv1 is null, bv2 is a new information that
             * can be just added to the assignment
             */
            ass.Set(formulaId, bv2);
            isNewState = true;
        }

        bool isAntecedent = EvaluateFormulas(formulaId);

        if (isNewState)
        {
            Debug.Log(formulaId + " is antecedent ? " + isAntecedent);

            // TODO visual handling of state change
            if (isAntecedent)
                AudioManager.GetInstance().PlaySound("notify");
        }

        ShowCurrentState();
    }

    private static bool EvaluateFormulas(string formulaId)
    {
        bool isAntecedent = false;
        BooleanValue bv = null;
        Assignment<BooleanValue> ass = currentState.GetT();
        string s = "";
        
        foreach (string id in rules.Keys)
        {
            Implication rule = rules[id];
            interpretation.SetAssignment(ass);

            s += rule.ToString() + "\n";

            Formula f = null;
            Formula P = rule.GetAntecedent();
            Formula Q = rule.GetConsequent();
            BooleanValue bvP = BooleanValue.CastBoolean(interpretation.Evaluate(P));
            BooleanValue bvQ = BooleanValue.CastBoolean(interpretation.Evaluate(Q));
            isAntecedent = rule.Contains(formulaId);

            s += "\tP " + bvP + " => Q " + bvQ + "\n";

            if (null == bvQ && null != bvP && bvP.IsDesignated()) // P => Q
            {
                if (null != Q && Q.HasEntity())
                {
                    f = Q;
                    bvQ = BooleanValue.TRUE;
                }
                else if (Q is Negation negation)
                {
                    f = negation.GetFormula();

                    if (null != f && f.HasEntity())
                        bvQ = BooleanValue.FALSE;
                }
            }
            else if (null != bvQ)
            {
                if (!bvQ.IsDesignated() && null != P && null == bvP) // -Q => -P
                    ass = SetAssignments(P, BooleanValue.FALSE, ass);

                if (null != Q && Q.HasEntity())
                {
                    f = Q;
                }
                else if (Q is Negation negation)
                {
                    f = negation.GetFormula();
                    bvQ = (BooleanValue)bvQ.Negate();
                }
            }

            if (null != f && null != bvQ)
                ass = ass.Set(f.GetName(), bvQ, true);

            bv = BooleanValue.CastBoolean(interpretation.Evaluate(rule));

            if (rule.HasEntity() && rule.GetEntity().HasDelegate(rule.GetName()))
            {
                s += "\t\tBV " + bv + "\n";
                ass = ass.Set(rule.GetName(), bv, true);
            }
        }

        foreach (string id in formulas.Keys)
        {
            Formula f = formulas[id];

            s += f.ToString() + "\n";

            interpretation.SetAssignment(ass);
            bv = BooleanValue.CastBoolean(interpretation.Evaluate(f));

            if (f.HasEntity())
            {
                s += "\tBV " + bv + "\n";
                ass = ass.Set(f.GetName(), bv, true);
            }
        }

        List<string> names = ass.GetNames();

        foreach (string name in names)
            SwitchState(name, ass.Get(name));

        ass = currentState.GetT();
        names = ass.GetNames();

        foreach (string name in names)
        {
            Formula f = Get(name);

            s += "\t" + name + ": " + ass.Get(name) + "\n";// + f.HasEntity() + "\n";

            if (null != f && f.HasEntity())
                f.GetEntity().Notify(name, ass.Get(name));
        }

        Debug.Log(s);

        EvaluateGoals(ass);
        return isAntecedent;
    }

    private static Assignment<BooleanValue> SetAssignments(Formula f, BooleanValue bv, Assignment<BooleanValue> ass)
    {
        if (f is Atom)
        {
            if (f.HasEntity())
            {
                ass = ass.Set(f.GetName(), BooleanValue.FALSE, true);
            }
            else if (f is Negation negation)
            {
                bv = (BooleanValue)bv.Negate();
                f = negation.GetFormula();

                if (f is Atom && f.HasEntity())
                    ass = ass.Set(f.GetName(), bv, true);
                else if (f is Disjunction disjunction)
                {
                    List<Formula> list = disjunction.GetList();

                    foreach (Formula f1 in list)
                        ass = SetAssignments(f1, bv, ass);
                }
            }
        }
        else if (f is Conjunction conjunction)
        {
            List<Formula> list = conjunction.GetList();

            foreach (Formula f1 in list)
                ass = SetAssignments(f1, bv, ass);
        }

        return ass;
    }

    private static void EvaluateGoals(Assignment<BooleanValue> ass)
    {
        if (!HasGoals)
        {
            /* 
             * This never must not be the case!
             * No goals, no end of game
             */
            return;
        }

        bool res = true;
        interpretation.SetAssignment(ass);

        foreach (string prefix in goals.Keys)
        {
            Formula goal = Get(prefix);
            
            if (null == goal)
            {
                Assignment<BooleanValue> goalAss = goals[prefix];
                goal = goalAss.ToFormula();
                goal.SetEntity(prefix, null);
                RegisterFormula(goal);
            }

            BooleanValue bv = BooleanValue.CastBoolean(interpretation.Evaluate(goal));

            Debug.Log("Goal " + goal.ToString() + " -> " + bv);

            if (null == bv || !bv.IsDesignated())
                res = false;
        }

        if (res)
        {
            // TODO real finish -> call to gamemanager
            AudioManager.GetInstance().PlaySound("win");
        }
    }

    private static bool SwitchState(string formulaId, BooleanValue bv)
    {
        Assignment<BooleanValue> ass1 = currentState.GetT();
        List<GraphNode<Assignment<BooleanValue>>> nodes = states.GetNodes();
        GraphNode<Assignment<BooleanValue>> nextState = null;
        int max = int.MinValue;
        bool isNewState = false;

        foreach (GraphNode<Assignment<BooleanValue>> node in nodes)
        {
            if (node != currentState)
            {
                Assignment<BooleanValue> ass2 = node.GetT();
                BooleanValue bv1 = ass2.Get(formulaId);

                if (null == bv1 || bv1.Equals(bv))
                {
                    int value = ass2.CompareTo(ass1);

                    if (value > 0 && value > max)
                    {
                        max = value;
                        nextState = node;
                    }
                }
            }
        }

        if (null == nextState)
        {
            Assignment<BooleanValue> newass = ass1.Clone();
            newass.Replace(formulaId, bv);
            nextState = states.Get(newass);
            isNewState = true;
        }
        else
        {
            Assignment<BooleanValue> ass2 = nextState.GetT();
            BooleanValue bv1 = ass2.Get(formulaId);

            if (null == bv1)
                ass2.Set(formulaId, bv);
        }

        currentState.AddOut(nextState);
        currentState = nextState;
        return isNewState;
    }

    public static void RegisterGoal(string prefix, string token, bool isDesignated)
    {
        if (!goals.ContainsKey(prefix))
            goals.Add(prefix, new Assignment<BooleanValue>());

        string name = prefix + token;
        Assignment<BooleanValue> ass = goals[prefix];
        BooleanValue bv1 = BooleanValue.ResolveValue(isDesignated);
        BooleanValue bv2 = ass.Get(name);

        if (null == bv2)
            ass.Set(name, bv1);
        else if (!bv2.Equals(bv1))
            ass.Remove(name);

        if (ass.IsEmpty())
            goals.Remove(prefix);
    }

    public static void ShowDB()
    {
        Formula f;
        string s = "Rules\n";
        int i = 0;

        foreach (string id in rules.Keys)
        {
            f = rules[id];
            s += i + "\t" + f.GetSymbol() + " | "  + f.GetName() + " (" + f.GetType().Name + ") = " + f.ToString() + "\n";
            i++;
        }

        s += "\nFormulas\n";
        i = 0;

        foreach (string id in formulas.Keys)
        {
            f = formulas[id];
            s += i + "\t" + f.GetSymbol() + " | " + f.GetName() + " (" + f.GetType().Name + ") = " + f.ToString() + "\n";
            i++;
        }

        s += "\nGoals\n";
        i = 0;

        foreach (string prefix in goals.Keys)
        {
            Assignment<BooleanValue> goalAss = goals[prefix];
            Formula goal = goalAss.ToFormula();
            s += i + "\t" + goal.ToString() + "\n";
            i++;
        }

        s += "\n";

        Debug.Log(s);
    }

    public static Formula BuildDNF(List<Atom> atoms)
    {
        int n = atoms.Count;
        int N = n > 1 ? 2 << (n - 1) : 2;
        int M = N > 1 ? 2 << (N - 1) : 2;
        int index = Random.Range(0, M);
        DNF dnf = new DNF(atoms[0].GetEntity().Prefix, null, atoms, index);
        return dnf.GetFormula();
    }

    public static Dictionary<string, List<Atom>> GetAtoms()
    {
        if (null == formulaDB)
            return null;

        Dictionary<string, List<Atom>> dict = new Dictionary<string, List<Atom>>();

        foreach (string key in formulaDB.Keys)
        {
            if (formulaDB[key] is Atom atom)
            {
                Entity entity = atom.GetEntity();

                if (null != entity)
                {
                    string prefix = entity.Prefix;

                    if (!dict.ContainsKey(prefix))
                        dict.Add(prefix, new List<Atom>());

                    dict[prefix].Add(atom);
                }
            }
        }

        return dict;
    }

    public static Formula Get(string id)
    {
        if (null == id)
            return null;

        if (!formulaDB.ContainsKey(id))
            return null;
        
        return formulaDB[id];
    }

    public static void RegisterAtom(string id, Entity entity)
    {
        if (null == id || null == entity)
            return;

        RegisterEntity(entity);

        if (formulaDB.ContainsKey(id))
            return;

        new Atom(id, entity);
    }

    public static void RegisterEntity(Entity entity)
    {
        if (null == entity || entities.Contains(entity))
            return;

        entities.Add(entity);
    }

    public static void RegisterFormula(Formula f)
    {
        if (null == f)
            return;

        string id = f.GetName();
        RemoveFormula(id);
        formulaDB.Add(id, f);

        if (f.HasEntity())
        {
            Entity entity = f.GetEntity();

            if (null != entity)
                RegisterEntity(entity);
        }

        if (f is Implication implication)
        {
            if (!rules.ContainsKey(id))
                rules.Add(id, implication);
        }
        else if (!(f is Atom) && f.HasEntity())
        {
            if (!formulas.ContainsKey(id))
                formulas.Add(id, f);
        }
    }

    public static void RemoveFormula(string id)
    {
        if (null == id || !formulaDB.ContainsKey(id))
            return;

        formulaDB.Remove(id);

        if (rules.ContainsKey(id))
            rules.Remove(id);

        if (formulas.ContainsKey(id))
            formulas.Remove(id);
    }
}