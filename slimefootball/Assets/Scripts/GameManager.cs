using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    enum GameState
    {
        Playing,
        Resetting,
        Finished
    }
    GameState gameState = GameState.Playing;

    [SerializeField]
    Tilemap backgroundTilemap;
    [SerializeField]
    Tile backgroundTile;

    [SerializeField]
    Tilemap environmentTilemap;
    [SerializeField]
    Tile environmentTile;

    [SerializeField]
    Transform playerPrefab;

    [SerializeField]
    Transform ballPrefab;

    [SerializeField]
    Transform goalPrefab;

    static public int gameWidth = 32;
    static public int gameHeight = 20;

    public int maxScore = 3;
    int[] playerScores = new int[2];

    Vector2Int[] goalSpawnLocations = new Vector2Int[2];
    Vector2Int[] playerSpawnLocations = new Vector2Int[2];
    Vector2Int ballSpawnLocation;
    List<PlayerController> playerControllerList = new List<PlayerController>();
    List<AiPlayerController> aiPlayerControllerList = new List<AiPlayerController>();
    List<ManualPlayerController> manualPlayerControllerList = new List<ManualPlayerController>();
    List<Transform> playerList = new List<Transform>();
    Transform ball;
    Transform leftGoal;
    Transform rightGoal;

    float restartDurationS = 2f;
    float restartStartTime = 0f;

    float finishedDurationS = 5f;
    float finishedStartTime = 0f;

    private void Start()
    {
        ResetScores();
        CalculateSpawnLocations();
        GenerateLevel();
        SpawnGoals();
        SpawnPlayers();
        SpawnBall();
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
    }

    void UpdateForRestartingState()
    {
        float timeSpentRestarting = Time.time - restartStartTime;
        if(timeSpentRestarting >= restartDurationS)
        {
            SetGameState( GameState.Playing );
        }
    }

    void UpdateForFinishedState()
    {
        float timeSpentFinished = Time.time - finishedStartTime;
        if( timeSpentFinished >= finishedDurationS )
        {
            ResetGame();
            SetGameState( GameState.Playing );
        }
    }

    void ResetGame()
    {
        ResetScores();
        ResetPositions();
    }

    void ResetScores()
    {
        for( int i = 0; i < playerScores.Length; i++ )
        {
            playerScores[i] = 0;
        }
    }

    void ResetPositions()
    {
        for( int i = 0; i < playerList.Count; i++ )
        {
            playerList[i].position = GetPlayerSpawnPos(i);
            Rigidbody2D rb = playerList[i].GetComponent<Rigidbody2D>();
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
        // Players
        playerSpawnLocations[0] = new Vector2Int( 4, 1 );
        playerSpawnLocations[1] = new Vector2Int( gameWidth - 5, 1 );

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

        leftGoal = Instantiate(goalPrefab, leftGoalWorldPos, Quaternion.identity);
        rightGoal = Instantiate(goalPrefab, rightGoalWorldPos, Quaternion.identity);

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
    }

    void SpawnPlayers()
    {
        SpawnPlayer( 0 );
        SpawnPlayer( 1 );

        // TEMP - attach a manual player controller to player 0 for controller index 0
        EnableManualControl( 0, 0 );
        //EnableManualControl( 1, 1 );

        // TEMP - attach an AI controller to player 1
        //EnableAi( 0 );
        EnableAi( 1 );
    }

    void SpawnPlayer(int index)
    {
        Transform player = Instantiate(playerPrefab, GetPlayerSpawnPos(index), Quaternion.identity);
        PlayerController playerController = player.GetComponent<PlayerController>();
        if( playerController == null )
            Debug.LogError( "No player controller script found on player" );

        playerController.SetPlayerSpriteIndex( index );
        playerList.Add( player );
        playerControllerList.Add( playerController );
    }

    void EnableManualControl( int playerIndex, int playerControllerIndex )
    {
        ManualPlayerController manualPlayerController = playerList[playerIndex].gameObject.AddComponent<ManualPlayerController>();
        manualPlayerController.SetPlayerControllerIndex( playerControllerIndex );
        manualPlayerControllerList.Add( manualPlayerController );
    }

    void EnableAi( int playerIndex )
    {
        AiPlayerController aiController = playerList[playerIndex].gameObject.AddComponent<AiPlayerController>();
        aiPlayerControllerList.Add( aiController );
    }

    void SpawnBall()
    {
        ball = Instantiate(ballPrefab, GetBallSpawnPos(), Quaternion.identity);
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

    Vector3 GetPlayerSpawnPos( int index )
    {
        Vector3 pos = GetTileCenterPos(playerSpawnLocations[index].x, playerSpawnLocations[index].y);
        return new Vector3(pos.x, pos.y, 0f);
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
            int playerIndex = -1;
            if( leftGoal.IsChildOf( gameObject.transform ) )
            {
                Debug.Log( "PLAYER 1 GOAL!" );
                playerIndex = 1;
            }

            else if( rightGoal.IsChildOf( gameObject.transform ) )
            {
                Debug.Log( "PLAYER 0 GOAL!" );
                playerIndex = 0;
            }

            if( playerIndex == -1 )
                Debug.LogError("Unknown player scored");

            playerScores[playerIndex]++;
            if( playerScores[playerIndex] >= maxScore )
            {
                Debug.Log("Player " + playerIndex + " wins!");
                SetGameState( GameState.Finished );
            }
            else
                SetGameState( GameState.Resetting );            
        }
        
        PrintScores();
    }

    void SetGameState(GameState state)
    {
        switch(state)
        {
            case GameState.Resetting:
            {
                restartStartTime = Time.time;
                SetAllAiPlayersEnabled( false );
                break;
            }
            case GameState.Playing:
            {
                ResetPositions();
                SetAllAiPlayersEnabled( true );
                break;
            }

            case GameState.Finished:
            {
                finishedStartTime = Time.time;
                SetAllAiPlayersEnabled( true );
                break;
            }
        }

        gameState = state;
    }

    void SetAllAiPlayersEnabled( bool enabled )
    {
        foreach( AiPlayerController controller in aiPlayerControllerList )
        {
            controller.enabled = enabled;
        }
    }

    void PrintScores()
    {
        Debug.Log( "SCORE: " + playerScores[0] + " - " + playerScores[1] );
    }
}
