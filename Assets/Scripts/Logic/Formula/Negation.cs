using System.Collections.Generic;

public class Negation : Formula
{
    private string fId;

    public Negation(Formula f) : this(null, null, f)
    {
    }

    public Negation(string name, Entity entity, Formula f) : base(name, entity)
    {
        string id = (null != f ? f.GetName() : null);
        SetFormula(id);
    }

    public void SetFormula(string id)
    {
        fId = id;
    }

    public Formula GetFormula()
    {
        return WorldDB.Get(fId);
    }

    public override Formula Simplify()
    {
        Formula f = GetFormula();

        if (null == f)
            return null;

        if (f is Atom)
            return this;

        Formula newF = f.Simplify();

        if (null != newF)
        {
            if (newF is Negation) // --F = F
            {
                Negation neg = (Negation)newF;
                newF = neg.GetFormula();
            }
            else if (newF is Junction) // -(F1 & F2) = -F1 | -F2 // -(F1 | F2) = -F1 & -F2
            {
                Junction junc = (Junction)newF;
                List<Formula> list = junc.GetList();
                List<Formula> newlist = new List<Formula>();

                foreach (Formula f1 in list)
                {
                    Negation neg = new Negation(f1);
                    Formula f2 = neg.Simplify();

                    if (null != f2)
                        newlist.Add(f2);
                }
                
                if (newlist.Count == 0)
                    newF = null;
                else if (newlist.Count == 1)
                    newF = newlist[0];
                else
                {
                    if (newF is Conjunction)
                        newF = new Disjunction(GetName(), GetEntity(), newlist);
                    else
                        newF = new Conjunction(GetName(), GetEntity(), newlist);
                }
            }
            else
                newF = new Negation(GetName(), GetEntity(), newF);
        }        

        return newF;
    }

    public override string ToString()
    {
        Formula f = WorldDB.Get(fId);
        string s = (null != f ? "-" + f.ToString() : "{ }");
        return s;
    }

    public override bool Equals(object o)
    {
        if (null == o || !(o is Negation))
            return false;

        Negation neg = (Negation)o;

        if (this == neg)
            return true;

        Formula f = GetFormula();
        Formula negF = neg.GetFormula();
        return null == f && null == negF || f.Equals(negF);
    }

    public override int GetHashCode()
    {
        string name = GetName();
        return new { name, fId }.GetHashCode();
    }
}