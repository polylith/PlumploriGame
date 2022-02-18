public class Atom : Formula
{
    public Atom(string name, Entity entity = null) : base(name, entity)
    {
    }

    public override Formula Simplify()
    {
        return this;
    }

    public override bool Equals(object o)
    {
        if (null == o || !(o is Atom))
            return false;

        Atom a = (Atom)o;

        if (this == a)
            return true;

        return a.GetName().Equals(GetName());
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return GetName();
    }
}