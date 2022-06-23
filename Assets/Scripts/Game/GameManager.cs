using System.Collections;
using System.Collections.Generic;
using Action;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The GameMananger is the main class responsible for
/// controlling the game flow: executing the changes of scenes,
/// creating the characters and calling the UI.
/// This class is available as a singleton so that the single
/// main instance can be called without any additional references.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    /// <summary>
    /// Static Function to get the singleton access
    /// </summary>
    /// <returns>Single instance of the GameManager</returns>
    public static GameManager GetInstance()
    {
        return instance;
    }

    public GameState CurrentGameState { get; private set; }
    public Player CurrentPlayer { get; private set; }
    public Room CurrentRoom { get => CurrentRoomConfig.Room; }
    public RoomConfig CurrentRoomConfig { get; private set; }

    public Material defaultSkyboxMaterial;
    public Transform roomConfigsParent;

    public UnityOutlineFX outlinePostEffect;
    
    private RoomConfig[] roomConfigs;
    private Interactable currentHighlighted;
    private readonly Dictionary<int, List<Renderer>> rendererMap = new Dictionary<int, List<Renderer>>();
    
    /// <summary>
    /// All other singletons are instantiated in Awake.
    /// The GameManager must be instantiated shortly after
    /// so that it can access them.
    /// </summary>
    private void Start()
    {
        if (null == instance)
        {
            instance = this;
            InitEntitySets();
            InitPlayers();
            InitRoomConfigs();
            InitWorld();

            GameEvent.GetInstance().Execute(StartGame,2f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void StartGame()
    {
        GameState startState = new GameState(
                "Plumplori",
                0
            );
        SetGameState(startState);
    }

    /// <summary>
    /// Apply a given game state
    /// </summary>
    /// <param name="gameState">game state to apply</param>
    private void SetGameState(GameState gameState)
    {
        CurrentGameState = gameState;
        string currentPlayName = gameState.CurrentPlayerName;
        int currentRoomConfigIndex = gameState.CurrentRoomConfigIndex;
        CurrentRoomConfig = roomConfigs[currentRoomConfigIndex];
        Player player = CharacterSet.GetInstance().GetPlayer(currentPlayName);
        SwitchCurrentPlayer(player);

        LoadRoom(CurrentRoomConfig);
    }

    /// <summary>
    /// Read all room configurations
    /// </summary>
    private void InitRoomConfigs()
    {
        roomConfigs = roomConfigsParent.GetComponentsInChildren<RoomConfig>();
    }

    /// <summary>
    /// Initlialize the world in logical terms.
    /// </summary>
    private void InitWorld()
    {
        WorldDB.InitDB();
        // TODO build formulas and goals
    }

    /// <summary>
    /// Initialize all entity sets
    /// </summary>
    private void InitEntitySets()
    {
        CollectableSet.GetInstance().Init();
    }

    /// <summary>
    /// Initialize all player prefabs
    /// </summary>
    private void InitPlayers()
    {
        CharacterSet.GetInstance().InitCharacters();
    }

    /// <summary>
    /// Initialize a given room configuration. That means:
    /// Apply the specific skybox, reset the state
    /// of each interactable and especially add all collectables
    /// that were dropped in this scene.
    /// Therefore the room must be loaded, so this method must be
    /// called in OnRoomStart.
    /// </summary>
    /// <param name="roomConfig">the room configuration to restore</param>
    private void InitRoomConfiguration(RoomConfig roomConfig)
    {
        Material skyBoxMaterial = roomConfig.skyBoxMaterial;

        if (null == skyBoxMaterial)
            skyBoxMaterial = defaultSkyboxMaterial;

        if (skyBoxMaterial.Equals(RenderSettings.skybox))
        {
            Debug.Log("skybox " + skyBoxMaterial.name + " didn't change");
        }
        else
        {
            RenderSettings.skybox = skyBoxMaterial;
            DynamicGI.UpdateEnvironment();
        }

        // TODO restore interactable
    }

    /// <summary>
    /// Set a given player as the current player
    /// </summary>
    /// <param name="player">player to switch to active</param>
    public void SwitchCurrentPlayer(Player player)
    {
        if (player == CurrentPlayer)
            return;

        // TODO if currentplayer is not null
        CurrentPlayer = player;
        CurrentPlayer.transform.SetParent(transform.parent);
        AudioManager.GetInstance().SwitchAudioListener(CurrentPlayer.audioListener);
    }

    /// <summary>
    /// Gets a specific player character by unique name.
    /// </summary>
    /// <param name="name">the unique name of the character</param>
    /// <returns>the player</returns>
    public Player GetPlayer(string name)
    {
        return CharacterSet.GetInstance().GetPlayer(name);
    }

    /// <summary>
    /// This method is called to make the current player
    /// go to a specific position in the current scene/room.
    /// </summary>
    /// <param name="position">position to go</param>
    public void GotoPosition(Vector3 position)
    {
        // no active player
        if (null == CurrentPlayer)
        {
            AudioManager.GetInstance().PlaySound("action.denied");
            return;
        }

        CurrentPlayer.Goto(position, position);
    }

    /// <summary>
    /// This method is called to make the current player go to
    /// a specific interactable object in the current scene/room.
    /// </summary>
    /// <param name="interactable">reference to the interactable object</param>
    public void GotoPosition(Interactable interactable)
    {
        // no active player
        if (null == CurrentPlayer)
        {
            AudioManager.GetInstance().PlaySound("action.denied");
            return;
        }

        Vector3 position = interactable.GetInteractionPosition();
        position = Calc.GetPointOnGround(position).point;
        CurrentPlayer.Goto(position, interactable.transform.position);
    }

    /// <summary>
    /// This method is called to let the current player interact
    /// with an object according to the currently active action.
    /// </summary>
    /// <param name="interactable">reference to the interactable object</param>
    public void Interact(Interactable interactable)
    {
        Vector3 position = interactable.GetInteractionPosition();
        Vector3 lookAt = interactable.GetLookAtPosition();

        if (interactable is Collectable collectable)
        {
            if (!ActionController.GetInstance().IsDropActionActive())
            {
                if (collectable.IsCollected)
                {
                    ActionController.GetInstance().ApplyActionState();
                    return;
                }
                else
                {
                    ObjectPlace objectPlace = UIDropPoint.GetInstance().ObjectPlace;

                    if (null != objectPlace)
                    {
                        position = objectPlace.GetWalkPosition(collectable);
                        lookAt = objectPlace.GetLookAtPosition();
                    }
                }
            }
        }

        /* 
         * Just go to the next interaction position and 
         * look at the interactable object.
         * no callback needed.
         */
        GotoAndInteract(
            position,
            lookAt
        );
    }

    /// <summary>
    /// This method is called to move the current player to a
    /// specific position in the current scene/room.
    /// The character should be facing a given position at the
    /// end and a callback method might be invoked.
    /// </summary>
    /// <param name="position">position to go</param>
    /// <param name="lookAt">position to look at</param>
    /// <param name="callBack">method to be invoked when character is in place</param>
    public void GotoAndInteract(Vector3 position, Vector3 lookAt, System.Action callBack = null)
    {
        // no active player
        if (null == CurrentPlayer)
        {
            AudioManager.GetInstance().PlaySound("action.denied");
            return;
        }

        /*
         * save the current action state at the time before 
         * the character is sent to the position, because this 
         * navigation might need a while and the action state 
         * may change by further mouse movements.
         */
        ActionState actionState = ActionController.GetInstance().GetCurrentActionState();
        position = Calc.GetPointOnGround(position).point;

        CurrentPlayer.Goto(
            position,
            lookAt,
            () =>
            {
                // restore the current action state after the character is in place
                ActionController.GetInstance().RestoreActionState(actionState);

                /* 
                 * if a callback method is given, it will be invoked,
                 * otherwise the ActionController will apply the current 
                 * state of the selected action
                 */
                if (null != callBack)
                {
                    callBack.Invoke();
                }
                else
                {
                    ActionController.GetInstance().ApplyActionState();
                }
            }
        );
    }

    /// <summary>
    /// Loads a room from the room configuration.
    /// </summary>
    /// <param name="roomConfig">the room configuration to load</param>
    public void LoadRoom(RoomConfig roomConfig)
    {
        StartCoroutine(IELoad(roomConfig));
    }

    /// <summary>
    /// Coroutine to load a scene asynchronous
    /// </summary>
    /// <param name="roomConfig">the room configuration to load</param>
    private IEnumerator IELoad(RoomConfig roomConfig)
    {
        UIGame uiGame = UIGame.GetInstance();
        uiGame.SetUIExclusive(gameObject, true);
        uiGame.SetCursorVisible(false);
        uiGame.ShowShade();

        yield return new WaitForSecondsRealtime(1f);

        UIProgress uiProgress = UIProgress.GetInstance();
        uiProgress.Run(true);

        yield return new WaitForSecondsRealtime(1f);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(
            roomConfig.sceneIndex,
            LoadSceneMode.Additive
        );
        asyncOperation.allowSceneActivation = true;
        asyncOperation.priority = 5;

        while (!asyncOperation.isDone)
        {
            uiProgress.UpdateProgressBar(asyncOperation.progress);

            yield return null;
        }
    }

    /// <summary>
    /// Callback for loaded room when scene is ready.
    /// </summary>
    /// <param name="room">the loaded room</param>
    public void OnRoomStart(Room room)
    {
        CameraFollowTarget cameraFollowTarget = CameraFollowTarget.GetInstance();
        CurrentRoomConfig.Room = room;
        room.sceneTestCamera?.gameObject.SetActive(false);
        InitRoomConfiguration(CurrentRoomConfig);
        room.InitCharacter(CurrentPlayer);
        cameraFollowTarget.SetPosition(room.cameraPosition);
        cameraFollowTarget.SetTarget(CurrentPlayer.transform);
        cameraFollowTarget.SetActive(true);
        
        StartCoroutine(IEFinishLoad());
    }

    private IEnumerator IEFinishLoad()
    {
        yield return new WaitForSecondsRealtime(1f);

        UIGame uiGame = UIGame.GetInstance();
        UIProgress uiProgress = UIProgress.GetInstance();
        uiProgress.ProgressReady();

        yield return new WaitForSecondsRealtime(1.5f);

        uiProgress.Stop();

        yield return new WaitForSecondsRealtime(1f);

        uiGame.HideShade();

        yield return new WaitForSecondsRealtime(1f);

        uiGame.SetCursorVisible(true);

        yield return new WaitForSecondsRealtime(1f);

        uiGame.SetUIExclusive(gameObject, false);
    }

    /// <summary>
    /// This method is called when the current player character
    /// is about to leave the current room using a door, so that
    /// the current room can be cleaned up.
    /// </summary>
    /// <param name="door">the door that was passed</param>
    public void OnBeforeRoomLeave(Door door)
    {
        // close all visible interactable uis
        InteractableUI.CloseActiveUIs();
    }

    /// <summary>
    /// This method is called when the current player character
    /// leaves the current room using a door, so that the next
    /// room can be loaded if one exists.
    /// </summary>
    /// <param name="door">the door that was passed</param>
    public void OnRoomLeft(Door door)
    {
        // TODO room logic
    }

    /// <summary>
    /// Removes the highlighting of the current object
    /// </summary>
    public void UnHighlight()
    {
        if (null != currentHighlighted)
        {
            List<Renderer> rendererList = CheckRenderers(currentHighlighted);
            currentHighlighted.IsHighlighted = false;
            outlinePostEffect.RemoveRenderers(rendererList);
        }

        currentHighlighted = null;
    }

    /// <summary>
    /// Highlights a given interactive object by rendering
    /// an outline on it. The color of the highlighting
    /// indicates whether the interaction is possible with the current action.
    /// - enabledState -1 means that the interaction is NOT possible.
    /// - enabledState 1 means that the interaction is possible.
    /// </summary>
    /// <param name="interactable">the interactable object to highlight</param>
    /// <param name="enabledState">state of the interaction</param>
    public void Highlight(Interactable interactable, int enabledState)
    {
        UnHighlight();

        if (null == interactable || !interactable.gameObject.activeSelf)
        {
            return;
        }

        List<Renderer> rendererList = CheckRenderers(interactable);
        currentHighlighted = interactable;
        outlinePostEffect.AddRenderers(rendererList);
        outlinePostEffect.SetColor(enabledState);
    }

    /// <summary>
    /// Retrieves all renderers of an interactable object
    /// </summary>
    /// <param name="interactable">the interactable object</param>
    /// <returns>the list of all renderers in the object hierarchy</returns>
    private List<Renderer> CheckRenderers(Interactable interactable)
    {
        /*
         * if the object is already mapped, the renderers can 
         * be fetched from the map, otherwise all renderers 
         * in the object hierarchy are collected and stored 
         * in the map for a later call.
         */
        int key = interactable.transform.GetInstanceID();

        if (!rendererMap.ContainsKey(key))
        {
            Renderer[] renderers = interactable.transform.GetComponentsInChildren<Renderer>();
            List<Renderer> rendererList = new List<Renderer>(renderers);
            rendererMap.Add(key, rendererList);
        }

        return rendererMap[key];
    }

    /// <summary>
    /// This method was called when a collected object
    /// is to be dropped in the current scene.
    /// </summary>
    /// <param name="isVisible">show or hide the object places</param>
    /// <param name="collectable">collectable object to be dropped</param>
    public void ShowObjectPlaces(bool isVisible, Collectable collectable = null)
    {
        /// The call is just passed to the current scene.
        CurrentRoom.ShowObjectsPlaces(isVisible, collectable);
    }

    /// <summary>
    /// This Method is called before closing the application.
    /// </summary>
    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}
