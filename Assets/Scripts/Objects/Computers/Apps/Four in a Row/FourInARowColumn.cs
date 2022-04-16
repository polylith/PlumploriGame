using UnityEngine;

public class FourInARowColumn : MonoBehaviour
{
    public FourInARowSlot[] slots;

    public int RowCount { get => slots.Length; }
    public bool HasFreeSlots { get => NextSlotIndex < slots.Length; }
    public int NextSlotIndex { get => nextSlotIndex; }

    private int nextSlotIndex;

    public void ResetColumn()
    {
        int i = 0;
        nextSlotIndex = 0;

        while (i < slots.Length)
        {
            if (slots[i].IsEmpty)
                break;

            slots[i].ResetSlot();
            i++;
        }
    }

    public bool InsertCoin(FourInARowCoin coin)
    {
        if (HasFreeSlots)
        {
            slots[nextSlotIndex].SetCoin(coin);
            nextSlotIndex++;
            return true;
        }

        return false;
    }

    public FourInARowSlot GetSlot(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= RowCount)
            return null;

        return slots[rowIndex];
    }

    public int GetColorIndex(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= RowCount)
            return -1;

        return slots[rowIndex].PlayerId;
    }
}
