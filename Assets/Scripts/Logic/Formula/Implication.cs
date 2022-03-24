public class Implication : Formula
{
    private string pId;
    private string qId;

    public Implication(Formula P, Formula Q) : this(null, null, P, Q)
    {
    }

    public Implication(string name, Entity entity, Formula P, Formula Q) : base(name,entity)
    {
        SetAntecedent(P);
        SetConsequent(Q);
    }

    public bool HasAntecedent()
    {
        return null != pId;
    }

    public void SetAntecedent(Formula f)
    {
        string id = (null != f ? f.GetName() : null);
        SetAntecedent(id);
    }

    public void SetAntecedent(string id)
    {
        pId = id;
    }

    public Formula GetAntecedent()
    {
        return WorldDB.Get(pId);
    }

    public bool HasConsequent()
    {
        return null != qId;
    }

    public void SetConsequent(Formula f)
    {
        string id = (null != f ? f.GetName() : null);
        SetConsequent(id);
    }

    public void SetConsequent(string id)
    {
        qId = id;
    }

    public Formula GetConsequent()
    {
        return WorldDB.Get(qId);
    }

    public override Formula Simplify()
    {
        Formula P = WorldDB.Get(pId);
        Formula Q = WorldDB.Get(qId);

        if (null == P)
        {
            if (null == Q)
                return null;

            return Q.Simplify();
        }

        Negation neg = new Negation(P);
        Formula f1 = neg.Simplify();

        if (null == Q)
            return f1;

        Formula f2 = Q.Simplify();

        if (null != f1 && null != f2)
        {
            Disjunction dis = new Disjunction(GetName(), GetEntity());
            dis.AddFormula(f1);
            dis.AddFormula(f2);
            return dis.Simplify();
        }
        else if (null == f1 && null != f2)
            return f2;
        else if (null != f1)
            return f1;

        return null;
    }

    public override bool Equals(object o)
    {
        if (null == o || !(o is Implication))
            return false;

        Implication imp = (Implication)o;
        Formula P = WorldDB.Get(pId);
        Formula Q = WorldDB.Get(qId);
        Formula impP = WorldDB.Get(imp.pId);
        Formula impQ = WorldDB.Get(imp.qId);
        return (null == P && null == impP || P.Equals(impP)) && (null == Q && null == impQ || Q.Equals(impQ));
    }

    public override int GetHashCode()
    {
        string name = GetName();
        return new { name, pId, qId }.GetHashCode();
    }

    public override string ToString()
    {
        Formula P = WorldDB.Get(pId);
        Formula Q = WorldDB.Get(qId);
        string s = (null != P ? P.ToString() : "{ }") + " => " + (null != Q ? Q.ToString() : "{ }");
        return s;
    }

    public override bool Contains(string formulaId)
    {
        if (!HasAntecedent())
            return false;

        if (pId.Equals(formulaId))
            return true;

        return GetAntecedent().Contains(formulaId);
    }
}