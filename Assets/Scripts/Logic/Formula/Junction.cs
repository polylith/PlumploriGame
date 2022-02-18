using System.Collections.Generic;

public abstract class Junction : Formula
{
    protected List<string> listIds;

    protected Junction(string name, Entity entity) : base(name, entity)
    {
        listIds = new List<string>();
    }

    protected Junction(string name, Entity entity, List<Formula> list) : this(name, entity)
    {
        if (null != list)
        {
            foreach (Formula f in list)
                AddFormula(f);
        }
    }

    public void AddFormula(Formula f)
    {
        if (null == f)
            return;

        string name = f.GetName();

        if (null == name || name.Equals(this.name))
            return;

        if (!listIds.Contains(name))
        {
            if (this is Conjunction && f is Conjunction)
            {
                Conjunction con = (Conjunction)f;

                foreach (Formula f_con in con.GetList())
                    AddFormula(f_con);
            }
            else if (this is Disjunction && f is Disjunction)
            {
                Disjunction dis = (Disjunction)f;

                foreach (Formula f_dis in dis.GetList())
                    AddFormula(f_dis);
            }
            else
                listIds.Add(name);
        }
    }

    public List<Formula> GetList()
    {
        List<Formula> list = new List<Formula>();

        foreach (string name in listIds)
        {
            Formula f = WorldDB.Get(name);

            if (null != f)
                list.Add(f);
        }

        return list;
    }

    public override string ToString()
    {
        string s = "";
        string op = (this is Conjunction ? "&" : "|");
        int i = 0;
        List<Formula> list = GetList();

        if (null == list || list.Count == 0)
            return "{ }";

        Formula f = list[i];
        s += "(" + f.ToString();
        i++;

        while (i < list.Count)
        {
            f = list[i];
            s += " " + op + " " + f.ToString();
            i++;
        }

        s += ")";
        return s;
    }
}