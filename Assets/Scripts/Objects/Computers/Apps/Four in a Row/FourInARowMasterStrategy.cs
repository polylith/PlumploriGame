using System;
using System.Collections.Generic;

public class FourInARowMasterStrategy : FourInARowStrategy
{
    private readonly List<int> playerIds = new List<int>();

    public FourInARowMasterStrategy(FourInARowBoard board, int playerId)
        : base(board, playerId)
    {
        playerIds.Add(playerId);

        for (int colorIndex = 0; colorIndex < 4; colorIndex++)
        {
            if (colorIndex != playerId)
            {
                playerIds.Add(colorIndex);
            }
        }
    }

    public override int FindBestSlotColumnIndex()
    {
        int bestColumnIndex = -1;

        if (Board.InsertCount > 3)
        {
            bestColumnIndex = FindBest();
        }

        if (bestColumnIndex < 0)
        {
            bestColumnIndex = UnityEngine.Random.Range(0, Board.ColumnCount);

            while (Board.HasFreeSlots && !Board.HasColumnFreeSlots(bestColumnIndex))
            {
                bestColumnIndex = UnityEngine.Random.Range(0, Board.ColumnCount);
            }
        }

        return bestColumnIndex;
    }

    private int FindBest()
    {
        foreach (int colorIndex in playerIds)
        {
            for (int columnIndex = 0; columnIndex < Board.ColumnCount; columnIndex++)
            {
                int[,] matrix = Board.Matrix;

                if (CheckWin(columnIndex, colorIndex, matrix))
                {
                    return columnIndex;
                }
            }
        }

        int half = Board.ColumnCount >> 1;

        for (int index = 1; index < playerIds.Count; index++)
        {
            for (int i = 0; i <= half; i++)
            {
                int[,] matrix = Board.Matrix;
                int j = half - i;
                int rowIndex = FindRow(matrix, j);

                if (rowIndex < Board.RowCount
                    && !CheckWinForTwoMoves(j, playerIds[index], MyPlayerId, matrix))
                {
                    return j;
                }

                matrix = Board.Matrix;
                j = half + i;
                rowIndex = FindRow(matrix, j);

                if (rowIndex < Board.RowCount
                    && !CheckWinForTwoMoves(j, playerIds[index], MyPlayerId, matrix))
                {
                    return j;
                }
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
        int[,] matrix, int rowIndex = -1)
    {
        if (rowIndex == -1)
        {
            rowIndex = FindRow(matrix, columnIndex);
        }

        int rowCount = matrix.GetLength(1);

        if (rowIndex >= rowCount)
        {
            return false;
        }

        matrix[columnIndex, rowIndex] = colorIndex;

        string vert = "";
        int startIndex = Math.Max(rowIndex - 3, 0);
        int endIndex = Math.Min(rowIndex + 4, rowCount);

        for (int i = startIndex; i < endIndex; i++)
        {
            vert += matrix[colorIndex, i] == colorIndex ? "1" : "0";
        }

        if (vert.Contains("1111"))
        {
            return true;
        }

        startIndex = Math.Max(columnIndex - 3, 0);
        endIndex = Math.Min(columnIndex + 4, matrix.GetLength(0));

        string horiz = "";
        string diag1 = "";
        string diag2 = "";

        for (int i = startIndex; i < endIndex; i++)
        {
            horiz += matrix[i, rowIndex] == colorIndex ? "1" : "0";
            int j = rowIndex + columnIndex - i;
            int k = rowIndex - columnIndex + i;

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
