public class ProfiBrainUserInput : ProfiBrainColorInput
{
    public bool EmptyInputs { get; set; }

    protected override void SetColorIndex()
    {
        int colorIndex = CurrentSelectedColorIndex;

        if (EmptyInputs && ColorIndex > -1)
            colorIndex = -1;

        SetColorIndex(colorIndex);
    }
}
