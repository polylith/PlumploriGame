using System.Collections.Generic;
using UnityEngine;

public class FourInARowLazyStrategy : FourInARowStrategy
{
    public FourInARowLazyStrategy(FourInARowBoard board, int playerId)
        : base(board, playerId)
    { }

    public override int FindBestSlotColumnIndex()
    {
        return FindBest(Board, MyPlayerId);
    }

    public static int FindBest(FourInARowBoard board, int myPlayerId)
    {
        int columnIndex = -1;

        if (board.InsertCount > 3)
        {
            columnIndex = GetMax(board, myPlayerId);
        }

        if (columnIndex < 0)
        {
            columnIndex = Random.Range(0, board.ColumnCount);

            while (board.HasFreeSlots && !board.HasColumnFreeSlots(columnIndex))
                columnIndex = Random.Range(0, board.ColumnCount);
        }

        return columnIndex;
    }

    private static int GetOtherColorIndex(Dictionary<int, int[]> mapCounter, int columnIndex, int myPlayerId, int value)
    {
        int otherColorIndex = -1;

        foreach (int colorIndex in mapCounter.Keys)
        {
            if (colorIndex != myPlayerId && mapCounter[colorIndex][columnIndex] >= value)
                otherColorIndex = colorIndex;
        }

        return otherColorIndex;
    }

    private static int GetMax(FourInARowBoard board, int myPlayerId)
    {
        int bestColumnIndex = -1;
        Dictionary<int, int[]> mapCounter1 = GetColorCount(board, 0);
        Dictionary<int, int[]> mapCounter2 = GetColorCount(board, 1);
        int max = 1;
        int[] myColorCount = mapCounter1.ContainsKey(myPlayerId)
            ? mapCounter1[myPlayerId] : new int[board.ColumnCount];

        for (int columnIndex = 0; columnIndex < board.ColumnCount; columnIndex++)
        {
            if (board.HasColumnFreeSlots(columnIndex))
            {
                int myCount = myColorCount[columnIndex];
                int otherPlayerId = GetOtherColorIndex(mapCounter1, columnIndex, myPlayerId, myCount);

                if (myCount > 2)
                    return columnIndex;

                if (otherPlayerId < 0)
                {
                    if (myCount > max)
                    {
                        max = myCount;
                        bestColumnIndex = columnIndex;
                    }
                }
                else if (mapCounter1[otherPlayerId][columnIndex] > 2)
                {
                    return columnIndex;
                }
                else if (mapCounter1[otherPlayerId][columnIndex] >= max)
                {
                    if (myCount >= mapCounter1[otherPlayerId][columnIndex]
                        && mapCounter2.ContainsKey(otherPlayerId)
                        && mapCounter2[otherPlayerId][columnIndex] < 3)
                    {
                        if (myCount > max)
                        {
                            max = Mathf.Max(max, myCount);
                            bestColumnIndex = columnIndex;
                        }
                    }
                    else if (mapCounter1[otherPlayerId][columnIndex] > max)
                    {
                        max = mapCounter1[otherPlayerId][columnIndex];
                        bestColumnIndex = columnIndex;
                    }
                }
            }
        }

        return bestColumnIndex;
    }

    private static Dictionary<int, int[]> GetColorCount(FourInARowBoard board, int offSet)
    {
        Dictionary<int, int[]> mapCounter = new Dictionary<int, int[]>();

        for (int columnIndex = 0; columnIndex < board.ColumnCount; columnIndex++)
        {
            int rowIndex = board.NextSlotIndexInColumn(columnIndex) + offSet;

            if (rowIndex > -1 && rowIndex < board.RowCount)
            {
                CountColors(board, columnIndex, rowIndex, ref mapCounter);
            }
        }

        return mapCounter;
    }

    private static void CountColors(FourInARowBoard board, int currentColumnIndex,
        int currentRowIndex, ref Dictionary<int, int[]> mapCounter)
    {
        FourInARowSlot slot = board.GetSlot(currentColumnIndex, currentRowIndex);

        if (null == slot || !slot.IsEmpty)
        {
            return;
        }

        int columnCount = board.ColumnCount;
        int rowCount = board.RowCount;
        int[][] directions = FourInARowBoard.Directions;

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

                    FourInARowSlot slot2 = board.GetSlot(columnIndex, rowIndex);
                    int color2 = slot2.PlayerId;

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
                if (!mapCounter.ContainsKey(colorIndex))
                {
                    mapCounter.Add(colorIndex, new int[columnCount]);
                }

                int currentCountValue = mapCounter[colorIndex][currentColumnIndex];
                int newCountValue = counter[colorIndex];
                mapCounter[colorIndex][currentColumnIndex] = Mathf.Max(currentCountValue, newCountValue);
            }
        }
    }
}
