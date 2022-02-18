using System.Collections;
using System.Collections.Generic;

public class BooleanLogic : Interpretation<BooleanValue>
{
    public BooleanLogic(Assignment<BooleanValue> ass) : base(ass)
    {

    }

    protected override LogicValue EvaluateNegation(Negation f)
    {
        if (null == f)
            return null;

        Formula f1 = f.GetFormula();
        LogicValue lv = Evaluate(f1);
        return null != lv ? lv.Negate(): null;
    }

    protected override LogicValue EvaluateConjunction(Conjunction f)
    {
        if (null == f)
            return null;

        LogicValue bv = BooleanValue.TRUE;
        List<Formula> list = f.GetList();

        foreach (Formula f1 in list)
        {
            bv = Evaluate(f1);

            if (null == bv || !bv.IsDesignated())
                return null != bv ? BooleanValue.FALSE : null;
        }

        return BooleanValue.TRUE;
    }

    protected override LogicValue EvaluateDisjunction(Disjunction f)
    {
        if (null == f)
            return null;

        LogicValue bv = BooleanValue.FALSE;
        List<Formula> list = f.GetList();

        foreach (Formula f1 in list)
        {
            bv = Evaluate(f1);

            if (null != bv && bv.IsDesignated())
                return BooleanValue.TRUE;
        }

        return BooleanValue.FALSE;
    }

    protected override LogicValue EvaluateImplication(Implication f)
    {
        if (null == f)
            return null;

        Formula Q = f.GetConsequent();
        LogicValue bvQ = Evaluate(Q);

        if (null == bvQ)
            return null;
        else if (bvQ.IsDesignated())
            return BooleanValue.TRUE;

        Formula P = f.GetAntecedent();
        LogicValue bvP = Evaluate(P);
        return null != bvP ? (!bvP.IsDesignated() ? BooleanValue.TRUE : BooleanValue.FALSE) : null;
    }
}