public interface ITelephoneDevice
{
    public bool InUse { get; }
    public string Name { get; }
    public string Number { get; }
    public bool HasNumber { get; }
    public PhoneBookEntry PBEntry { get; }

    public void FinishCall();
    public void FinishInCall();
    public void FinishOutCall();
    public void AnswerCall(ITelephoneDevice phoneDevice);
    public void RingOnce();
}
