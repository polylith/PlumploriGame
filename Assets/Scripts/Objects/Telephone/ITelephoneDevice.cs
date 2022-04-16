public interface ITelephoneDevice
{
    public bool InUse { get; }
    public string Number { get; }
    public bool HasNumber { get; }

    public void FinishCall();
    public void FinishInCall();
    public void FinishOutCall();
    public void AnswerCall(ITelephoneDevice phoneDevice);
    public void RingOnce();
}
