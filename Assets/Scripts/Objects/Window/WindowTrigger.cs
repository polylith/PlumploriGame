using Action;

public class WindowTrigger : Openable
{
    public WindowHandle handle;

    private void Awake()
    {
        SetActive(false);
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public override void RegisterGoals()
    {
        // TODO
    }
    
    public void SetActive(bool isEnabled)
    {
        col.enabled = isEnabled;
    }

    public override int IsInteractionEnabled()
    {
        if (!ActionController.GetInstance().IsCurrentAction(typeof(OpenAction)))
            return base.IsInteractionEnabled();

        return handle.State == 0 ? -1 : 1;
    }

    public override bool Interact(Interactable interactable)
    {
        ActionController actionController = ActionController.GetInstance();

        if (!actionController.IsCurrentAction(typeof(OpenAction)))
            return base.Interact(interactable);

        if (handle.State == 0)
            return false;

        isOpen = !isOpen;
        Fire("IsOpen", IsOpen);
        handle.Open(IsOpen);
        return true;
    }
}
