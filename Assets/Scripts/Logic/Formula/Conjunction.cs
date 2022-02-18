﻿using System.Collections.Generic;

public class Conjunction : Junction
{
    public Conjunction() : this(null, null)
    {
    }

    public Conjunction(List<Formula> list) : this(null, null, list)
    {
    }

    public Conjunction(string name, Entity entity) : base(name, entity)
    {
    }

    public Conjunction(string name, Entity entity, List<Formula> list) : base(name, entity, list)
    {
    }

    public override Formula Simplify()
    {
        List<Formula> newlist = new List<Formula>();
        List<Formula> list = GetList();

        foreach (Formula f1 in list)
        {
            Formula f2 = f1.Simplify();

            if (null != f2)
            {
                if (f2 is Conjunction)
                {
                    Conjunction con = (Conjunction)f2;
                    List<Formula> listCon = con.GetList();

                    foreach (Formula f in listCon)
                        newlist.Add(f);
                }
                else
                    newlist.Add(f2);
            }
        }

        if (newlist.Count > 1)
        {
            List<int> indexList = new List<int>();

            for (int i = 0; i < newlist.Count; i++)
            {
                if (!indexList.Contains(i))
                {
                    Formula f_i = newlist[i];

                    for (int j = 0; j < newlist.Count; j++)
                    {
                        if (!indexList.Contains(j))
                        {
                            if (i != j)
                            {
                                Formula f_j = newlist[j];

                                if (f_i is Negation)
                                {
                                    Negation neg_i = (Negation)f_i;
                                    Formula F = neg_i.GetFormula();

                                    if (F.Equals(f_j)) // -f & f = F
                                        return null;
                                }
                                else
                                {
                                    if (f_i.Equals(f_j)) // f & f = f
                                    {
                                        if (!indexList.Contains(j))
                                            indexList.Add(j);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (indexList.Count > 0)
            {
                if (indexList.Count > 1)
                {
                    indexList.Sort(delegate (int x1, int x2)
                    {
                        return x1 < x2 ? -1 : 1;
                    });
                }

                while (indexList.Count > 0)
                {
                    int n = indexList.Count - 1;
                    int index = indexList[n];
                    newlist.RemoveAt(index);
                    indexList.RemoveAt(n);
                }
            }
        }

        if (newlist.Count == 0)
            return null;

        if (newlist.Count == 1)
            return newlist[0];

        return new Conjunction(GetName(), GetEntity(), newlist);
    }

    public override bool Equals(object o)
    {
        if (null == o || !(o is Conjunction))
            return false;

        Conjunction con = (Conjunction)o;
        
        if (this == con)
            return true;

        List<Formula> list = GetList();
        List<Formula> list2 = con.GetList();
        int count = list.Count;

        if (list2.Count != count)
            return false;

        foreach (Formula f1 in list)
        {
            foreach (Formula f2 in list2)
            {
                if (f1.Equals(f2))
                    count--;
            }
        }

        return count <= 0;
    }

    public override int GetHashCode() {
        string name = GetName();
        return new { name, listIds }.GetHashCode();
    }
}