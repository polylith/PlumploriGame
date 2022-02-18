public abstract class Interpretation<T> where T : LogicValue
{
    private Assignment<T> ass;

    protected Interpretation(Assignment<T> ass)
    {
        this.ass = ass;
    }

    public void SetAssignment(Assignment<T> ass)
    {
        this.ass = ass;
    }

    public LogicValue Evaluate(Formula f)
    {
        LogicValue lv = null;

        if (f is Negation)
        {
            lv = EvaluateNegation((Negation)f);
        }
        else if (f is Conjunction)
        {
            lv = EvaluateConjunction((Conjunction)f);
        }
        else if (f is Disjunction)
        {
            lv = EvaluateDisjunction((Disjunction)f);
        }
        else if (f is Implication)
        {
            lv = EvaluateImplication((Implication)f);
        }
        else if (f is Atom)
        {
            string id = f.GetName();

            if (null != ass && ass.Contains(id))
                lv = ass.Get(id);
        }

        return lv;
    }

    protected abstract LogicValue EvaluateNegation(Negation f);
    protected abstract LogicValue EvaluateConjunction(Conjunction f);
    protected abstract LogicValue EvaluateDisjunction(Disjunction f);
    protected abstract LogicValue EvaluateImplication(Implication f);
}
