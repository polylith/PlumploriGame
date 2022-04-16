public class TelephoneDevice : ITelephoneDevice
{
    public bool InUse { get => inUse; }

    public string Name { get => deviceName; }

    public string Number { get => number; }

    public bool HasNumber { get => null != number; }

    private string deviceName = "???";
    private string number;
    private bool inUse;

    public TelephoneDevice(string name, string number)
    {
        deviceName = name;
        this.number = number;
        this.number = PhoneDirectory.Register(this);
    }

    public void AnswerCall(ITelephoneDevice phoneDevice)
    {
        inUse = true;
        UnityEngine.Debug.Log(Number + " answer call from " + (null != phoneDevice ? phoneDevice.Number : "unknown"));

        if (phoneDevice is Telephone telephone)
        {
            telephone.AnswerCall(this);
            return;
        }

        GameEvent.GetInstance().Execute(() => {
            phoneDevice.FinishCall();
            FinishCall();
        }, UnityEngine.Random.Range(5f, 15f));
    }

    public void FinishCall()
    {
        inUse = false;
    }

    public void FinishInCall()
    {
        inUse = false;
    }

    public void FinishOutCall()
    {
        inUse = false;
    }

    public void RingOnce()
    {
        // nothing to do
    }
}
