using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    enum GameState
    {
        Playing,
        Resetting,
        Finished
    }
    GameState gameState = GameState.Playing;

    [SerializeField] GameObject parentGameObject;

    [SerializeField] Tilemap backgroundTilemap;
    [SerializeField] Tile backgroundTile;

    [SerializeField] Tilemap environmentTilemap;
    [SerializeField] Tile environmentTile;

    [SerializeField] Transform playerPrefab;
    [SerializeField] Transform ballPrefab;
    [SerializeField] Transform goalPrefab;

    [SerializeField] GameObject inGameMenu;
    [SerializeField] GameObject showMenuButton;
    [SerializeField] Text scoreText;
    [SerializeField] Text winnerText;

    static public int gameWidth = 32;
    static public int gameHeight = 20;

    public int maxScore = 3;
    int[] playerScores = new int[2];

    float prePauseTimeScale = 1f;

    Vector2Int[] goalSpawnLocations = new Vector2Int[2];
    Vector2Int[] playerSpawnLocations = new Vector2Int[2];
    Vector2Int ballSpawnLocation;

    struct Player
    {
        public PlayerController controller;
        public Transform transform;
        public int teamIndex;
    }
    List<Player> players = new List<Player>();
    List<ICustomPlayerController> customPlayerControllerList = new List<ICustomPlayerController>();

    Transform[] goals = new Transform[2];
    Transform ball;

    float restartDurationS = 2f;
    float restartStartTime = 0f;

    float finishedDurationS = 5f;
    float finishedStartTime = 0f;

    public void QuitToMenu()
    {
        SceneManager.LoadScene( "menu", LoadSceneMode.Single );
    }

    public void ShowInGameMenu()
    {
        inGameMenu.SetActive( true );
        showMenuButton.SetActive( false );
        SetGamePaused( true );
    }

    public void CloseInGameMenu()
    {
        inGameMenu.SetActive( false );
        showMenuButton.SetActive( true );
        SetGamePaused( false );
    }

    public void SetGamePaused(bool paused)
    {
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();

        if( paused )
        {
            prePauseTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = prePauseTimeScale;
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        ResetScores();
        CalculateSpawnLocations();
        GenerateLevel();
        SpawnGoals();
        SpawnPlayers();
        SpawnBall();
        SetupCamera();

        StartNewRound();
    }

    private void Update()
    {
        switch(gameState)
        {
            case GameState.Resetting:
            {
                UpdateForRestartingState();
                break;
            }

            case GameState.Finished:
            {
                UpdateForFinishedState();
                break;
            }
        }

        HandleKeyboardInput();
    }

    void UpdateForRestartingState()
    {
        float timeSpentRestarting = Time.time - restartStartTime;
        if(timeSpentRestarting >= restartDurationS)
        {
            StartNewRound();
        }
    }

    void UpdateForFinishedState()
    {
        float timeSpentFinished = Time.time - finishedStartTime;
        if( timeSpentFinished >= finishedDurationS )
        {
            StartNewGame();
        }
    }

    void StartNewGame()
    {
        ResetScores();
        ResetPositions();
        SetGameState( GameState.Playing );
        foreach( ICustomPlayerController controller in customPlayerControllerList )
            controller.HandleRoundStarted( ball, goals, playerScores, maxScore );

        winnerText.gameObject.SetActive( false );
    }

    void StartNewRound()
    {
        ResetPositions();
        SetGameState( GameState.Playing );
        foreach( ICustomPlayerController controller in customPlayerControllerList )
            controller.HandleRoundStarted( ball, goals, playerScores, maxScore );

        winnerText.gameObject.SetActive( false );
    }

    void EndRound()
    {
        SetGameState( GameState.Resetting );
        foreach( ICustomPlayerController controller in customPlayerControllerList )
            controller.HandleRoundFinished();
    }

    void EndGame()
    {
        SetGameState( GameState.Finished );
        foreach( ICustomPlayerController controller in customPlayerControllerList )
            controller.HandleRoundFinished();
    }

    void ResetScores()
    {
        int[] scores = new int[2];
        scores[0] = 0;
        scores[1] = 0;
        SetScores( scores );
    }

    void SetScores( int[] scores )
    {
        if( playerScores.Length != 2 )
            Debug.LogError( "setScores array was an unexpected size" );

        playerScores = scores;
        scoreText.text = playerScores[0] + " - " + playerScores[1];
    }

    void ResetPositions()
    {
        for( int i = 0; i < players.Count; i++ )
        {
            int teamIndex = players[i].teamIndex;
            players[i].transform.position = GetTeamSpawnPos(teamIndex);
            Rigidbody2D rb = players[i].transform.GetComponent<Rigidbody2D>();
            if( rb != null )
                rb.velocity = new Vector3( 0f, 0f, 0f );
        }

        ball.position = GetBallSpawnPos();
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if( ballRb != null )
            ballRb.velocity = new Vector3( 0f, 0f, 0f );
    }

    void CalculateSpawnLocations()
    {
        int playerEdgeOffset = 4;

        // Players
        playerSpawnLocations[0] = new Vector2Int( playerEdgeOffset, 1 );
        playerSpawnLocations[1] = new Vector2Int( gameWidth - playerEdgeOffset - 1, 1 );

        // Goals
        goalSpawnLocations[0] = new Vector2Int (1, 1);
        goalSpawnLocations[1] = new Vector2Int (gameWidth - 3, 1);

        // Ball
        ballSpawnLocation = new Vector2Int( gameWidth / 2, gameHeight / 2 );
    }

    void GenerateLevel()
    {
        for( int x = 0; x < gameWidth; x++ )
        {
            bool isHorizontalEdge = (x == 0 || x == gameWidth - 1);
            for( int y = 0; y < gameHeight; y++ )
            {
                bool isVerticalEdge = (y == 0 || y == gameHeight - 1);

                // Place floor tile
                Vector3Int p = new Vector3Int( x, y, 0 );
                backgroundTilemap.SetTile( p, backgroundTile );

                // Place environment tile
                if( isHorizontalEdge || isVerticalEdge )
                    environmentTilemap.SetTile( p, environmentTile );
            }
        }
    }

    void SpawnGoals()
    {
        Vector3 leftGoalWorldPos = GetTileBottomLeftPos( goalSpawnLocations[0].x, goalSpawnLocations[0].y );
        Vector3 rightGoalWorldPos = GetTileBottomLeftPos( goalSpawnLocations[1].x, goalSpawnLocations[1].y );

        Transform leftGoal = Instantiate(goalPrefab, leftGoalWorldPos, Quaternion.identity, parentGameObject.transform);
        Transform rightGoal = Instantiate(goalPrefab, rightGoalWorldPos, Quaternion.identity, parentGameObject.transform);

        // Flip the right goal to it's facing the right way then shift it two along to put it back in the correct position 
        // (its position is normally its bottom left, but flipping it in the x axis makes its position its bottom right)
        rightGoal.transform.localScale = new Vector3( rightGoal.transform.localScale.x * -1, 1, 1 );
        rightGoal.transform.position = new Vector3( rightGoal.transform.position.x + 2, rightGoal.transform.position.y, rightGoal.transform.position.z );

        // Listen for goal events
        GoalController leftGoalController = leftGoal.GetComponent<GoalController>();
        if( leftGoalController == null )
            Debug.LogError("Unable to find goal controller on left goal game object");
        leftGoalController.OnGoalEvent += HandleGoalEvent;

        GoalController rightGoalController = rightGoal.GetComponent<GoalController>();
        if( rightGoalController == null )
            Debug.LogError( "Unable to find goal controller on right goal game object" );
        rightGoalController.OnGoalEvent += HandleGoalEvent;

        goals[0] = leftGoal;
        goals[1] = rightGoal;
    }

    void SpawnPlayers()
    {
        if(MenuData.GameMode == Common.GameMode.AiOnly1v1)
        {
            SpawnPlayer( 0, 0 );
            SpawnPlayer( 1, 1 );
            AddAiPlayerController( 0 );
            AddAiPlayerController( 1 );
        }

        else if( MenuData.GameMode == Common.GameMode.SinglePlayer1v1 )
        {
            SpawnPlayer( 0, 0 );
            SpawnPlayer( 1, 1 );
            AddKeyboardPlayerController( 0, 0 );
            AddAiPlayerController( 1 );
        }

        else if( MenuData.GameMode == Common.GameMode.TwoPlayer1v1 )
        {
            SpawnPlayer( 0, 0 );
            SpawnPlayer( 1, 1 );
            AddKeyboardPlayerController( 0, 0 );
            AddKeyboardPlayerController( 1, 1 );
        }

        else if( MenuData.GameMode == Common.GameMode.AiOnly2v2 )
        {
            SpawnPlayer( 0, 0 );
            SpawnPlayer( 1, 0 );
            SpawnPlayer( 2, 1 );
            SpawnPlayer( 3, 1 );
            AddAiPlayerController( 0 );
            AddAiPlayerController( 1 );
            AddAiPlayerController( 2 );
            AddAiPlayerController( 3 );
        }

        else if( MenuData.GameMode == Common.GameMode.SinglePlayer2v2 )
        {
            SpawnPlayer( 0, 0 );
            SpawnPlayer( 1, 0 );
            SpawnPlayer( 2, 1 );
            SpawnPlayer( 3, 1 );
            AddKeyboardPlayerController( 0, 0 );
            AddAiPlayerController( 1 );
            AddAiPlayerController( 2 );
            AddAiPlayerController( 3 );
        }

        else if( MenuData.GameMode == Common.GameMode.TwoPlayer2v2 )
        {
            SpawnPlayer( 0, 0 );
            SpawnPlayer( 1, 0 );
            SpawnPlayer( 2, 1 );
            SpawnPlayer( 3, 1 );
            AddKeyboardPlayerController( 0, 0 );
            AddAiPlayerController( 1 );
            AddKeyboardPlayerController( 2, 1 );
            AddAiPlayerController( 3 );
        }

        else if( MenuData.GameMode == Common.GameMode.TwoPlayerCoop2v2 )
        {
            SpawnPlayer( 0, 0 );
            SpawnPlayer( 1, 0 );
            SpawnPlayer( 2, 1 );
            SpawnPlayer( 3, 1 );
            AddKeyboardPlayerController( 0, 0 );
            AddKeyboardPlayerController( 1, 1 );
            AddAiPlayerController( 2 );
            AddAiPlayerController( 3 );
        }

        else if( MenuData.GameMode == Common.GameMode.TwoPlayerCoop2v10 )
        {
            SpawnPlayer( 0, 0 );
            SpawnPlayer( 1, 0 );
            SpawnPlayer( 2, 1 );
            SpawnPlayer( 3, 1 );
            SpawnPlayer( 2, 1 );
            SpawnPlayer( 3, 1 );
            SpawnPlayer( 2, 1 );
            SpawnPlayer( 3, 1 );
            SpawnPlayer( 2, 1 );
            SpawnPlayer( 3, 1 );
            AddKeyboardPlayerController( 0, 0 );
            AddKeyboardPlayerController( 1, 1 );
            AddAiPlayerController( 2 );
            AddAiPlayerController( 3 );
            AddAiPlayerController( 4 );
            AddAiPlayerController( 5 );
            AddAiPlayerController( 6 );
            AddAiPlayerController( 7 );
            AddAiPlayerController( 8 );
            AddAiPlayerController( 9 );
        }

        else
        {
            Debug.LogError("Unsupported game mode: " + MenuData.GameMode);
        }
    }

    void SpawnPlayer(int playerSpriteIndex, int teamIndex)
    {
        Transform playerTransform = Instantiate(playerPrefab, GetTeamSpawnPos(teamIndex), Quaternion.identity, parentGameObject.transform);
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if( playerController == null )
            Debug.LogError( "No player controller script found on player (SpawnPlayer)" );

        playerController.SetPlayerSpriteIndex( playerSpriteIndex );

        Player player = new Player();
        player.controller = playerController;
        player.transform = playerTransform;
        player.teamIndex = teamIndex;
        players.Add( player );
    }

    void AddKeyboardPlayerController( int playerIndex, int playerControllerIndex )
    {
        Player player = players[playerIndex];
        KeyboardPlayerController keyboardController = player.transform.gameObject.AddComponent<KeyboardPlayerController>();

        keyboardController.SetPlayerInputIndex( playerControllerIndex );
        keyboardController.SetTeamIndex( player.teamIndex );
        keyboardController.SetPlayerController( player.controller );
        customPlayerControllerList.Add( keyboardController );
    }

    void AddAiPlayerController( int playerIndex )
    {
        Player player = players[playerIndex];

        Common.AiImplementations[] teamAiTypes = MenuData.TeamAiImplementations;
        if( player.teamIndex >= teamAiTypes.Length )
            Debug.LogError("attempting to attach AI for team index " + player.teamIndex + " but no AI for this index exists");

        Common.AiImplementations aiType = teamAiTypes[player.teamIndex];

        if(aiType == Common.AiImplementations.Random)
        {
            aiType = (Common.AiImplementations)Random.Range( (int)Common.AiImplementations.Default, (int)Common.AiImplementations.Random - 1 );
        }

        ICustomPlayerController aiController = null;
        switch(aiType)
        {
            case Common.AiImplementations.Default:
            {
                aiController = player.transform.gameObject.AddComponent<AiPlayerController>();
                break;
            }

            case Common.AiImplementations.Aaron:
            {
                aiController = player.transform.gameObject.AddComponent<AaronAiPlayerController>();
                break;
            }

            case Common.AiImplementations.Rich:
            {
                aiController = player.transform.gameObject.AddComponent<RichAiPlayerController>();
                break;
            }

            default:
            {
                Debug.LogError( "AddAiPlayerController - no implementation for ai type: " + aiType );
                break;
            }
        }

        player.controller.SetNameTag(aiController.GetDisplayName());
        aiController.SetPlayerController( player.controller );
        aiController.SetTeamIndex( player.teamIndex );
        customPlayerControllerList.Add( aiController );
    }

    void SpawnBall()
    {
        ball = Instantiate(ballPrefab, GetBallSpawnPos(), Quaternion.identity, parentGameObject.transform );
    }

    void SetupCamera()
    {
        float camX = environmentTilemap.transform.position.x + (0.5f * gameWidth * environmentTilemap.cellSize.x);
        float camY = environmentTilemap.transform.position.y + (0.5f * gameHeight * environmentTilemap.cellSize.y);

        { // Make the game height the same as the camera height

            // The orthographicSize is half the size of the vertical viewing volume. The horizontal size of the viewing volume depends on the aspect ratio.
            Camera.main.orthographicSize = ( gameHeight + 2 * environmentTilemap.cellSize.y ) / 2;
        }

        { // Make the game width the same as the camera width

            // 1. take the aspect ratio of the game world
            //float gameWorldAspectRatio = (float)gameWidth / (float)gameHeight;

            //Camera.main.orthographicSize = ( ( gameHeight * environmentTilemap.cellSize.y ) / ( 2f ) ) * gameWorldAspectRatio;

            // TODO: figure out what scale we need to fit either the full width or height into the camera view port
        }

        Camera.main.transform.position = new Vector3( camX, camY, Camera.main.transform.position.z );
    }

    Vector3 GetTileCenterPos(int x, int y)
    {
        Vector3Int cellPos = new Vector3Int(x, y, 0);
        return environmentTilemap.GetCellCenterWorld( cellPos );
    }

    Vector3 GetTileCenterPos( Vector2Int pos )
    {
        return GetTileCenterPos( pos.x, pos.y );
    }

    Vector3 GetTileBottomLeftPos( int x, int y )
    {
        Vector3Int cellPos = new Vector3Int(x, y, 0);
        Vector3 cellCenterWorldPos = environmentTilemap.GetCellCenterWorld( cellPos );
        return new Vector3(cellCenterWorldPos.x - (environmentTilemap.cellSize.x * 0.5f), cellCenterWorldPos.y - (environmentTilemap.cellSize.y * 0.5f));

    }

    Vector3 GetTeamSpawnPos( int teamIndex )
    {
        float xVariance = 0f + (players.Count * .2f);
        Vector3 pos = GetTileCenterPos(playerSpawnLocations[teamIndex].x, playerSpawnLocations[teamIndex].y);
        return new Vector3(pos.x + Random.Range(-xVariance, xVariance), pos.y, 0f);
    }

    Vector3 GetBallSpawnPos()
    {
        const float maxHorizontalPosVariance = 0.2f;
        const float maxVerticalPosVariance = 1f;

        Vector3 pos = GetTileCenterPos( ballSpawnLocation );

        float randomX = Random.Range(pos.x - maxHorizontalPosVariance, pos.x + maxHorizontalPosVariance);
        float randomY = Random.Range(pos.y - maxVerticalPosVariance, pos.y + maxVerticalPosVariance);

        pos = new Vector3(randomX, randomY, pos.z);

        return pos;
    }

    bool IsTilePlayerSpawn(int x, int y)
    {
        for( int i = 0; i < playerSpawnLocations.Length; i++ )
        {
            if( playerSpawnLocations[i].x == x && playerSpawnLocations[i].y == y )
                return true;
        }
        return false;
    }

    void HandleGoalEvent(GameObject gameObject)
    {
        if( gameState == GameState.Playing )
        {
            int teamIndex = -1;
            if( goals[0].IsChildOf( gameObject.transform ) )
            {
                teamIndex = 1;
            }

            else if( goals[1].IsChildOf( gameObject.transform ) )
            {
                teamIndex = 0;
            }

            if( teamIndex == -1 )
                Debug.LogError("Unknown player scored");

            HandleTeamScore( teamIndex );
        }
    }

    void HandleTeamWin(int teamIndex)
    {
        winnerText.text = "TEAM " + ( teamIndex + 1 ) + " WINS";
        winnerText.gameObject.SetActive( true );
        EndGame();
    }

    void HandleTeamScore( int teamIndex )
    {
        int [] newScores = playerScores;
        newScores[teamIndex]++;
        SetScores( newScores );
        if( playerScores[teamIndex] >= maxScore )
        {
            HandleTeamWin( teamIndex );
        }
        else
        {
            winnerText.text = "GOAL";
            EndRound();
        }
        winnerText.gameObject.SetActive( true );
    }

    void SetGameState(GameState state)
    {
        if( state == gameState )
        {
            //Debug.LogWarning("SetGameState - the state is already set to " + state);
            return;
        }

        switch(state)
        {
            case GameState.Resetting:
            {
                restartStartTime = Time.time;
                break;
            }
            case GameState.Playing:
            {
                break;
            }

            case GameState.Finished:
            {
                finishedStartTime = Time.time;
                break;
            }
        }

        gameState = state;
    }

    void PrintScores()
    {
        Debug.Log( "SCORE: " + playerScores[0] + " - " + playerScores[1] );
    }

    void HandleKeyboardInput()
    {
        if( Input.GetKeyDown( KeyCode.Escape ) )
        {
            if(inGameMenu.activeSelf)
                CloseInGameMenu();
            else
                ShowInGameMenu();
        }
    }
}
