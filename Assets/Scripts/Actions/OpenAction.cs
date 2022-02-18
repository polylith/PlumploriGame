using Language;

namespace Action
{
    /// <summary>
    /// This action enables interaction with objects
    /// of type openable or any other interactables
    /// that react in a non-standard way to the open
    /// action.
    /// </summary>
    public class OpenAction : GameAction
    {
        /// <summary>
        /// The natural language descriptor of this
        /// action changes depending on the selected
        /// interactable, to avoid the need of an
        /// additional close action.
        /// </summary>
        /// <param name="interactable">interactable to determine the natural language descriptor</param>
        private void CheckActionType(Interactable interactable = null)
        {
            if (null != interactable && interactable is Openable openable)
            {
                langKey = openable.IsOpen ? LangKey.CloseAction : LangKey.OpenAction;
            }
            else if (null != interactable && interactable is InteractableProxy proxy
                && null != proxy.interactables && proxy.interactables.Length > 0
                && null != proxy.interactables[0] && proxy.interactables[0] is Openable proxyOpenable)
            {
                langKey = proxyOpenable.IsOpen ? LangKey.CloseAction : LangKey.OpenAction;
            }
            else
            {
                langKey = LangKey.OpenAction;
            }
        }

        public override int SetActionState(Interactable interactable)
        {
            CheckActionType(interactable);
            ActionController.GetInstance().UpdateToolTip();
            base.SetActionState(interactable);
            return interactable.IsInteractionEnabled();
        }

        public override void Cancel()
        {
            CheckActionType();
            ClearState();
            base.Cancel();
        }

        public override bool ApplyActionState()
        {
            if (ActionCount == 0)
                return true;

            bool res = ActionState(0).Interact(null);
            CheckActionType();
            ClearState();
            return res;
        }

        public override int CheckActionState()
        {
            if (!IsCurrent || ActionCount == 0)
                return 0;

            return ActionState(0).IsInteractionEnabled();
        }
    }
}