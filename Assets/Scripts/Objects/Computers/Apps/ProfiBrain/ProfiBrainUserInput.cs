public class ProfiBrainUserInput : ProfiBrainColorInput
{
    private void Start()
    {
        
    }

    protected override void SetColorIndex()
    {
        SetColorIndex(CurrentSelectedColorIndex);
    }
}
