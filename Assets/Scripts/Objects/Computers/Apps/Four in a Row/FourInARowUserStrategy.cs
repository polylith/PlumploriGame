public class FourInARowUserStrategy : FourInARowStrategy
{
    public FourInARowUserStrategy(FourInARowBoard board, int playerId)
        : base(board, playerId)
    { }

    public override int FindBestSlotColumnIndex()
    {
        // nothing to do, because user sets input in UI
        return -1;
    }
}
