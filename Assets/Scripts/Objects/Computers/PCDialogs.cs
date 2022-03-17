using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// <para>
/// This class handles the display of dialogs on the PC.
/// </para>
/// </summary>
public class PCDialogs : MonoBehaviour
{
    public RectTransform rectTransform;
    public Image backdrop;
    public PCDialog dialogPrefab;

    private readonly Dictionary<int, PCDialog> dict = new Dictionary<int, PCDialog>();
    private readonly List<int> blockingOwnerIds = new List<int>();

    private void OnDisable()
    {
        CloseAllDialogs();
    }

    public void CloseAllDialogs()
    {
        foreach (PCDialog pcDialog in dict.Values)
        {
            Destroy(pcDialog.gameObject);
        }

        dict.Clear();
        blockingOwnerIds.Clear();
        backdrop.enabled = false;
    }

    public bool ShowDialog(
        GameObject owner,
        bool isBlocking,
        PCDialog.DialogType dialogType,
        string messageText,
        PCDialog.ButtonLabels buttonLabels,
        List<System.Action> buttonActions,
        System.Action closeAction = null,
        string[] buttonTexts = null
    )
    {
        if (null == owner)
            return false;

        int ownerId = owner.GetInstanceID();
        PCDialog pcDialog;

        if (dict.ContainsKey(ownerId))
        {
            pcDialog = dict[ownerId];
        }
        else
        {
            pcDialog = Instantiate(dialogPrefab) as PCDialog;
            pcDialog.rectTransform.name = "PC Dialog " + ownerId.ToString();
            dict.Add(ownerId, pcDialog);
        }

        List<System.Action> actionsList = null;

        if (null != buttonActions && buttonActions.Count > 0)
        {
            actionsList = new List<System.Action>();

            foreach (System.Action action in buttonActions)
            {
                actionsList.Add(
                    () => {
                        action?.Invoke();
                        HideDialog(pcDialog);
                        closeAction?.Invoke();
                    }
                );
            }
        }

        pcDialog.SetOwnerId(ownerId);
        pcDialog.SetMessage(messageText);
        pcDialog.SetType(dialogType);
        pcDialog.SetButtonActions(
            buttonLabels,
            actionsList,
            buttonTexts
        );
        pcDialog.hideButton.gameObject.SetActive(false);
        pcDialog.closeButton.SetAction(() => {
            HideDialog(pcDialog);
            closeAction?.Invoke();
        });

        if (isBlocking)
        {
            backdrop.enabled = true;

            if (!blockingOwnerIds.Contains(ownerId))
                blockingOwnerIds.Add(ownerId);
        }
        // TODO set position more dynamically
        Vector3 position = pcDialog.rectTransform.localPosition;
        pcDialog.rectTransform.SetParent(rectTransform);
        pcDialog.rectTransform.SetAsLastSibling();
        pcDialog.rectTransform.localPosition = position;
        pcDialog.Show();
        return true;
    }

    private void HideDialog(PCDialog pcDialog)
    {
        if (!pcDialog.IsVisible)
            return;

        int ownerId = pcDialog.OwnerId;

        if (blockingOwnerIds.Contains(ownerId))
            blockingOwnerIds.Remove(ownerId);

        pcDialog.Hide();
        backdrop.enabled = blockingOwnerIds.Count > 0;
    }
}
