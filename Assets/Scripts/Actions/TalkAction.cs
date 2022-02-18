namespace Action
{
    /// <summary>
    /// This action is used to initiate dialogs.
    /// </summary>
    public class TalkAction : GameAction
    {
        public override bool ApplyActionState()
        {
            // TODO
            return false;
        }

        public override void Cancel()
        {
            ClearState();
            base.Cancel();
        }

        public override int CheckActionState()
        {
            // TODO
            return IsCurrent ? 1 : -1;
        }
    }
}