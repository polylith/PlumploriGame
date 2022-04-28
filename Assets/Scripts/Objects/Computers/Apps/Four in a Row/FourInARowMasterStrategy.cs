using System;

public class FourInARowMasterStrategy : FourInARowStrategy
{
    public FourInARowMasterStrategy(FourInARowBoard board, int playerId)
        : base(board, playerId)
    {}

    public override int FindBestSlotColumnIndex()
    {
        return FindBest(Board, MyPlayerId);
    }

    public static int FindBest(FourInARowBoard board, int myPlayerId)
    {
        int bestColumnIndex = CheckWin(board, myPlayerId);

        if (bestColumnIndex > -1)
            return bestColumnIndex;

        for (int colorIndex = 0; colorIndex < board.MaxPlayers; colorIndex++)
        {
            if (colorIndex != myPlayerId)
            {
                bestColumnIndex = CheckWin(board, colorIndex);

                if (bestColumnIndex > -1)
                    return bestColumnIndex;
            }
        }

        int half = board.ColumnCount >> 1;

        for (int colorIndex = 0; colorIndex < board.MaxPlayers; colorIndex++)
        {
            if (colorIndex != myPlayerId)
            {
                for (int i = 0; i <= half; i++)
                {
                    int[,] matrix = board.Matrix;
                    int j = half - i;
                    int rowIndex = FindRow(matrix, j);

                    if (rowIndex < board.RowCount
                        && !CheckWinForTwoMoves(j, colorIndex, myPlayerId, matrix))
                    {
                        return j;
                    }

                    matrix = board.Matrix;
                    j = half + i;
                    rowIndex = FindRow(matrix, j);

                    if (rowIndex < board.RowCount
                        && !CheckWinForTwoMoves(j, colorIndex, myPlayerId, matrix))
                    {
                        return j;
                    }
                }
            }
        }

        bestColumnIndex = UnityEngine.Random.Range(0, board.ColumnCount);

        while (board.HasFreeSlots && !board.HasColumnFreeSlots(bestColumnIndex))
        {
            bestColumnIndex = UnityEngine.Random.Range(0, board.ColumnCount);
        }

        return bestColumnIndex;
    }

    private static int CheckWin(FourInARowBoard board, int colorIndex)
    {
        for (int columnIndex = 0; columnIndex < board.ColumnCount; columnIndex++)
        {
            int[,] matrix = board.Matrix;

            if (CheckWin(columnIndex, colorIndex, matrix))
            {
                return columnIndex;
            }
        }

        return -1;
    }

    private static bool CheckWinForTwoMoves(int columnIndex, int colorIndex,
        int myColorIndex, int[,] matrix)
    {
        int rowIndex = FindRow(matrix, columnIndex);

        if (rowIndex >= matrix.GetLength(1))
        {
            return false;
        }

        matrix[columnIndex, rowIndex] = myColorIndex;
        return CheckWin(columnIndex, colorIndex, matrix, rowIndex + 1);
    }

    private static bool CheckWin(int columnIndex, int colorIndex,
        int[,] matrix, int nextRowIndex = -1)
    {
        if (nextRowIndex == -1)
        {
            nextRowIndex = FindRow(matrix, columnIndex);
        }

        int rowCount = matrix.GetLength(1);

        if (nextRowIndex >= rowCount)
        {
            return false;
        }

        matrix[columnIndex, nextRowIndex] = colorIndex;
        string vert = "";

        for (int rowIndex = 0; rowIndex <= nextRowIndex; rowIndex++)
        {
            vert += matrix[columnIndex, rowIndex] == colorIndex ? "1" : "0";
        }

        if (vert.Contains("1111"))
        {
            return true;
        }

        int startIndex = Math.Max(columnIndex - 3, 0);
        int endIndex = Math.Min(columnIndex + 4, matrix.GetLength(0));

        string horiz = "";
        string diag1 = "";
        string diag2 = "";

        for (int i = startIndex; i < endIndex; i++)
        {
            horiz += matrix[i, nextRowIndex] == colorIndex ? "1" : "0";
            int j = nextRowIndex + columnIndex - i;
            int k = nextRowIndex - columnIndex + i;

            if (j >= 0 && j < rowCount)
            {
                diag1 += matrix[i, j] == colorIndex ? "1" : "0";
            }

            if (k >= 0 && k < rowCount)
            {
                diag2 += matrix[i, k] == colorIndex ? "1" : "0";
            }
        }

        return horiz.Contains("1111")
            || diag1.Contains("1111")
            || diag2.Contains("1111");
    }
}
