using System.Collections;
using System.Collections.Generic;

public abstract class LogicValue
{
    private string symbol;
    private string name;
    private bool isDesignated;

    protected LogicValue(string symbol, string name, bool isDesignated)
    {
        this.symbol = symbol;
        this.name = name;
        this.isDesignated = isDesignated;
    }
        
    public string GetSymbol()
    {
        return symbol;
    }

    public string GetName()
    {
        return name;
    }

    public bool IsDesignated()
    {
        return isDesignated;
    }

    public abstract LogicValue Negate();
    public abstract LogicValue Conjunct(LogicValue lv);
    public abstract LogicValue Disjunct(LogicValue lv);

    public override bool Equals(object obj)
    {
        if (null == obj || !(obj is LogicValue))
            return false;

        LogicValue lv = (LogicValue)obj;
        return null != lv.symbol && lv.symbol.Equals(symbol) && null != lv.name
            && lv.name.Equals(name) && lv.IsDesignated() == IsDesignated();
    }

    public override int GetHashCode() => new { name, symbol, isDesignated }.GetHashCode();

    public override string ToString()
    {
        return symbol;
    }
}