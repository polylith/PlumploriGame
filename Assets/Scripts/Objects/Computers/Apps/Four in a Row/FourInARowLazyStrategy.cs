using System.Collections.Generic;
using UnityEngine;

public class FourInARowLazyStrategy : FourInARowStrategy
{
    public FourInARowLazyStrategy(FourInARowBoard board, int playerId)
        : base(board, playerId)
    { }

    public override int FindBestSlotColumnIndex()
    {
        int columnIndex = -1;

        if (Board.InsertCount > 3)
        {
            columnIndex = GetMax();
        }

        if (columnIndex < 0)
        {
            columnIndex = Random.Range(0, Board.ColumnCount);

            while (Board.HasFreeSlots && !Board.HasColumnFreeSlots(columnIndex))
                columnIndex = Random.Range(0, Board.ColumnCount);
        }

        return columnIndex;
    }

    private int GetOtherColorIndex(Dictionary<int, int[]> mapCounter, int columnIndex, int value)
    {
        int otherColorIndex = -1;

        foreach (int colorIndex in mapCounter.Keys)
        {
            if (colorIndex != MyPlayerId && mapCounter[colorIndex][columnIndex] >= value)
                otherColorIndex = colorIndex;
        }

        return otherColorIndex;
    }

    private int GetMax()
    {
        int bestColumnIndex = -1;
        Dictionary<int, int[]> mapCounter1 = GetColorCount(0);
        Dictionary<int, int[]> mapCounter2 = GetColorCount(1);
        int max = 1;
        int[] myColorCount = mapCounter1.ContainsKey(MyPlayerId)
            ? mapCounter1[MyPlayerId] : new int[Board.ColumnCount];

        for (int columnIndex = 0; columnIndex < Board.ColumnCount; columnIndex++)
        {
            if (Board.HasColumnFreeSlots(columnIndex))
            {
                int myCount = myColorCount[columnIndex];
                int otherPlayerId = GetOtherColorIndex(mapCounter1, columnIndex, myCount);

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

    private Dictionary<int, int[]> GetColorCount(int offSet)
    {
        Dictionary<int, int[]> mapCounter = new Dictionary<int, int[]>();

        for (int columnIndex = 0; columnIndex < Board.ColumnCount; columnIndex++)
        {
            int rowIndex = Board.NextSlotIndexInColumn(columnIndex) + offSet;

            if (rowIndex > -1 && rowIndex < Board.RowCount)
            {
                CountColors(columnIndex, rowIndex, ref mapCounter);
            }
        }

        return mapCounter;
    }

    private void CountColors(int currentColumnIndex, int currentRowIndex,
        ref Dictionary<int, int[]> mapCounter)
    {
        FourInARowSlot slot = Board.GetSlot(currentColumnIndex, currentRowIndex);

        if (null == slot || !slot.IsEmpty)
        {
            return;
        }

        int columnCount = Board.ColumnCount;
        int rowCount = Board.RowCount;
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

                    FourInARowSlot slot2 = Board.GetSlot(columnIndex, rowIndex);
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
