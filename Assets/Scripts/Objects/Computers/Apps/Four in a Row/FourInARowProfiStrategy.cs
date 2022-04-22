using System.Collections.Generic;
using UnityEngine;

public class FourInARowProfiStrategy : FourInARowStrategy
{
    public FourInARowProfiStrategy(FourInARowBoard board, int playerId)
        : base(board, playerId)
    { }

    public override int FindBestSlotColumnIndex()
    {
        return FindBest(Board, MyPlayerId);
    }

    public static int FindBest(FourInARowBoard board, int playerId)
    { 
        int[,] matrix = board.Matrix;
        int bestColumnIndex = -1;

        if (board.InsertCount > 3)
        {
            Debug.Log("==========");

            int maxScore = int.MinValue;

            for (int columnIndex = 0; columnIndex < matrix.GetLength(0); columnIndex++)
            {
                int rowIndex = FindRow(matrix, columnIndex);

                if (rowIndex < matrix.GetLength(1))
                {
                    int score = CheckColumn(matrix, 0, columnIndex, playerId);

                    Debug.Log(playerId + " col " + columnIndex + " score " + score + " max " + maxScore + " -> " + bestColumnIndex);

                    if (score > maxScore)
                    {
                        maxScore = score;
                        bestColumnIndex = columnIndex;
                    }
                }
            }
        }

        if (bestColumnIndex < 0)
        {
            bestColumnIndex = Random.Range(0, board.ColumnCount);

            while (board.HasFreeSlots && !board.HasColumnFreeSlots(bestColumnIndex))
                bestColumnIndex = Random.Range(0, board.ColumnCount);
        }

        return bestColumnIndex;
    }

    private static int CheckColumn(int[,] matrix, int depth, int columnIndex, int playerId)
    {
        int rowIndex = FindRow(matrix, columnIndex);

        if (rowIndex >= matrix.GetLength(1))
            return 0;

        int[] values = CountColors(matrix, columnIndex, rowIndex);
        int colorIndex = values[0];
        int count = values[1];
        
        if (colorIndex < 0)
        {
            return 0;
        }

        count++;

        if (depth == 0 && count > 3)
            return count * 100 * (colorIndex == playerId ? 10 : 5);

        matrix[columnIndex, rowIndex] = colorIndex;
        count = (count > 3 ? 3 : 1) * (colorIndex != playerId ? -2 : 1);
        int score = CheckColumn(matrix, depth + 1, columnIndex, playerId);
        matrix[columnIndex, rowIndex] = -1;

        Debug.Log("(" + columnIndex + ", " + rowIndex + ") " + score + " + " + count);

        return score + count;
    }

    private static int[] CountColors(int[,] matrix, int currentColumnIndex, int currentRowIndex)
    {
        int columnCount = matrix.GetLength(0);
        int rowCount = matrix.GetLength(1);
        int[][] directions = FourInARowBoard.Directions;
        int maxCount = 0;
        int maxColorIndex = -1;

        for (int index = 0; index < directions.Length; index++)
        {
            Dictionary<int, int> counter = new Dictionary<int, int>();
            int columnIndex = currentColumnIndex;
            int rowIndex = currentRowIndex;
            int deltaColumnIndex = directions[index][0];
            int deltaRowIndex = directions[index][1];
            columnIndex += deltaColumnIndex;
            rowIndex += deltaRowIndex;

            for (int direction = 0; direction < 2; direction++)
            {
                int color1 = -1;

                while (true)
                {
                    if (columnIndex < 0 || columnIndex >= columnCount
                    || rowIndex < 0 || rowIndex >= rowCount)
                        break;

                    int color2 = matrix[columnIndex, rowIndex];

                    if (color1 == -1 && color2 != -1)
                    {
                        color1 = color2;

                        if (!counter.ContainsKey(color2))
                        {
                            counter.Add(color2, 0);
                        }

                        counter[color2]++;
                    }
                    else if (color2 == -1 || color1 != color2)
                    {
                        break;
                    }
                    else
                    {
                        if (!counter.ContainsKey(color2))
                        {
                            counter.Add(color2, 0);
                        }

                        counter[color2]++;
                    }

                    columnIndex += deltaColumnIndex;
                    rowIndex += deltaRowIndex;
                }

                deltaColumnIndex *= -1;
                deltaRowIndex *= -1;
                columnIndex = currentColumnIndex;
                rowIndex = currentRowIndex;
                columnIndex += deltaColumnIndex;
                rowIndex += deltaRowIndex;
            }

            foreach (int colorIndex in counter.Keys)
            {
                int count = counter[colorIndex];

                if (count >= maxCount)
                {
                    maxCount = count;
                    maxColorIndex = colorIndex;
                }
            }
        }

        return new int[] { maxColorIndex, maxCount };
    }
}
