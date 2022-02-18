using System.Collections.Generic;

namespace Action
{
    /// <summary>
    /// This data structure stores the attributes for an action
    /// to be executed. These values must be temporarily stored
    /// and restored because when the current player is animated
    /// to reach the interaction point, the action state may
    /// have changed. 
    /// </summary>
    public class ActionState
    {
        public List<Interactable> Stack { get; private set; }
        public GameAction Action { get; private set; }
        public int Requires { get; }

        public ActionState(GameAction action, List<Interactable> stack,
            int requiredActionCount)
        {
            Action = action;
            Stack = new List<Interactable>(stack);
            Requires = requiredActionCount;
        }
    }
}
