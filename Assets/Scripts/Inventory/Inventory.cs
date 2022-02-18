using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Action;
using TMPro;

/// <summary>
/// This class is the UI for the inventory.
/// It's also a singleton.
///
/// All collected objects of the current
/// player are displayed here. There is a
/// limited number of slots, which may
/// contain exactly one collectable in each.
/// 
/// It has a reference to the counter in the
/// action bar, which displays the number of
/// objects collected and the maximum number
/// of slots.
/// </summary>
public class Inventory : MonoBehaviour
{
    public static Inventory ins;

    public static Inventory GetInstance()
    {
        return ins;
    }

    private static Vector3[] positions = new Vector3[] { Vector3.one, Vector3.zero };

    public RectTransform rectTransform;
    public TextMeshProUGUI countDisplay;
    public InventorySlot[] slots;

    private bool isVisible = true;
    private IEnumerator ieScale;

    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
            ClearDisplay();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Is called when the size of the UI canvas
    /// is changed.
    /// </summary>
    public void UpdatePositions()
    {
        positions[0] = new Vector3(
            rectTransform.rect.xMin,
            rectTransform.rect.yMin,
            0f
        );
        positions[1] = new Vector3(
            positions[0].x,
            positions[0].y - rectTransform.rect.height,
            positions[0].z
        );
    }

    /// <summary>
    /// Reset the item counter to 0.
    /// </summary>
    public void ClearDisplay()
    {
        UpdateDisplay(0);
    }

    /// <summary>
    /// Update the item counter for a given player.
    /// </summary>
    /// <param name="player">the player to update</param>
    public void UpdateDisplay(Player player)
    {
        int itemCount = player.GetInventoryContentCount();
        bool hasCapacity = player.HasInventoryCapacity();
        UpdateDisplay(itemCount, hasCapacity);
    }

    /// <summary>
    /// Update the item counter.
    /// </summary>
    /// <param name="itemCount">item count</param>
    /// <param name="hasCapacity">true = slots free, false = no free slots</param>
    public void UpdateDisplay(int itemCount, bool hasCapacity)
    {
        UpdateDisplay(itemCount);
        countDisplay.color = hasCapacity ? Color.white : Color.red;
    }

    /// <summary>
    /// Update the item counter.
    /// </summary>
    /// <param name="itemCount">item count</param>
    public void UpdateDisplay(int itemCount)
    {
        string s = itemCount.ToString() + " / " + slots.Length;
        countDisplay.SetText(s);
        countDisplay.color = Color.white;
    }

    /// <summary>
    /// Clear the whole inventory and reset the item counter.
    /// </summary>
    public void Clear()
    {
        ClearDisplay();

        foreach (InventorySlot slot in slots)
            slot.RemoveObject();
    }

    /// <summary>
    /// Show or hide the inventory UI.
    /// </summary>
    /// <param name="visible">true = show, false = hide</param>
    /// <param name="instant">true = don't animate</param>
    public void Show(bool visible, bool instant = false)
    {
        if (isVisible == visible)
            return;

        isVisible = visible;
        Vector3 position = isVisible ? positions[0] : positions[1];
        Vector3 scale = (isVisible ? Vector3.one : Vector3.zero);

        if (null != ieScale)
            StopCoroutine(ieScale);

        ieScale = null;

        if (instant)
        {
            rectTransform.position = position;
            rectTransform.localScale = scale;

            if (isVisible)
                CheckSlots();
            else
                HideSlotObjects();
        }
        else
        {
            ieScale = IEScale(position, scale);
            StartCoroutine(ieScale);
        }
    }

    /// <summary>
    /// Animation to open or close the inventory UI.
    /// </summary>
    /// <param name="position">move position</param>
    /// <param name="scale">scale size</param>
    private IEnumerator IEScale(Vector3 position, Vector3 scale)
    {
        if (isVisible)
            CheckSlots();

        float f = 0f;

        while (f <= 1f)
        {
            rectTransform.position = Vector3.Lerp(rectTransform.position, position, f);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scale, f);
            yield return null;
            f += Time.deltaTime;
        }

        rectTransform.localScale = scale;
        rectTransform.position = position;
        ieScale = null;
    }

    /// <summary>
    /// Hide all slots.
    /// </summary>
    private void HideSlotObjects()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].HideObject();
    }

    /// <summary>
    /// Animation to move a collectable to the inventory.
    /// TODO fix it!!!
    /// Doesn't seem to work in current setup.
    /// </summary>
    /// <param name="collectable">collectable to add</param>
    private IEnumerator IEMove(Collectable collectable)
    {
        collectable.Restore();
        collectable.gameObject.SetActive(true);
        collectable.col.enabled = false;

        float f = 0f;
        Vector3 position = ActionController.GetInstance().inventorybox.GetWorldPosition();

        while (f <= 1f)
        {
            collectable.transform.position = Vector3.Lerp(collectable.transform.position, position, f);

            yield return null;

            f += Time.deltaTime;
        }

        collectable.col.enabled = true;
        collectable.transform.gameObject.layer = transform.gameObject.layer;
        collectable.transform.gameObject.SetActive(false);
        ActionController.GetInstance().inventorybox.Close(true);
    }

    /// <summary>
    /// Add another collectable to the inventory.
    /// </summary>
    public void Insert(Collectable collectable)
    {
        ActionController actionController = ActionController.GetInstance();

        if (actionController.IsVisible && !actionController.inventorybox.IsOpen)
        {
            actionController.inventorybox.Open(true);
            StartCoroutine(IEMove(collectable));
        }
        else
        {
            if (actionController.inventorybox.IsOpen)
                CheckSlots();

            collectable.col.enabled = true;
            collectable.transform.gameObject.layer = transform.gameObject.layer;
            collectable.transform.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Check the item count automatically.
    /// </summary>
    private void CheckSlots()
    {
        Player player = GameManager.GetInstance().CurrentPlayer;

        if (null == player)
            return;

        List<Collectable> content = player.GetInventoryItems();
        UpdateDisplay(0);

        if (null == content)
            return;

        int itemCount = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (null != content[i])
            {
                if (!slots[i].HasObject)
                {
                    SetObject(i, content[i]);
                    content[i].SetInventorySlot(slots[i]);
                }
                
                slots[i].ResetObjectInPosition();
                itemCount++;
            }
        }

        UpdateDisplay(itemCount, itemCount < slots.Length);
    }

    /// <summary>
    /// Set a collectable on a slot. 
    /// </summary>
    /// <param name="index">index of the slot</param>
    /// <param name="collectable">collectable to set</param>
    private void SetObject(int index, Collectable collectable)
    {
        Debug.Log("set object " + index + " " + collectable.transform.name);

        InventorySlot slot = slots[index];
        collectable.SetLayer((int)Layers.Invisible);

        slot.SetObject(collectable);
    }
}