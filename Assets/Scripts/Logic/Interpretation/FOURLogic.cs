using System.Collections;
using System.Collections.Generic;

public class FOURLogic : Interpretation<FOURValue>
{
    public FOURLogic(Assignment<FOURValue> ass) : base(ass)
    {

    }

    protected override LogicValue EvaluateNegation(Negation f)
    {
        if (null == f)
            return FOURValue.BOT;

        Formula f1 = f.GetFormula();
        LogicValue fv = Evaluate(f1);
        return fv.Negate();
    }

    protected override LogicValue EvaluateConjunction(Conjunction f)
    {
        if (null == f)
            return FOURValue.BOT;

        LogicValue fv = FOURValue.TRUE;
        List<Formula> list = f.GetList();

        foreach (Formula f1 in list)
        {
            fv = fv.Conjunct(Evaluate(f1));

            if ( !fv.IsDesignated())
                return FOURValue.FALSE;
        }

        return fv;
    }

    protected override LogicValue EvaluateDisjunction(Disjunction f)
    {
        if (null == f)
            return FOURValue.BOT;

        LogicValue fv = FOURValue.FALSE;
        List<Formula> list = f.GetList();

        foreach (Formula f1 in list)
        {
            fv = fv.Disjunct(Evaluate(f1));

            if (fv.IsDesignated())
                return FOURValue.TRUE;
        }

        return fv;
    }

    protected override LogicValue EvaluateImplication(Implication f)
    {
        if (null == f)
            return FOURValue.BOT;

        Formula P = f.GetAntecedent();
        Formula Q = f.GetConsequent();
        LogicValue fvP = Evaluate(P);
        LogicValue fvQ = Evaluate(Q);
        return fvP.Negate().Disjunct(fvQ);
    }
}
