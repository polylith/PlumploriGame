using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourInARowApp : PCApp
{
    public int CurrentPlayerId { get; private set; }
    public int WinnerPlayerId { get; private set; }

    public FourInARowOptions options;

    public UIIconButton hintButton;
    public UIIconButton newGameButton;
    public UIIconButton optionsButton;

    public FourInARowPlayerDisplay[] playerDisplays;

    public FourInARowCoin coinPrefab;
    public FourInARowBoard board;

    private List<FourInARowStrategy> players;
    private int coinCounter;
    private IEnumerator ieInput;
    private bool isGameRunning;
    private bool isGamePaused;
    private bool isGameFinished;
    private int hintCount;

    public override List<string> GetAttributes()
    {
        return new List<string>()
        {
            "FourInARowApp.HasWon"
        };
    }

    public override Dictionary<string, Action<bool>> GetDelegates()
    {
        return null;
    }

    public override List<Formula> GetGoals()
    {
        return new List<Formula>()
        {
            new Implication(WorldDB.Get("FourInARowApp.HasWon"), null)
        };
    }

    protected override void Effect()
    {
        if (!IsInfected || !IsActive)
            return;


        if (board.HasFreeSlots)
        {
            int columnIndex = UnityEngine.Random.Range(0, board.ColumnCount);

            while (board.HasFreeSlots && !board.HasColumnFreeSlots(columnIndex))
                columnIndex = UnityEngine.Random.Range(0, board.ColumnCount);

            if (board.HasFreeSlots)
                InsertCoin(columnIndex);

            FourInARowCoin.ShuffleColors();
        }
        
        GameEvent.GetInstance().Execute(Effect, UnityEngine.Random.Range(5f, 25f));
    }

    public override void SetInfected(bool isInfected)
    {
        if (this.isInfected == isInfected)
            return;

        this.isInfected = isInfected;

        if (!isInfected)
            return;

        GameEvent.GetInstance().Execute(Effect, UnityEngine.Random.Range(10f, 25f));
    }

    protected override void Init()
    {
        base.Init();
        board.SetInsertDelegate(InsertCoin);

        hintButton.SetAction(ShowHint);
        hintButton.IsEnabled = false;
        optionsButton.SetAction(ShowOptions);
        newGameButton.SetAction(NewGame);

        options.Init(ApplyOptions, ResumeGame);
    }

    protected override void PreCall()
    {
        isGameRunning = false;
        isGamePaused = false;
        SetupOptions();
        options.Show();
    }

    public override void ResetApp()
    {
        isGameRunning = false;
        isGamePaused = false;
        StopInput();
        board.ResetBoard();
    }

    private void ShowOptions()
    {
        isGamePaused = true;
        StopInput();
        options.Show(true);
    }

    private void ResumeGame()
    {
        isGamePaused = false;

        if (isGameFinished)
            return;

        SwitchPlayer();
    }

    private void ApplyOptions()
    {
        isGameRunning = true;
        isGamePaused = false;
        SetupOptions();
    }

    private void SetupOptions()
    {
        ResetPlayers();
        FourInARowSettings settings = options.Settings;
        int numberOfPlayers = settings.numberOfPlayers;
        int[] playerModes = new int[] {
            settings.playerMode0,
            settings.playerMode1,
            settings.playerMode2,
            settings.playerMode3
        };
        players = new List<FourInARowStrategy>();

        for (int i = 0; i < playerDisplays.Length; i++)
        {
            int playerId = i < numberOfPlayers ? i : -1;
            playerDisplays[i].PlayerId = playerId;

            if (playerId > -1)
            {
                players.Add(
                    FourInARowStrategy.GetStrategy(
                        playerModes[i],
                        board,
                        playerId
                    )
                );
            }
        }

        board.MaxPlayers = players.Count;
        options.Hide();
        StartGame();
    }

    private void ResetPlayers()
    {
        FourInARowCoin.ShuffleColors();

        foreach (FourInARowPlayerDisplay playerDisplay in playerDisplays)
        {
            playerDisplay.PlayerId = -1;
        }

        WinnerPlayerId = -1;
        CurrentPlayerId = -1;
    }

    private void StopInput()
    {
        if (null != ieInput)
            StopCoroutine(ieInput);

        ieInput = null;
    }

    private void ShowHint()
    {
        hintCount++;
        int columnIndex1 = FourInARowProfiStrategy.FindBest(board, CurrentPlayerId);
        int rowIndex = board.NextSlotIndexInColumn(columnIndex1);
        List<int[]> list = new List<int[]>() { new int[] { columnIndex1, rowIndex } };
        int columnIndex2 = FourInARowMasterStrategy.FindBest(board, CurrentPlayerId);

        if (columnIndex2 != columnIndex1)
        {
            rowIndex = board.NextSlotIndexInColumn(columnIndex2);
            list.Add(new int[] { columnIndex2, rowIndex });            
        }

        int columnIndex3 = FourInARowLazyStrategy.FindBest(board, CurrentPlayerId);

        if (columnIndex3 != columnIndex1 && columnIndex3 != columnIndex2)
        {
            rowIndex = board.NextSlotIndexInColumn(columnIndex3);
            list.Add(new int[] { columnIndex3, rowIndex });
        }

        board.HighlightSlots(list, CurrentPlayerId);
    }

    private void NewGame()
    {
        isGameRunning = true;
        isGamePaused = false;
        StartGame();
    }

    private void StartGame()
    {
        coinCounter = 0;
        isGameFinished = false;
        hintCount = 0;
        StopInput();
        board.ResetBoard();

        /*
         * winner is next player to start,
         * else next last player if there 
         * is no winner.
         */
        if (WinnerPlayerId > -1)
        {
            CurrentPlayerId = WinnerPlayerId;
        }
        else
        {
            CurrentPlayerId++;
            CurrentPlayerId %= players.Count;
        }

        /*
         * player id has to be decreased by 1,
         * because NextPlayer() adds 1.
         */
        CurrentPlayerId--;
        NextPlayer();
    }

    private void InsertCoin(int columnIndex)
    {
        board.UnhighlightSlots();
        hintButton.IsEnabled = false;
        board.IsSelectionEnabled = false;

        if (!isGameRunning || isGamePaused || isGameFinished)
            return;

        FourInARowCoin coin = coinPrefab.Instantiate(CurrentPlayerId, coinCounter);

        if (!board.InsertCoin(columnIndex, coin, coinCounter + 1))
        {
            return;
        }

        AudioManager.GetInstance().PlaySound("coin.insert", computer.gameObject);

        coinCounter++;
        System.Action action = NextPlayer;

        if (board.CheckWin(columnIndex))
        {
            WinnerPlayerId = CurrentPlayerId;
            action = ShowWinner;
            isGameFinished = true;
        }
        else if (!board.HasFreeSlots)
        {
            WinnerPlayerId = -1;
            action = ShowWinner;
        }

        GameEvent.GetInstance().Execute(action, 0.5f);
    }

    private void ShowWinner()
    {
        if (WinnerPlayerId > -1)
        {
            board.HighlightWinningSlots();

            if (hintCount == 0 && players[WinnerPlayerId].IsUser)
            {
                computer.AppFire("FourInARowApp.HasWon", true);
            }

            playerDisplays[WinnerPlayerId].AddScore(board.Score);
            AudioManager.GetInstance().PlaySound("win", computer.gameObject);
            return;
        }

        AudioManager.GetInstance().PlaySound("drawn", computer.gameObject);
    }

    private void NextPlayer()
    {
        if (CurrentPlayerId > -1)
        {
            playerDisplays[CurrentPlayerId].IsCurrent = false;
        }

        CurrentPlayerId++;
        CurrentPlayerId %= players.Count;
        SwitchPlayer();
    }

    private void SwitchPlayer()
    {
        bool isUser = players[CurrentPlayerId].IsUser;
        playerDisplays[CurrentPlayerId].IsCurrent = true;
        board.IsSelectionEnabled = isUser;
        hintButton.IsEnabled = isUser;

        if (isUser)
            return;

        ieInput = IEHandleInput();
        StartCoroutine(ieInput);
    }

    private IEnumerator IEHandleInput()
    {
        int columnIndex = players[CurrentPlayerId].FindBestSlotColumnIndex();

        float waitTime = UnityEngine.Random.Range(0.5f, 1f);

        yield return new WaitForSecondsRealtime(waitTime);

        InsertCoin(columnIndex);

        yield return null;

        ieInput = null;
    }
}
