using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TicTacToeApp : PCApp
{
    private static int[][] checkData = new int[][]
    {
        new int[] { 0, 1, 2},
        new int[] { 3, 4, 5},
        new int[] { 6, 7, 8},

        new int[] { 0, 3, 6},
        new int[] { 1, 4, 7},
        new int[] { 2, 5, 8},

        new int[] { 0, 4, 8},
        new int[] { 2, 4, 6}
    };

    public Sprite[] sprites;
    public TicTacToeField[] fields;
    public Transform display;
    public TextMeshProUGUI textMesh;

    private int currentPlayer = -1;
    private int winPlayer;
    private int count;
    private int[] counter = new int[] { 0, 0, 0, 0 };

    private void Awake()
    {
        Init();
    }

    protected override void Effect()
    {
        // nothing to do here
    }

    public override void ResetApp()
    {
        counter = new int[] { 0, 0, 0, 0 };
        winPlayer = UnityEngine.Random.Range(0, 100) % 2;
        currentPlayer = -1;
        ResetFields();
        InitUI();
    }

    private void InitUI()
    {
        ShowText(Language.LanguageManager.GetText(Language.LangKey.NewGame), "#ffff00");
    }

    protected override void Init()
    {
        foreach (TicTacToeField field in fields)
            field.onClick += FieldClick;

        currentPlayer = -1;
        winPlayer = UnityEngine.Random.Range(0, 100) % 2;
        base.Init();
        InitUI();
    }

    private void ResetFields()
    {
        display.gameObject.SetActive(false);
        count = fields.Length;

        TicTacToeField.ShuffleColors();

        foreach (TicTacToeField field in fields)
            field.SetState(-1);
    }

    public void NewGame()
    {
        sprites = ArrayHelper.Shuffle<Sprite>(sprites);
        counter[3] = 0;
        ResetFields();
        currentPlayer = (winPlayer + 1) % 2;

        if (currentPlayer == 0)
            return;

        SetNextField();
    }

    public override List<string> GetAttributes()
    {
        string[] attributes = new string[]
        {
            "TicTacToeApp.IsEnabled",
            "TicTacToeApp.HasWon"
        };

        List<string> list = new List<string>();

        foreach (string attribute in attributes)
            list.Add(attribute);

        return list;
    }

    public override Dictionary<string, Action<bool>> GetDelegates()
    {
        Dictionary<string, System.Action<bool>> dict = new Dictionary<string, System.Action<bool>>();
        dict.Add("TicTacToeApp.IsEnabled", SetEnabled);
        return dict;
    }

    public override List<Formula> GetGoals()
    {
        List<Formula> list = new List<Formula>();
        list.Add(new Implication(null, WorldDB.Get("TicTacToeApp.IsEnabled")));
        list.Add(new Implication(WorldDB.Get("TicTacToeApp.HasWon"), null));
        return list;
    }

    private void FieldClick(int id)
    {
        if (currentPlayer != 0)
            return;

        PlaceField(id);
    }

    private void PlaceField(int id)
    {
        if (isInfected)
        {
            id = UnityEngine.Random.Range(0, 9);
        }

        AudioManager.GetInstance().PlaySound("wush", computer.gameObject, currentPlayer == 0 ? 1.125f : 0.875f);
        fields[id].SetState(currentPlayer, sprites[currentPlayer]);
        count--;
        CheckWin();
    }

    private void CheckWin()
    {
        bool bFinish = CheckFields();
        counter[3]++;

        if (bFinish)
        {
            bool b = currentPlayer == 0;
            counter[currentPlayer]++;
            counter[2]++;
            computer?.AppFire("TicTacToeApp.HasWon", b);
            string text = Language.LanguageManager.GetText(b ? Language.LangKey.YouWin : Language.LangKey.YouLose) + "<br>" + GetScoreString(); 
            ShowText(text, b ? "#00ff00" : "#ff0000", 1f);
            AudioManager.GetInstance().PlaySound(b ? "win" : "lose", computer.gameObject);
            winPlayer = currentPlayer;
            currentPlayer = -1;            
            return;
        }

        if (count == 0)
        {
            winPlayer = UnityEngine.Random.Range(0,100) % 2;
            currentPlayer = -1;
            string text = Language.LanguageManager.GetText(Language.LangKey.Drawn) + "<br>" + GetScoreString();
            ShowText(text, "#555555", 1f);
            AudioManager.GetInstance().PlaySound("drawn", computer.gameObject);
            return;
        }

        currentPlayer++;
        currentPlayer %= 2;

        if (currentPlayer == 0)
            return;

        SetNextField();
    }

    private string GetScoreString()
    {
        return counter[0].ToString() + " : " + counter[1].ToString();
    }

    private void SetNextField()
    {
        int id = 0;

        if (counter[3] > 0)
            id = FindBestField();
        else
            id = GetRandomField();

        GameEvent.GetInstance().Execute<int>(PlaceField, id, 0.5f);
    }

    private int GetRandomField()
    {
        int id = UnityEngine.Random.Range(0, 100) % fields.Length;

        while (fields[id].State > -1)
            id = UnityEngine.Random.Range(0, 100) % fields.Length;

        return id;
    }

    private int FindBestField()
    {
        int[] arr = new int[fields.Length];

        for (int i = 0; i < checkData.Length; i++)
        {
            int j0 = checkData[i][0];
            int j1 = checkData[i][1];
            int j2 = checkData[i][2];

            if (fields[j0].State == -1)
                arr[j0] += (fields[j1].State > -1 && fields[j2].State > -1) 
                    && fields[j1].State == fields[j2].State ? (fields[j2].State == 0 ? 8 : 16) 
                    : (fields[j1].State > -1 || fields[j2].State > -1 ? 2 : 1);

            if (fields[j1].State == -1)
                arr[j1] += (fields[j0].State > -1 && fields[j2].State > -1) 
                    && fields[j0].State == fields[j2].State ? (fields[j2].State == 0 ? 8 : 16) 
                    : (fields[j0].State > -1 || fields[j2].State > -1 ? 2 : 1);

            if (fields[j2].State == -1)
                arr[j2] += (fields[j1].State > -1 && fields[j0].State > -1) 
                    && fields[j1].State == fields[j0].State ? (fields[j0].State == 0 ? 8 : 16) 
                    : (fields[j1].State > -1 || fields[j0].State > -1 ? 2 : 1);
        }

        List<int> list = new List<int>();
        int max = int.MinValue;

        for (int i = 0; i < arr.Length; i++)
            if (arr[i] >= max && fields[i].State == -1)
                max = arr[i];

        for (int i = 0; i < arr.Length; i++)
            if (arr[i] >= max && fields[i].State == -1)
                list.Add(i);

        if (list.Count == 0)
            return GetRandomField();

        int k = 0;

        if (list.Count > 1)
            k = UnityEngine.Random.Range(0, 100) % list.Count;

        return list[k];
    }

    private bool CheckFields()
    {
        for (int i = 0; i < checkData.Length; i++)
        {
            if (fields[checkData[i][0]].State > -1 
                && fields[checkData[i][0]].State == fields[checkData[i][1]].State 
                && fields[checkData[i][0]].State == fields[checkData[i][2]].State)
            {
                fields[checkData[i][0]].Highlight();
                fields[checkData[i][1]].Highlight();
                fields[checkData[i][2]].Highlight();
                return true;
            }
        }

        return false;
    }

    private void ShowText(string s, string scolor, float delay = 0f)
    {
        string text = "<color=" + scolor + ">" + s + "</color>"; 
        textMesh.SetText(text);

        if (delay <= 0f)
            ShowText(false);
        else
            GameEvent.GetInstance().Execute<bool>(ShowText, true, delay);        
    }

    private void ShowText(bool animate)
    {
        if (!animate)
        {
            display.gameObject.SetActive(true);
            return;
        }

        AudioManager.GetInstance().PlaySound("max", computer.gameObject);
        display.localScale = Vector3.zero;
        display.gameObject.SetActive(true);
        display.DOScale(Vector3.one, 0.5f);
    }
}