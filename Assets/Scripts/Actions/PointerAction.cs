namespace Action
{
    /// <summary>
    /// This action is used only to point on interactive
    /// objects and let the current player go to that
    /// position or a position on the walkable ground.
    /// </summary>
    public class PointerAction : GameAction
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

            return ActionState(0).IsInteractionEnabled();
        }
    }
}
