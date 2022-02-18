using System.Collections.Generic;
using UnityEngine;
using Language;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Action
{
    /// <summary>
    /// <para>
    /// In this abstract base class all attributes, methods
    /// and functions for specific game actions are encapsulated.
    /// These classes are responsible for the functional handling
    /// of actions as well as for the representation of the action
    /// as an icon in the action bar within the UI.
    /// </para>
    /// <para>
    /// Each game action has three possible states:
    /// <list type="bullet">
    /// <item>-1 disabled</item>
    /// <item>0 no interaction</item>
    /// <item>1 enabled</item>
    /// </list>
    /// Each of these states has a different cursors
    /// in the size of 32x32 pixels.
    /// </para>
    /// <para>
    /// For each cursor a different pivot point can be defined.
    /// By default, this pivot point is at (0, 0).
    /// However, the handling feels strange with some cursors when
    /// the pivot point does not match the intuitive expectation.
    /// The action is displayed with a specific image in the action bar.
    /// </para>
    /// <para>
    /// Each game action has a lang key that is translated differently
    /// into a natural language text depending on the selected language.
    /// </para>
    /// <para>
    /// To enable object-related interaction in the game, there is a
    /// stack on which the interactive objects are placed when the mouse
    /// pointer is placed over them.  Most actions require only one
    /// interactable to which the action refers. e.g. the pointer action,
    /// the grab action, the drop action, the open action. only the
    /// use action may require either one or two interactables.
    /// </para>
    /// </summary>
    public abstract class GameAction : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public static readonly Color[] colors = new Color[]
        {
            Color.gray,
            Color.white,
            Color.yellow
        };

        public bool IsCurrent { get => isCurrent; set => SetCurrent(value); }
        public int ActionCount { get => actionState.Count; }
        public int Requires { get => requiredActionCount; }

        public LangKey langKey;
        /// <summary>
        /// Normal cursor for no interaction
        /// </summary>
        public Texture2D defaultTexture;
        /// <summary>
        /// Cursor when interaction is endabled
        /// </summary>
        public Texture2D enabledTexture;
        /// <summary>
        /// Cursor when interaction is disabled
        /// </summary>
        public Texture2D disabledTexture;
        /// <summary>
        /// Individual pivot for each of the three cursors
        /// </summary>
        public Vector2[] pivots = new Vector2[] {
            Vector2.zero,
            Vector2.zero,
            Vector2.zero
        };

        private readonly List<Interactable> actionState = new List<Interactable>();
        /// <summary>
        /// Number of interactables needed. In general this is 1,
        /// only for use action there might be 2 interactables
        /// needed, when a useable has defined a required typ to
        /// be used with.
        /// </summary>
        protected int requiredActionCount = 1;

        /// <summary>
        /// Image in to display action icon. It's just colored
        /// on mouse exit and mouse out.
        /// </summary>
        private Image image;
        /// <summary>
        /// Indication whether this action is currently selected.
        /// </summary>
        private bool isCurrent = false;
                
        /// <summary>
        /// Initialize the icon in actions panel in game ui.
        /// </summary>
        public void Init()
        {
            image = GetComponent<Image>();
            UpdateState(-1);
        }

        private void UpdateState(int i)
        {
            if (i < 0)
                i = isCurrent ? 2 : 0;

            image.color = colors[i];
        }

        public void Highlight(int i = -1)
        {
            UpdateState(i);
            UpdateToolTip();
        }

        /// <summary>
        /// On mouse over on the UI element in the action bar.
        /// This method is called by Unity. A manual call is
        /// not possible.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isCurrent)
            {
                UIGame.GetInstance().SetCursorEnabled(false, false);
                return;
            }

            UIGame.GetInstance().SetCursorEnabled(true, false);
            Highlight(1);
        }

        /// <summary>
        /// On mouse exit on the UI element in the action bar.
        /// This method is called by Unity. A manual call is
        /// not possible.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            UIGame.GetInstance().SetCursorEnabled(false, false);

            if (isCurrent)
            {
                return;
            }

            UpdateState(-1);
            UIToolTip.GetInstance().Restore();
        }

        /// <summary>
        /// On mouse click on the UI element in the action bar.
        /// This method is called by Unity. A manual call is
        /// not possible.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (isCurrent)
            {
                return;
            }

            SetCurrent(!isCurrent);
            UpdateToolTip();
            UIGame.GetInstance().SetCursorEnabled(false, false);
        }

        /// <summary>
        /// Regular ending of an action. This method may need to
        /// be overridden in subclasses. However, the function
        /// from the superclass must always be called.
        /// </summary>
        public virtual void Finish()
        {
            ClearState();
        }

        /// <summary>
        /// Cancel the current action. This method may need to
        /// be overridden in subclasses. However, the function
        /// from the superclass must always be called.
        /// </summary>
        public virtual void Cancel()
        {
            GameManager.GetInstance().UnHighlight();
            UIGame.GetInstance().ShowEscape(false);
            UIGame.GetInstance().HideObjectOnCursor();
            UIGame.GetInstance().SetCursorEnabled(false, true);
            UpdateToolTip();

            Debug.Log(this.GetType().Name + " cancelled");
        }

        /// <summary>
        /// Get the specific cursor image for an action.
        /// </summary>
        /// <param name="isEnabled">is action enabled</param>
        /// <returns>texture for the cursor</returns>
        public Texture2D GetEnabled(bool isEnabled = false)
        {
            return isEnabled ? enabledTexture : defaultTexture;
        }

        /// <summary>
        /// Get the specific cursor image for an disabled action.
        /// </summary>
        /// <returns>texture for the cursor</returns>
        public Texture2D GetDisabled()
        {
            return disabledTexture;
        }

        /// <summary>
        /// Get the specific pivot of a cursor image.
        /// </summary>
        /// <param name="enabledState">state of action</param>
        /// <returns>pivot for the cursor image</returns>
        public Vector2 GetPivots(int enabledState)
        {
            return pivots[enabledState + 1];
        }

        /// <summary>
        /// Get the current state of the action for restoring.
        /// </summary>
        /// <returns>state to restore action on execution</returns>
        public ActionState GetActionState()
        {
            return new ActionState(
                this,
                actionState,
                Requires
            );
        }

        /// <summary>
        /// Restores the state of the action.
        /// </summary>
        /// <param name="stack">involved interactables</param>
        /// <param name="requiredActionCount">number of needed interactables</param>
        public void RestoreActionState(List<Interactable> stack, int requiredActionCount)
        {
            int n = Mathf.Min(actionState.Count, stack.Count);
            /* 
             * Quickcheck whether action stack has changed.
             * If these counters have changed, the stack has 
             * certainly changed.
             */
            bool isEqual = actionState.Count == stack.Count && Requires == requiredActionCount;

            /* 
             * If counters are equal, deepcheck wheter
             * the interactables have changed.
             */
            if (isEqual)
            {
                for (int i = 0; i < n; i++)
                {
                    if (actionState[i] != stack[i])
                    {
                        isEqual = false;
                        break;
                    }
                }
            }

            // if still equal, don't do anything
            if (isEqual)
            {
                return;
            }

            ClearState();
            this.requiredActionCount = requiredActionCount;
            actionState.AddRange(stack);
            UpdateToolTip();
        }

        /// <summary>
        /// Get the i-th interactable from stack.
        /// </summary>
        /// <param name="i">index of the interactable on stack</param>
        /// <returns>the interactable or null if index is out of bounds</returns>
        protected Interactable ActionState(int i)
        {
            if (i < 0 || i >= actionState.Count)
                return null;

            return actionState[i];
        }

        /// <summary>
        /// Remove the i-th interactable from stack.
        /// It will not do anything if index is out
        /// of bounds.
        /// </summary>
        /// <param name="i">index of the interactable on stack</param>
        protected void ActionStateRemove(int i)
        {
            if (i < 0 || i >= actionState.Count)
                return;

            actionState.RemoveAt(i);
        }

        public abstract bool ApplyActionState();

        /// <summary>
        /// Sets an interactable on index 0.
        /// </summary>
        /// <param name="interactable">main interactable for this action</param>
        /// <returns>enabled state</returns>
        public virtual int SetActionState(Interactable interactable)
        {
            int res = SetActionState(interactable, 0);
            UpdateToolTip();
            return res;
        }

        /// <summary>
        /// Sets an interactable at a given index.
        /// </summary>
        /// <param name="interactable">interactable to put on stack</param>
        /// <param name="i">index on stack</param>
        /// <returns>enabled state</returns>
        public int SetActionState(Interactable interactable, int i)
        {
            if (i >= actionState.Count)
                actionState.Add(interactable);
            else
                actionState[i] = interactable;

            return 1;
        }

        /// <summary>
        /// Clear the action state. This method may need to
        /// be overridden in subclasses. However, the function
        /// from the superclass must always be called.
        /// </summary>
        /// <param name="interactable">active interactable for this action</param>
        public virtual void UnsetActionState(Interactable interactable)
        {
            ClearState();
            UpdateToolTip();
        }

        /// <summary>
        /// Is action active? This function may need to
        /// be overridden in subclasses.
        /// </summary>
        /// <returns>true = action is active</returns>
        public virtual bool IsActivated()
        {
            return false;
        }

        /// <summary>
        /// Clear the stack. This method may need to
        /// be overridden in subclasses. However, the function
        /// from the superclass must always be called.
        /// </summary>
        protected virtual void ClearState()
        {
            if (actionState.Count > 0)
                actionState.Clear();

            UIGame uiGame = UIGame.GetInstance();
            uiGame.HideObjectOnCursor();
            uiGame.SetCursorEnabled(false, true);
            GameManager.GetInstance().UnHighlight();
        }

        /// <summary>
        /// Determine wether this action might be applied.
        /// This method must be overridden in subclasses. 
        /// </summary>
        /// <returns>enabled state</returns>
        public abstract int CheckActionState();

        public bool IsApplyable()
        {
            return actionState.Count == requiredActionCount;
        }

        /// <summary>
        /// Update the tooltip displaying current action and
        /// the interactables on action stack.
        /// </summary>
        public virtual void UpdateToolTip()
        {
            string text = "";
            int i = 0;

            while (i < actionState.Count)
            {
                Interactable inter = actionState[i];
                text += inter.GetText() + " ";
                i++;
            }

            if (i < requiredActionCount && !(this is PointerAction))
                text += "...";

            text = LanguageManager.GetText(langKey, text);
            UIToolTip.GetInstance().SetText(text, 1);
            UIToolTip.GetInstance().Show(transform);
        }

        /// <summary>
        /// Sets wether this is the current action
        /// </summary>
        /// <param name="isCurrent">true = set, false = unset</param>
        private void SetCurrent(bool isCurrent)
        {
            if (this.isCurrent == isCurrent)
                return;

            this.isCurrent = isCurrent;
            UpdateState(-1);

            if (isCurrent)
                ActionController.GetInstance().SetCurrentAction(this);
        }

        /// <summary>
        /// Just for debugging
        /// </summary>
        public void ShowActionStates()
        {
            string s = GetType().Name + " " + ActionCount + " / " + requiredActionCount;

            for (int i = 0; i < actionState.Count; i++)
                s += "\n\t" + i + " -> " + actionState[i].GetType().Name;

            Debug.Log(s);
        }
    }
}