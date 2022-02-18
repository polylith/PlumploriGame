public class UIMessage
{
    public string text;
    public bool isError;

    public UIMessage(string text) : this(text, false) { }

    public UIMessage(string text, bool isError)
    {
        this.text = text;
        this.isError = isError;
    }
}