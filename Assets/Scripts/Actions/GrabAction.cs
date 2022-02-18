namespace Action
{
    /// <summary>
    /// This action can be selected to grab an object.
    /// </summary>
    public class GrabAction : GameAction
    {
        public override bool ApplyActionState()
        {
            if (ActionCount == 0)
                return true;

            bool res = ActionState(0).Interact(null);

            if (res)
                AudioManager.GetInstance().PlaySound("grabaction", ActionController.GetInstance().inventorybox.gameObject);

            ClearState();
            UpdateToolTip();
            return res;
        }

        public override int SetActionState(Interactable interactable)
        {
            int res = 0;

            if (IsCurrent)
            {
                if (interactable is Collectable collectable)
                {
                    res = collectable.IsCollected ? -1 : 1;
                }
                else
                {
                    res = -1;
                }
            }

            base.SetActionState(interactable);
            return res;
        }

        public override int CheckActionState()
        {
            if (!IsCurrent || ActionCount == 0)
                return 0;

            Interactable interactable = ActionState(0);

            if (!(interactable is Collectable collectable))
                return 0;

            return collectable.IsCollected ? 0 : 1;
        }

        public override void Cancel()
        {
            if (ActionCount > 0)
            {
                Interactable interactable = ActionState(0);

                if (null != interactable
                    && interactable is Collectable collectable)
                    collectable.Restore();
            }

            ClearState();
            base.Cancel();
        }
    }    
}