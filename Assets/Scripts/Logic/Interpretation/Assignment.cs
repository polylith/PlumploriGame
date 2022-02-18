using System.Collections;
using System.Collections.Generic;

public class Assignment<T> where T : LogicValue
{
    private static int idCount;

    private Dictionary<string, T> assignment;
    private int id;

    public Assignment()
    {
        id = ++idCount;
        assignment = new Dictionary<string, T>();
    }

    public Assignment<T> Clone()
    {
        Assignment<T> ass = new Assignment<T>();

        foreach (string key in assignment.Keys)
        {
            if (!ass.assignment.ContainsKey(key))
                ass.assignment.Add(key, assignment[key]);
            else
                ass.assignment[key] = assignment[key];
        }

        return ass;
    }

    public void Replace(string name, T lv)
    {
        Remove(name);
        Set(name, lv);
    }

    public int CompareTo(Assignment<T> ass)
    {
        if (null == ass)
            return int.MinValue;

        int count = 0;

        foreach (string name in assignment.Keys)
        {
            if (ass.assignment.ContainsKey(name))
            {
                T lv = ass.assignment[name];

                if (assignment[name].Equals(lv))
                    count++;
            }
        }

        return count > 0 ? count : int.MinValue;
    }

    public bool IsEmpty()
    {
        return assignment.Count == 0;
    }

    public void Remove (string name)
    {
        if (null == name || !assignment.ContainsKey(name))
            return;

        assignment.Remove(name);
    }

    public Assignment<T> Set(string name, T lv, bool check = false)
    {
        if (null == name)
            return this;

        if (assignment.ContainsKey(name))
        {
            T lv2 = assignment[name];

            if (check)
            {
                if (lv2.Equals(lv))
                    return this;

                Assignment<T> ass = new Assignment<T>();

                if (null != lv)
                    ass.assignment.Add(name, lv);

                foreach (string name2 in assignment.Keys)
                {
                    if (!name2.Equals(name))
                        ass.assignment.Add(name2, assignment[name2]);
                }

                return ass;
            }

            assignment.Remove(name);
        }

        if (null != lv)
            assignment.Add(name, lv);

        return this;
    }

    public bool Contains(string name)
    {
        if (null == name)
            return false;

        return assignment.ContainsKey(name);
    }

    public List<string> GetNames()
    {
        List<string> keys = new List<string>();

        foreach (string name in assignment.Keys)
            keys.Add(name);

        return keys;
    }

    public T Get(string name)
    {
        if (Contains(name))
            return assignment[name];

        return null;
    }

    public Formula ToFormula()
    {
        List<Formula> listCon = new List<Formula>();

        foreach (string name in assignment.Keys)
        {
            Formula f = WorldDB.Get(name);
            T lv = assignment[name];

            if (null != f && f is Atom)
            {
                if (lv.IsDesignated())
                    listCon.Add(f);
                else
                    listCon.Add(new Negation(f));
            }
        }

        if (listCon.Count > 1)
            return new Conjunction(listCon);
        else if (listCon.Count > 0)
            return listCon[0];

        return null;
    }

    public override string ToString()
    {
        string s = "(" + id + ") < ";

        if (assignment.Count > 0)
        {
            foreach (string name in assignment.Keys)
                s += name + " = " + assignment[name].ToString() + " ";
        }
        else
            s += "EMPTY ";

        s += ">";
        return s;
    }

    public override bool Equals(object o)
    {
        if (null == o || !(o is Assignment<T>))
            return false;

        Assignment<T> ass = (Assignment<T>)o;
        return ass.id == id;
    }

    public override int GetHashCode() => new { id }.GetHashCode();
}