namespace Action
{
    /// <summary>
    /// This action calls the natural language description
    /// of an interactable.
    /// </summary>
    public class LookAction : GameAction
    {
        public override void Cancel()
        {
            ClearState();
            base.Cancel();
        }

        public override bool ApplyActionState()
        {
            if (ActionCount == 0)
                return true;

            return ActionState(0).Interact(null);
        }

        public override int CheckActionState()
        {
            if (!IsCurrent || ActionCount == 0)
                return 0;

            return 1;
        }
    }
}