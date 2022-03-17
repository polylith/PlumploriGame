using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Action
{
    /// <summary>
    /// <para>
    /// The ActionController is a singleton that takes care
    /// of triggering the current game action and handling of
    /// the interactions.
    /// </para>
    /// <para>
    /// As a MonoBehaviour it is considered for the presentation
    /// of the ActionBar in the UI and also controls the logical
    /// flow of the changing of the current game action.
    /// </para>
    /// <para>
    /// The inventory box is also part of the ActionController.
    /// </para>
    /// </summary>
    public class ActionController : MonoBehaviour
    {
        public static ActionController GetInstance()
        {
            return ins;
        }

        private static ActionController ins;

        public bool IsVisible { get => isVisible; }
        public GameAction Current { get => currentAction; }
        public Inventorybox inventorybox;
        public RectTransform actionsParent;
        public GameAction defaultAction;
        public ShowMap showMap;

        private GameAction currentAction;
        private bool isVisible = true;
        private IEnumerator ieScale;
        private readonly Dictionary<string, GameAction> actionMap = new Dictionary<string, GameAction>();

        public Inventory GetInventory()
        {
            return inventorybox.inventory;
        }

        public void CloseInventorybox()
        {
            inventorybox.Close();
        }

        private void Awake()
        {
            if (null == ins)
            {
                ins = this;
                Init();
                // TODO hide at start instant
                // because the menu has to be shown first
                // Show(false, true);
                Show(true, true);
            }
            else
            {
                Destroy(gameObject);
            }            
        }

        private void Init()
        {
            GameAction[] actions = actionsParent.GetComponentsInChildren<GameAction>();

            foreach (GameAction gameAction in actions)
            {
                string actionName = gameAction.GetType().FullName.ToLower();
                actionMap.Add(actionName, gameAction);
                gameAction.Init();
            }

            SetCurrentAction(defaultAction);
        }

        public void UpdateMap()
        {
            showMap.UpdateMap();
        }

        public void Show(bool isVisible, bool instant = false)
        {
            if (this.isVisible == isVisible)
                return;

            if (null != ieScale)
                StopCoroutine(ieScale);

            this.isVisible = isVisible;

            if (!isVisible)
                UIToolTip.GetInstance()?.Hide();

            ieScale = null;
            float value = (isVisible ? 1f : 0f);
            Vector3 scale = new Vector3(1f, value, 1f);

            if (!isVisible && null != inventorybox && inventorybox.IsOpen)
                inventorybox.Close();

            if (instant)
            {
                actionsParent.localScale = scale;

                if (isVisible)
                    UpdateToolTip();

                return;
            }

            ieScale = IEScale(scale);

            if (!instant)
            {
                AudioManager.GetInstance().PlaySound(isVisible ? "max" : "min");
            }
            
            StartCoroutine(ieScale);
        }

        private IEnumerator IEScale(Vector3 scale)
        {
            float f = 0f;

            while (f <= 1f)
            {
                actionsParent.localScale = Vector3.Lerp(actionsParent.localScale, scale, f);
                yield return null;
                f += Time.deltaTime;
            }

            actionsParent.localScale = scale;

            if (isVisible)
                UpdateToolTip();

            ieScale = null;            
        }

        public void HighlightCurrent()
        {
            if (null == currentAction)
                return;

            currentAction.Highlight();
        }

        public void SetDefaultAction()
        {
            SetCurrentAction(defaultAction);
        }

        public void SetCurrentAction(System.Type actionType)
        {
            string actionName = actionType.ToString().ToLower();

            Debug.Log("SetCurrentAction " + actionName);

            if (!actionMap.ContainsKey(actionName))
                return;

            GameAction gameAction = actionMap[actionName];
            SetCurrentAction(gameAction);
        }

        public void SetCurrentAction(GameAction action)
        {
            if (Current == action)
                return;

            if (null != Current)
            {
                currentAction.Cancel();
                currentAction.IsCurrent = false;
            }

            currentAction = action;
            currentAction.IsCurrent = true;
            UIGame.GetInstance()?.SetCursor(currentAction);
        }

        public bool IsCurrentActionActive()
        {
            if (null == Current)
                return false;

            return Current.IsActivated();
        }

        public bool IsDropActionActive()
        {
            return IsCurrentAction(typeof(DropAction)) && Current.IsActivated();
        }

        public bool IsCurrentAction(System.Type actionType)
        {
            if (null == Current || null == actionType)
                return false;

            return currentAction.GetType().ToString().Equals(actionType.ToString()); 
        }

        public ActionState GetCurrentActionState()
        {
            return null != Current ? Current.GetActionState() : null;
        }

        public void RestoreActionState(ActionState actionState)
        {
            SetCurrentAction(actionState.Action);
            Current.RestoreActionState(actionState.Stack, actionState.Requires);
        }

        public int CheckCurrentActionState()
        {
            if (null == Current)
                return 0;

            return Current.CheckActionState();
        }

        public int SetActionState(Interactable inter)
        {
            return Current.SetActionState(inter);
        }

        public void ApplyActionState()
        {
            if (null == Current)
                return;

            bool res = Current.ApplyActionState();

            if (Current is PointerAction)
            {
                Current.Finish();
                UpdateToolTip();
                return;
            }

            if (!res)
                GameEvent.GetInstance().Execute(ActionDenied, 0.25f);

            if (!(Current is DropAction) && !(Current is UseAction) || Current.Requires < 2)
            {
                UIGame.GetInstance().SetCursorVisible(true);
                UIToolTip.GetInstance().Hide();
                GameEvent.GetInstance().Execute(RestoreUI, 1f);
            }
        }

        public void ShowTempText(string sText)
        {
            UIToolTip uiToolTip = UIToolTip.GetInstance();
            uiToolTip.SetText(sText, 1);
            uiToolTip.Show(defaultAction.transform);
        }

        private void RestoreUI()
        {
            if (null != Current)
                Current.UpdateToolTip();
        }

        public void ActionDenied()
        {
            AudioManager.GetInstance().PlaySound("action.denied", gameObject);
        }

        public void DenyAction()
        {
            ActionDenied();
            Current?.Cancel();
        }

        public void UnsetActionState(Interactable inter)
        {
            Current?.UnsetActionState(inter);
        }

        public bool IsApplyable()
        {
            return currentAction.IsApplyable();
        }

        public void UpdateToolTip()
        {
            UIGame.GetInstance().RestoreCursor();
            UIGame.GetInstance().SetCursorEnabled(false, true);
            UIToolTip.GetInstance().Restore();

            if (null != Current)
                Current.UpdateToolTip();
        }

        /// <summary>
        /// Is called by Unity Engine in every frame.
        /// </summary>
        private void Update()
        {
            if (UIGame.GetInstance().IsUIExclusive)
                return;

            // when user presses escape
            if (Input.GetKeyDown(KeyCode.Escape)
                && null != Current && Current.IsActivated())
            {
                // the current active action is cancelled.
                Current.Cancel();
                UIGame.GetInstance().ShowEscape(false);                
            }
        }
    }
}