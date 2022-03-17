/// <summary>
/// <para>
/// This struct represents a parsed user input in prompt app.
/// TODO Unify this structure with the predefined commands in
/// the prompt app. Then it maybe better should be a class rather
/// than a struct.
/// </para>
/// </summary>
public struct PromptCommand
{
    public readonly string code;
    public readonly string[] args;
    public int NumArgs { get => null != args ? args.Length : 0; }

    public PromptCommand(string code, string[] args)
    {
        this.code = (null != code ? code.ToUpper() : "");
        this.args = args;
    }
}