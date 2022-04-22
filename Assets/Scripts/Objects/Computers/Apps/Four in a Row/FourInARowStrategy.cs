public abstract class FourInARowStrategy
{
    protected static int FindRow(int[,] matrix, int columnIndex)
    {
        int rowIndex = 0;

        while (rowIndex < matrix.GetLength(1))
        {
            if (matrix[columnIndex, rowIndex] < 0)
                return rowIndex;

            rowIndex++;
        }

        return rowIndex;
    }

    public static FourInARowStrategy GetStrategy(
        int mode,
        FourInARowBoard board,
        int playerId)
    {
        switch (mode)
        {
            case 1:
                return new FourInARowLazyStrategy(board, playerId);
            case 2:
                return new FourInARowProfiStrategy(board, playerId);
            case 3:
                return new FourInARowMasterStrategy(board, playerId);
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
