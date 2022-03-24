using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNF : Formula
{
    private List<Atom> atoms;
    private int index;
    private Formula f;
    private bool generated;

    public DNF(string name, Entity entity, List<Atom> atoms, int index) : base(name, entity)
    {
        this.atoms = new List<Atom>();
        this.atoms.AddRange(atoms);
        this.index = index;
        generated = false;
    }

    public Formula GetFormula()
    {
        if (!generated)
            Simplify();

        return f;
    }

    public override Formula Simplify()
    {
        if (generated && null != f)
            return f;

        int n = atoms.Count;
        int index0 = this.index;
        int i = 0;
        Disjunction dis = new Disjunction(GetName(), GetEntity());

        while (index0 > 0)
        {
            if (index0 % 2 == 1)
            {
                int index1 = i;
                int j = 0;
                Conjunction con = new Conjunction("", null);
                dis.AddFormula(con);

                while (j < n)
                {
                    if (index1 % 2 == 1)
                        con.AddFormula(atoms[j]);
                    else
                        con.AddFormula(new Negation("", null, atoms[j]));

                    index1 >>= 1;
                    j++;
                }
            }

            index0 >>= 1;
            i++;
        }

        f = dis.Simplify();
        generated = true;
        return f;
    }

    public override bool Equals(object o)
    {
        if (null == o || !(o is DNF))
            return false;

        DNF dnf = (DNF)o;

        if (dnf.atoms.Count != atoms.Count || dnf.index != index)
            return false;

        for (int i = 0; i < atoms.Count; i++)
        {
            if (!dnf.atoms.Contains(atoms[i]))
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        string name = GetName();
        return new { name, atoms }.GetHashCode();
    }

    public override string ToString()
    {
        string s = "DNF [ " + atoms[0].ToString();

        for (int i = 1; i < atoms.Count; i++)
            s += ", " + atoms[i].ToString();

        s += " ] (" + index + ")";
        return s;
    }

    public override bool Contains(string formulaId)
    {
        foreach (Atom atom in atoms)
        {
            if (atom.Contains(formulaId))
                return true;
        }

        return false;
    }
}