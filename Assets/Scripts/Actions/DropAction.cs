namespace Action
{
    /// <summary>
    /// This action can be selected to drop an object
    /// that has been collected and set as dropable in
    /// one of several possible positions in the current
    /// scene.
    /// </summary>
    public class DropAction : GameAction
    {
        public override bool ApplyActionState()
        {
            if (ActionCount == 0 || ActionState(0).Interact(null))
                return true;

            return false;
        }
        
        public override int SetActionState(Interactable inter)
        {
            int res = -1;

            if (IsCurrent)
            {
                if (IsActivated())
                    return -1;
                else if (inter is Collectable collectable)
                    res = collectable.IsCollected && collectable.isDropAble ? 1 : -1; 
            }

            /*
             * The function of the superclass has just to be called, 
             * but the result is not used for return value.
             */
            base.SetActionState(inter, 0);
            return res;
        }

        public override int CheckActionState()
        {
            if (!IsCurrent || ActionCount == 0)
                return -1;

            Interactable inter = ActionState(0);

            if (inter is Collectable collectable)
            {
                return collectable.IsCollected
                    && collectable.isDropAble ? 1 : -1;
            }    
            
            return -1;
        }

        /// <summary>
        /// This function tells whether the drop action is active.
        /// </summary>
        /// <returns>true = drop action is active</returns>
        public override bool IsActivated()
        {
            if (!IsCurrent || ActionCount == 0)
                return false;

            Interactable inter = ActionState(0);

            if (!(inter is Collectable collectable) || !collectable.isDropAble)
                return false;

            return collectable.IsDropping;
        }

        public override void Finish()
        {
            GameManager.GetInstance().ShowObjectPlaces(false);
            base.Finish();
        }

        public override void Cancel()
        {
            if (ActionCount > 0)
            {
                Interactable interactable = ActionState(0);

                if (null != interactable)
                {
                    ((Collectable)interactable).FinishDrop();
                    interactable.gameObject.SetActive(false);

                    if (interactable.IsHighlighted)
                        GameManager.GetInstance().UnHighlight();

                    ActionController.GetInstance().inventorybox.Open();
                }
            }

            ClearState();
            base.Cancel();
        }
    }
}