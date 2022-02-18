using System.Collections;
using System.Collections.Generic;

public class FOURValue : LogicValue
{
    public static readonly FOURValue FALSE = new FOURValue("f", "false", new bool[2] { false, true }, false);
    public static readonly FOURValue TRUE = new FOURValue("t", "true", new bool[2]{true, false}, true);
    public static readonly FOURValue BOT = new FOURValue("⊥", "bot", new bool[2] { false, false }, false);
    public static readonly FOURValue TOP = new FOURValue("⊤", "top", new bool[2] { true, true }, true);

    public static FOURValue ResolveValue(bool x, bool y)
    {
        if (x && y)
            return TOP;

        if (!x && y)
            return TRUE;

        if (x && !y)
            return FALSE;

        return BOT;
    }

    private bool[] value; 

    private FOURValue(string symbol, string name, bool[] value, bool isDesignated)
        : base(symbol, name, isDesignated)
    {
        this.value = value;
    }

    public static FOURValue CastFOUR(LogicValue lv)
    {
        if (null == lv)
            return BOT;

        if (lv.IsDesignated())
            return TRUE;

        return FALSE;
    }

    public override LogicValue Conjunct(LogicValue lv)
    {
        FOURValue fv = CastFOUR(lv);
        bool x1 = value[0];
        bool x2 = fv.value[0];
        bool y1 = value[1];
        bool y2 = fv.value[1];
        bool x = x1 && x2;
        bool y = y1 || y2;
        return ResolveValue(x, y);
    }

    public override LogicValue Disjunct(LogicValue lv)
    {
        FOURValue fv = CastFOUR(lv);
        bool x1 = value[0];
        bool x2 = fv.value[0];
        bool y1 = value[1];
        bool y2 = fv.value[1];
        bool x = x1 || x2;
        bool y = y1 && y2;
        return ResolveValue(x, y);
    }

    public override LogicValue Negate()
    {
        return ResolveValue(value[1], value[0]);
    }

    public override bool Equals(object obj)
    {
        if (null == obj || !(obj is FOURValue))
            return false;

        if (!base.Equals(obj))
            return false;

        FOURValue fv = (FOURValue)obj;
        return fv.value[0] == value[0] && fv.value[1] == value[1];
    }

    public override int GetHashCode()
    {
        int hash = base.GetHashCode();
        hash += new { value }.GetHashCode();
        return hash;
    }
}
