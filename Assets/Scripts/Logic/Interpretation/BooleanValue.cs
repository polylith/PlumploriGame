using System.Collections;
using System.Collections.Generic;

public class BooleanValue : LogicValue
{
    public static readonly BooleanValue FALSE = new BooleanValue("f", "false", false);
    public static readonly BooleanValue TRUE = new BooleanValue("t", "true", true);

    public static BooleanValue ResolveValue(bool b)
    {
        if (b)
            return TRUE;

        return FALSE;
    }

    private BooleanValue(string symbol, string name, bool isDesignated) 
        : base(symbol,name,isDesignated)
    {
    }

    public static BooleanValue CastBoolean(LogicValue lv)
    {
        if (null == lv)
            return null;

        if (lv.IsDesignated())
            return TRUE;

        return FALSE;
    }

    public override LogicValue Conjunct(LogicValue lv)
    {
        BooleanValue bv = CastBoolean(lv);

        if (null == lv)
            return null;

        return ( this.IsDesignated() && bv.IsDesignated() ? TRUE : FALSE );
    }

    public override LogicValue Disjunct(LogicValue lv)
    {
        BooleanValue bv = CastBoolean(lv);

        if (null == lv)
            return null;

        return (this.IsDesignated() || bv.IsDesignated() ? TRUE : FALSE);
    }

    public override LogicValue Negate()
    {
        return (this.IsDesignated() ? FALSE : TRUE);
    }

    public override bool Equals(object o)
    {
        if (null == o || !(o is BooleanValue))
            return false;

        BooleanValue bv = (BooleanValue)o;
        return bv.IsDesignated() == IsDesignated();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}