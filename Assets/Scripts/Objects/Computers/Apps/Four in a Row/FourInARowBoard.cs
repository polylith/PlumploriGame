using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class FourInARowBoard : MonoBehaviour
{
    /// Directions:
    /// 0   
    /// 1 X 
    /// 2 3 
    public readonly int[][] Directions = new int[][] {
        new int[] { -1,  1 }, // 0
        new int[] { -1,  0 }, // 1
        new int[] { -1, -1 }, // 2
        new int[] {  0, -1 }, // 3
    };

    public int InsertCount { get; private set; }
    public bool HasFreeSlots { get => CheckFreeSlots(); }
    public int NextFreeColumnIndex { get => GetNextFreeColumnIndex(); }
    public bool IsSelectionEnabled { get => isSelectionEnabled; set => SetSelectionEnabled(value); }
    public int Score { get => winningSlots.Count; }
    public int ColumnCount { get => fourInARowColumns.Length; }
    public int RowCount { get => fourInARowColumns[0].RowCount; }

    public FourInARowColumnSelect[] fourInARowColumnSelects;
    public FourInARowColumn[] fourInARowColumns;
    public RectTransform coinsParent;
    public int slotSize = 128;

    private bool isSelectionEnabled;
    private readonly List<FourInARowSlot> winningSlots = new List<FourInARowSlot>();

    public int NextSlotIndexInColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= fourInARowColumns.Length)
            return -1;

        return fourInARowColumns[columnIndex].NextSlotIndex;
    }

    public void SetInsertDelegate(FourInARowColumnSelect.OnColumnSelectEvent InsertDelegate)
    {
        for (int i = 0; i < fourInARowColumnSelects.Length; i++)
        {
            fourInARowColumnSelects[i].OnColumnSelect += InsertDelegate;
        }
    }

    public bool HasColumnFreeSlots(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= ColumnCount)
            return false;

        return fourInARowColumns[columnIndex].HasFreeSlots;
    }

    public void ResetBoard()
    {
        for (int i = 0; i < fourInARowColumns.Length; i++)
        {
            fourInARowColumns[i].ResetColumn();
        }

        InsertCount = 0;
    }

    public FourInARowSlot GetSlot(int columnIndex, int rowIndex)
    {
        if (columnIndex < 0 || columnIndex >= ColumnCount
            || rowIndex < 0 || rowIndex >= RowCount)
            return null;

        return fourInARowColumns[columnIndex].GetSlot(rowIndex);
    }

    public bool InsertCoin(int columnIndex, FourInARowCoin coin, int insertCount)
    {
        InsertCount = insertCount;
        FourInARowColumn column = fourInARowColumns[columnIndex];

        if (column.InsertCoin(coin))
        {
            float halfHeight = 0.5f * fourInARowColumns.Length * slotSize;
            float halfSlotSize = 0.5f * slotSize;
            int d = column.slots.Length + 2 - column.NextSlotIndex;
            float x = (columnIndex - 0.5f * fourInARowColumns.Length) * slotSize;
            float y2 = (column.slots.Length + 5) * slotSize;
            float y1 = (column.NextSlotIndex + 1) * slotSize;
            y1 -= halfHeight;
            y2 -= halfHeight;
            y1 -= halfSlotSize;
            x += halfSlotSize;
            float duration = d * 0.1f;
            coin.transform.SetParent(coinsParent, false);
            coin.transform.localPosition = new Vector3(x, y2, 0f);
            coin.transform.localScale = Vector3.zero;
            Vector3 rot = new Vector3(0f, 0f, Random.Range(180f, 360f) + 360f);
            
            DOTween.Sequence()
                .SetAutoKill(true)
                .Append(
                    coin.transform.DOScale(Vector3.one, 0.125f)
                    .SetEase(Ease.Linear)
                )
                .Append(
                    coin.transform.DOLocalMove(new Vector3(x, y1, 0f), duration)
                    .SetEase(Ease.Linear)
                )
                .Append(
                    coin.transform.DOLocalRotate(rot, duration * 0.9f)
                    .SetEase(Ease.Linear)
                )
                .Play();

            return true;
        }

        return false;
    }

    public void HighlightWinningSlots()
    {
        foreach (FourInARowSlot slot in winningSlots)
        {
            slot.Highlight();
        }
    }

    private void ClearWinningSlots()
    {
        winningSlots.Clear();
    }

    private void AddWinningSlots(List<FourInARowSlot> list)
    {
        foreach (FourInARowSlot slot in list)
        {
            if (!winningSlots.Find((s) => s.gameObject.GetInstanceID() == slot.gameObject.GetInstanceID()))
            {
                winningSlots.Add(slot);
            }
        }
    }

    public bool CheckWin(int currentColumnIndex)
    {
        ClearWinningSlots();
        int columnCount = fourInARowColumns.Length;
        FourInARowColumn column = fourInARowColumns[currentColumnIndex];
        int rowCount = column.slots.Length;
        int currentRowIndex = column.NextSlotIndex - 1;
        int currentColorIndex = column.GetColorIndex(currentRowIndex);
        int maxConnectedCount = 0;

        for (int index = 0; index < Directions.Length; index++)
        {
            List<FourInARowSlot> list = new List<FourInARowSlot>();
            int connectedCount = 0;
            int columnIndex = currentColumnIndex;
            int rowIndex = currentRowIndex;
            int deltaColumnIndex = Directions[index][0];
            int deltaRowIndex = Directions[index][1];

            for (int direction = 0; direction < 2; direction++)
            {
                while (true)
                {
                    if (columnIndex < 0 || columnIndex >= columnCount
                        || rowIndex < 0 || rowIndex >= rowCount)
                        break;

                    FourInARowSlot slot = fourInARowColumns[columnIndex].GetSlot(rowIndex);
                    int colorIndex = null != slot ? slot.PlayerId : -1;

                    if (colorIndex != currentColorIndex)
                    {
                        break;
                    }
                    else
                    {
                        list.Add(slot);
                        connectedCount++;
                        columnIndex += deltaColumnIndex;
                        rowIndex += deltaRowIndex;
                    }
                }

                deltaColumnIndex *= -1;
                deltaRowIndex *= -1;
                columnIndex = currentColumnIndex;
                rowIndex = currentRowIndex;
                columnIndex += deltaColumnIndex;
                rowIndex += deltaRowIndex;
            }

            if (list.Count > 3)
            {
                AddWinningSlots(list);
            }

            maxConnectedCount = Mathf.Max(maxConnectedCount, connectedCount);
        }

        return maxConnectedCount > 3;
    }

    private int GetNextFreeColumnIndex()
    {
        for (int index = 0; index < fourInARowColumns.Length; index++)
        {
            if (fourInARowColumns[index].HasFreeSlots)
                return index;
        }

        return -1;
    }

    private bool CheckFreeSlots()
    {
        for (int index = 0; index < fourInARowColumns.Length; index++)
        {
            if (fourInARowColumns[index].HasFreeSlots)
                return true;
        }

        return false;
    }

    private void SetSelectionEnabled(bool isSelectionEnabled)
    {
        if (this.isSelectionEnabled == isSelectionEnabled)
            return;

        this.isSelectionEnabled = isSelectionEnabled;

        for (int index = 0; index < fourInARowColumnSelects.Length; index++)
        {
            FourInARowColumnSelect select = fourInARowColumnSelects[index];
            FourInARowColumn column = fourInARowColumns[index];
            select.IsEnabled = isSelectionEnabled && column.HasFreeSlots;
        }
    }

}
