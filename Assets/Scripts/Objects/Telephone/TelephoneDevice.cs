public class TelephoneDevice : ITelephoneDevice
{
    public bool InUse { get => inUse; }

    public string Name { get => PBEntry.Name; }

    public string Number { get => PBEntry.Number; }

    public bool HasNumber { get => null != PBEntry && null != PBEntry.Number; }

    public PhoneBookEntry PBEntry { get; private set; }

    private bool inUse;

    public TelephoneDevice(PhoneBookEntry entry)
    {
        PBEntry = entry;
        PBEntry.Number = PhoneDirectory.Register(this);
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
