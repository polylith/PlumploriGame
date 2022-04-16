using System;
using System.Collections.Generic;

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

    public override List<string> GetAttributes()
    {
        return new List<string>();
    }

    public override Dictionary<string, Action<bool>> GetDelegates()
    {
        return null;
    }

    public override List<Formula> GetGoals()
    {
        return null;
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
        optionsButton.SetAction(options.Show);
        newGameButton.SetAction(NewGame);

        options.Init(SetupOptions);
    }

    protected override void PreCall()
    {
        SetupOptions();
        options.Show();
    }

    public override void ResetApp()
    {
        board.ResetBoard();
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

        options.Hide();
        NewGame();
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

    private void ShowHint()
    {
        // TODO
    }

    private void NewGame()
    {
        coinCounter = 0;
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
        hintButton.IsEnabled = false;
        board.IsSelectionEnabled = false;
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
        bool isUser = players[CurrentPlayerId].IsUser;
        playerDisplays[CurrentPlayerId].IsCurrent = true;
        board.IsSelectionEnabled = isUser;
        hintButton.IsEnabled = isUser;

        if (isUser)
            return;

        GameEvent.GetInstance().Execute(HandleInput, UnityEngine.Random.Range(0.5f, 1f));
    }

    private void HandleInput()
    {
        int columnIndex = players[CurrentPlayerId].FindBestSlotColumnIndex();
        InsertCoin(columnIndex);
    }
}
