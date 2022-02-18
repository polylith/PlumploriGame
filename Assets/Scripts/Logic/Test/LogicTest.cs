using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogicTest : MonoBehaviour
{
    public TestEntity entityPrefab;
    public CanvasScaler canvasScaler;

    private void Awake()
    {
        canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
    }

    private void Start()
    {
        //FormulaTest();
        EntityTest();
    }

    private void EntityTest()
    {
        WorldDB.InitDB();
        List<TestEntity> entities = new List<TestEntity>();
        int count = 0;

        while (entities.Count < 5)
        {
            TestEntity entity = Instantiate(entityPrefab, transform) as TestEntity;
            entity.transform.name = "Test " + entities.Count;
            entity.Initialize();
            entities.Add(entity);

            RectTransform rectTransform = entity.GetComponent<RectTransform>();
            Vector3 position = Random.insideUnitCircle;
            position.x *= Screen.width * 0.5f;
            position.y *= Screen.height * 0.5f;
            rectTransform.anchoredPosition = position;

            if (!entity.HasDelegate(entity.Prefix + "IsEnabled"))
                count++;
        }

        if (count == 0)
        {
            int zz = Random.Range(0, 100) % entities.Count;
            entities[zz].ResetEnabled();
        }

        foreach (Entity entity in entities)
        {
            entity.Register();
            entity.RegisterGoals();
            //entity.RegisterCurrentState();
        }

        Assignment<BooleanValue> startAss = WorldDB.currentState.GetT();
        
        foreach (string formulaId in WorldDB.rules.Keys)
        {
            Implication rule = WorldDB.rules[formulaId];
            Formula Q = rule.GetConsequent();
            Entity entity = Q.GetEntity();

            if (Q is Negation)
            {
                Formula f = ((Negation)Q).GetFormula();
                entity = f.GetEntity();
            }

            if (!rule.HasAntecedent())
            {
                string prefix = entity.Prefix;
                List<Formula> listDis = new List<Formula>();

                foreach (string prefix2 in WorldDB.goals.Keys)
                {
                    if (!prefix.Equals(prefix2))
                    {
                        Formula f = WorldDB.Get(prefix2);

                        if (null == f)
                        {
                            Assignment<BooleanValue> ass = WorldDB.goals[prefix2];
                            f = ass.ToFormula();
                            f.SetEntity(prefix2, null);
                            WorldDB.RegisterFormula(f);
                        }
                        
                        listDis.Add(f);
                    }
                }

                if (listDis.Count > 0)
                {
                    Formula P = null;

                    if (listDis.Count > 1)
                        P = new Disjunction(listDis);
                    else
                        P = listDis[0];

                    rule.SetAntecedent(P);
                }
            }

            BooleanValue bv = startAss.Get(formulaId);

            if (null != bv && bv.IsDesignated())
            {
                Formula P = rule.GetAntecedent();

                if (Q is Negation)
                    WorldDB.RegisterFormula(new Negation(formulaId, entity, P));
                else
                    P.SetEntity(formulaId, entity);
            }
        }

        WorldDB.ShowDB();
        WorldDB.ShowCurrentState();
    }

    private void FormulaTest()
    {
        WorldDB.InitDB();
        List<Formula> list0 = new List<Formula>();
        List<Formula> list1 = new List<Formula>();
        List<Formula> list2 = new List<Formula>();
        list0.Add(new Atom("A")); // 0
        list0.Add(new Atom("B")); // 1
        list0.Add(new Atom("C")); // 2

        list1.Add(new Negation(list0[0])); // 0
        list1.Add(new Negation(list0[2])); // 1
        list1.Add(new Conjunction(list0)); // 2
        list1.Add(new Disjunction(list0)); // 3
        list1.Add(new Negation(list1[3])); // 4

        list1.Add(new Conjunction()); // 5
        ((Conjunction)list1[5]).AddFormula(list1[2]);
        ((Conjunction)list1[5]).AddFormula(list1[4]);

        list1.Add(new Conjunction()); // 6
        ((Conjunction)list1[6]).AddFormula(list0[1]);
        ((Conjunction)list1[6]).AddFormula(list1[3]);

        list1.Add(new Disjunction()); // 7
        ((Disjunction)list1[7]).AddFormula(list1[0]);
        ((Disjunction)list1[7]).AddFormula(list1[4]);

        list1.Add(new Conjunction()); // 8
        ((Conjunction)list1[8]).AddFormula(list1[1]);
        ((Conjunction)list1[8]).AddFormula(list1[2]);

        list1.Add(new Negation(list1[7])); // 9

        list1.Add(new Implication(list1[0], list1[1])); // 10

        string s = "Formulas\n";

        for (int i = 0; i < list0.Count; i++)
        {
            Formula f1 = list0[i];
            s += "\t" + f1.GetType().Name + " " + f1.GetName() + " = " + f1.ToString() + "\n";

            Formula f2 = f1.Simplify();

            if (null != f2)
                list2.Add(f2);
        }

        s += "\n==================================================\n\n";

        for (int i = 0; i < list1.Count; i++)
        {
            Formula f1 = list1[i];
            s += "\t" + f1.GetType().Name + " " + f1.GetName() + " = " + f1.ToString() + "\n";
        }

        for (int i = 0; i < list1.Count; i++)
        {
            Formula f1 = list1[i];
            Formula f2 = f1.Simplify();

            if (null != f2)
                list2.Add(f2);
        }

        s += "\n==================================================\n\n";

        s += "Simplify\n";

        for (int i = 0; i < list2.Count; i++)
        {
            Formula f1 = list2[i];
            s += "\t" + f1.GetType().Name + " " + f1.GetName() + " = " + f1.ToString() + "\n";
        }

        Debug.Log(s);

        Assignment<BooleanValue> ass = new Assignment<BooleanValue>();
        Interpretation<BooleanValue> interpretation = new BooleanLogic(ass);
        s = "";

        for (int i = 0; i < 8; i++)
        {
            ass = new Assignment<BooleanValue>();
            int index = i;
            int j = 0;

            while (j < 3)
            {
                ass.Set(list0[j].GetName(), index % 2 == 1 ? BooleanValue.TRUE : BooleanValue.FALSE);
                index >>= 1;
                j++;
            }

            s += i + "\t" + ass.ToString() + "\n";

            interpretation.SetAssignment(ass);

            for (int k = 0; k < list1.Count; k++)
            {
                Formula f1 = list1[k];
                LogicValue lv = interpretation.Evaluate(f1);
                s += "\t" + k + "\t" + f1.GetType().Name + " " + f1.GetName() + " = " + f1.ToString() + "\t" + lv + "\n";
            }

            s += "\n";
        }

        Debug.Log(s);
    }
}
