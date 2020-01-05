﻿using System.Collections;
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

    private void Start()
    {
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
            controller.StartRound( ball, goals, playerScores, maxScore );
    }

    void StartNewRound()
    {
        ResetPositions();
        SetGameState( GameState.Playing );
        foreach( ICustomPlayerController controller in customPlayerControllerList )
            controller.StartRound( ball, goals, playerScores, maxScore );
    }

    void EndRound()
    {
        SetGameState( GameState.Resetting );
        foreach( ICustomPlayerController controller in customPlayerControllerList )
            controller.EndRound();
    }

    void EndGame()
    {
        SetGameState( GameState.Finished );
        foreach( ICustomPlayerController controller in customPlayerControllerList )
            controller.EndRound();
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

        Transform leftGoal = Instantiate(goalPrefab, leftGoalWorldPos, Quaternion.identity);
        Transform rightGoal = Instantiate(goalPrefab, rightGoalWorldPos, Quaternion.identity);

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
        SpawnPlayer( 0, 0 );
        SpawnPlayer( 1, 1 );

        // TEMP - attach a manual player controller to player 0 for controller index 0
        AddKeyboardPlayerController( 0, 0 );
        //AddKeyboardPlayerController( 1, 1 );

        // TEMP - attach an AI controller to player 1
        //AddAiPlayerController( 0 );
        AddAiPlayerController( 1 );
    }

    void SpawnPlayer(int playerSpriteIndex, int teamIndex)
    {
        Transform playerTransform = Instantiate(playerPrefab, GetTeamSpawnPos(teamIndex), Quaternion.identity);
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
        AiPlayerController aiController = player.transform.gameObject.AddComponent<AiPlayerController>();

        aiController.SetPlayerController( player.controller );
        aiController.SetTeamIndex( player.teamIndex );
        customPlayerControllerList.Add( aiController );
    }

    void SpawnBall()
    {
        ball = Instantiate(ballPrefab, GetBallSpawnPos(), Quaternion.identity);
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
        float xVariance = 0f;
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
            int playerIndex = -1;
            if( goals[0].IsChildOf( gameObject.transform ) )
            {
                Debug.Log( "PLAYER 1 GOAL!" );
                playerIndex = 1;
            }

            else if( goals[1].IsChildOf( gameObject.transform ) )
            {
                Debug.Log( "PLAYER 0 GOAL!" );
                playerIndex = 0;
            }

            if( playerIndex == -1 )
                Debug.LogError("Unknown player scored");

            playerScores[playerIndex]++;
            if( playerScores[playerIndex] >= maxScore )
            {
                EndGame();
                Debug.Log( "Player " + playerIndex + " wins!" );
            }
            else
            {
                EndRound();
            }
        }
        
        PrintScores();
    }

    void SetGameState(GameState state)
    {
        if( state == gameState )
        {
            Debug.LogWarning("SetGameState - the state is already set to " + state);
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
}
