/// <summary>
/// Data structure to save and reload the current game state.
/// </summary>
public class GameState : AbstractData
{
    public string CurrentPlayerName { get => currentPlayerName; }
    public int CurrentRoomConfigIndex { get => currentRoomConfigIndex; }

    private string currentPlayerName;
    private int currentRoomConfigIndex;

    public GameState(string currentPlayerName, int currentRoomConfigIndex)
    {
        this.currentPlayerName = currentPlayerName;
        this.currentRoomConfigIndex = currentRoomConfigIndex;
    }

    public void Load()
    {
        
    }

    public void Save()
    {
        
    }
}
