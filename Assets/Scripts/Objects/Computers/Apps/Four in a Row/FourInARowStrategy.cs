public abstract class FourInARowStrategy
{
    public static FourInARowStrategy GetStrategy(
        int mode,
        FourInARowBoard board,
        int playerId)
    {
        // TODO
        switch (mode)
        {
            case 1:
                return new FourInARowLazyStrategy(board, playerId);
            case 2:

                break;
        }

        return new FourInARowUserStrategy(board, playerId);
    }

    public bool IsUser { get => this is FourInARowUserStrategy; }

    public readonly int MyPlayerId;
    public readonly FourInARowBoard Board;

    protected FourInARowStrategy(FourInARowBoard board, int playerId)
    {
        Board = board;
        MyPlayerId = playerId;
    }

    public abstract int FindBestSlotColumnIndex();
}
