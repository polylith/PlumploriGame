public abstract class Formula
{
    private static int[] counter = new int[2] { 0, 0 };

    private static string GetSymbol(int i)
    {
        string symbol = (i == 0 ? "A" : "F") + counter[i];
        counter[i]++;
        return symbol;
    }

    private string symbol;
    protected string name;
    private Entity entity;

    protected Formula(string name, Entity entity)
    {
        this.name = name;
        this.entity = entity;
        WorldDB.RegisterFormula(this);
    }

    public void SetEntity(string name, Entity entity)
    {
        if (null != name)
            WorldDB.RemoveFormula(name);

        this.name = name;
        this.entity = entity;
        WorldDB.RegisterFormula(this);
    }

    public string GetSymbol()
    {
        if (null == symbol)
            symbol = GetSymbol(IsAtom() ? 0 : 1);

        return symbol;
    }

    public bool HasEntity()
    {
        return null != entity;
    }

    public Entity GetEntity()
    {
        return entity;
    }

    public string GetName()
    {
        if (null == name)
            name = GetSymbol();

        return name;
    }

    public bool IsAtom()
    {
        return this is Atom;
    }

    public override string ToString()
    {
        return this.GetType().Name.ToUpper();
    }

    public abstract Formula Simplify();
}