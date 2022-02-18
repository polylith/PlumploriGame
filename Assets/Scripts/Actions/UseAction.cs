using Language;
using UnityEngine;

namespace Action
{
    /// <summary>
    /// <para>
    /// This action enables interaction with objects of type
    /// useable or other objects that react to the use action.
    /// </para>
    /// <para>
    /// This action may require more than one interactable.
    /// Interactables that can be used with themselves, such
    /// as a lamp that can be turned on or off, need only a
    /// single interactable. If a key is to be used with a
    /// door, these two interactables are needed on the action
    /// stack to perform the action.
    /// </para>
    /// </summary>
    public class UseAction : GameAction
    {
        private int actionState;

        protected override void ClearState()
        {
            actionState = 0;
            requiredActionCount = 1;
            base.ClearState();
        }

        public override bool IsActivated()
        {
            return requiredActionCount > 1 && actionState > 0;
        }

        public override void Cancel()
        {
            if (actionState > 0)
            {
                Interactable inter = ActionState(0);

                if (null != inter)
                {
                    Useable useable = ((Useable)inter);
                    UIGame.GetInstance().HideObject();
                    useable.Restore();

                    if (ActionController.GetInstance().inventorybox.IsOpen && useable.IsCollected)
                        AudioManager.GetInstance().PlaySound("useaction.cancel", ActionController.GetInstance().inventorybox.gameObject);
                }
            }

            ClearState();
            base.Cancel();
        }
                
        public override int CheckActionState()
        {
            if (!IsCurrent || ActionCount == 0)
                return 0;

            ShowActionStates();

            if (requiredActionCount == 1)
            {
                if (ActionCount == 0)
                    return 1;

                return ActionState(0).IsInteractionEnabled();
            }

            if (ActionCount < 2)
                return 1;

            return ((Useable)ActionState(0)).IsUseable(ActionState(1));
        }

        public override bool ApplyActionState()
        {
            bool res = true;
            Interactable interactable = ActionState(0);
            ShowActionStates();

            if (requiredActionCount == 1)
            {
                if (ActionCount > 0)
                {
                    res = interactable.Interact(null);

                    if (!res)
                        UIGame.GetInstance().SetCursorDisabled();

                    ClearState();
                    UpdateToolTip();
                }

                return res;
            }
            
            Useable useable = (Useable)interactable;

            switch (ActionCount)
            {
                case 1:
                    UIGame.GetInstance().ShowEscape(true);

                    if (!useable.IsCollected)
                    {
                        if (!useable.Collect())
                        {
                            useable.StoreValues();
                        }
                    }

                    UIGame.GetInstance().ShowObject(interactable);
                    UIGame.GetInstance().ShowObjectOnCursor(interactable);

                    if (useable.IsHighlighted)
                        GameManager.GetInstance().UnHighlight();

                    useable.gameObject.SetActive(false);
                    actionState = 1;
                    break;
                case 2:
                    UIGame.GetInstance().ShowEscape(false);
                    res = useable.Interact(ActionState(1));                   

                    if (!useable.IsCollected)
                        useable.Restore();

                    ClearState();
                    UpdateToolTip();
                    UIGame.GetInstance().HideObject();
                    UIGame.GetInstance().HideObjectOnCursor();
                    break;
            }

            return res;
        }

        public override int SetActionState(Interactable inter)
        {
            int res = 0;

            /*
             * if current use action requires another
             * interactable (requiredActionCount == 2)
             * and first interable is already selected (actionState > 0)
             * second interactable will always be put
             * on stack at position 1.
             */
            if (requiredActionCount > 1 && ActionCount > 0) // actionState > 0)
            {
                Debug.Log("SetActionState " + GetType() + " required " + requiredActionCount + "count " + ActionCount + " state " + actionState);

                res = ((Useable)ActionState(0)).IsUseable(inter);
                base.SetActionState(inter, 1);
                Debug.Log(" ---> SetActionState " + GetType() + " required " + requiredActionCount + "count " + ActionCount + " state " + actionState);
                UpdateToolTip();
                return res;
            }

            ClearState();

            if (inter is Useable useable)
            {
                requiredActionCount = useable.RequiresType() ? 2 : 1;
                res = 1;

                if (requiredActionCount == 1)
                    res = inter.IsInteractionEnabled();
            }
            else
            {
                requiredActionCount = 1;
                res = inter.IsInteractionEnabled();
            }

            base.SetActionState(inter);

            return res;
        }

        public override void UnsetActionState(Interactable inter)
        {
            if (requiredActionCount < 2 || actionState == 0)
                ClearState();
            else
                ActionStateRemove(actionState);

            UpdateToolTip();
        }

        public override void UpdateToolTip()
        {
            // ShowActionStates();

            if (requiredActionCount == 1)
            {
                base.UpdateToolTip();
                return;
            }

            Interactable inter = ActionState(0);
            string text = "...";

            if (null != inter)
                text = LanguageManager.GetText(inter.LangKey) + " ";

            text += LanguageManager.GetText(LangKey.With) + " ";

            if (ActionCount < requiredActionCount)
                text += "...";
            else
            {
                inter = ActionState(1);
                text += LanguageManager.GetText(inter.LangKey);
            }

            int enabledState = ActionController.GetInstance().CheckCurrentActionState();
            text = LanguageManager.GetText(GetType().Name, text);
            UIToolTip.GetInstance().SetText(text, enabledState);
        }
    }
}