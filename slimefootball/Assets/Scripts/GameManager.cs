using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    enum GameState
    {
        Playing,
        NotPlaying
    }
    GameState gameState = GameState.NotPlaying;

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

    Vector2Int[] goalSpawnLocations = new Vector2Int[2];
    Vector2Int[] playerSpawnLocations = new Vector2Int[2];
    Vector2Int ballSpawnLocation;
    List<PlayerController> playerControllerList = new List<PlayerController>();
    List<Transform> playerList = new List<Transform>();
    Transform ball;
    Transform leftGoal;
    Transform rightGoal;

    private void Start()
    {
        CalculateSpawnLocations();
        GenerateLevel();
        SpawnGoals();
        SpawnPlayers();
        SpawnBall();
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
        for( int i = 0; i < 2; i++ )
        {
            Transform player = Instantiate(playerPrefab, GetPlayerSpawnPos(i), Quaternion.identity);
            PlayerController playerController = player.GetComponent<PlayerController>();
            if( playerController == null )
                Debug.LogError( "No player controller script found on player" );

            playerList.Add( player );
            playerControllerList.Add( playerController );
        }

        // TEMP - attach a manual player controller to player 0 for controller index 0
        EnableManualControl( 0, 0 );

        // TEMP - attach an AI controller to player 1
        EnableAi( 1 );
    }

    void EnableManualControl( int playerIndex, int playerControllerIndex )
    {
        ManualPlayerController manualPlayerController = playerList[playerIndex].gameObject.AddComponent<ManualPlayerController>();
        manualPlayerController.SetPlayerControllerIndex( playerControllerIndex );
    }

    void EnableAi( int playerIndex )
    {
        playerList[playerIndex].gameObject.AddComponent<AiPlayerController>();
    }

    void SpawnBall()
    {
        Vector3 ballSpawnPos = GetTileCenterPos(ballSpawnLocation.x, ballSpawnLocation.y);
        ball = Instantiate(ballPrefab, ballSpawnPos, Quaternion.identity);
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
        // TODO
        if( leftGoal.IsChildOf(gameObject.transform) )
            Debug.Log( "PLAYER 1 GOAL!" );
        else if( rightGoal.IsChildOf(gameObject.transform) )
            Debug.Log( "PLAYER 0 GOAL!" );

        ResetPositions();
    }
    
    void ResetPositions()
    {
        for(int i = 0; i < playerList.Count; i++)
        {
            playerList[i].position = GetTileCenterPos(playerSpawnLocations[i]);
            Rigidbody2D rb = playerList[i].GetComponent<Rigidbody2D>();
            if( rb != null )
                rb.velocity = new Vector3( 0f, 0f, 0f );
        }

        ball.position = GetTileCenterPos( ballSpawnLocation );
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if( ballRb != null )
            ballRb.velocity = new Vector3( 0f, 0f, 0f );
    }
}
