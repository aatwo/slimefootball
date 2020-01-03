using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
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

    Vector2[] goalSpawnLocations = new Vector2[2]; // In world coordinates
    Vector2Int[] playerSpawnLocations = new Vector2Int[2]; // In cell coordinates
    List<PlayerController> playerList = new List<PlayerController>();
    Transform ball;
    Transform leftGoal;
    Transform rightGoal;

    private void Start()
    {
        CalculateSpawnLocations();
        GenerateLevel();
        SpawnPlayers();
        SpawnBall();
    }

    void CalculateSpawnLocations()
    {
        // Players
        playerSpawnLocations[0] = new Vector2Int( 2, 1 );
        playerSpawnLocations[1] = new Vector2Int( gameWidth - 3, 1 );

        // Goals
        Vector2Int leftGoalGridPos = new Vector2Int (1, 1);
        goalSpawnLocations[0] = GetTileBottomLeftPos( leftGoalGridPos.x, leftGoalGridPos.y );

        Vector2Int rightGoalGridPos = new Vector2Int (gameWidth - 3, 1);
        goalSpawnLocations[1] = GetTileBottomLeftPos( rightGoalGridPos.x, rightGoalGridPos.y );
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

        // Goals
        leftGoal = Instantiate(goalPrefab, goalSpawnLocations[0], Quaternion.identity);
        rightGoal = Instantiate(goalPrefab, goalSpawnLocations[1], Quaternion.identity);

        // Flip the right goal to it's facing the right way then shift it two along to put it back in the correct position 
        // (its position is normally its bottom left, but flipping it in the x axis makes its position its bottom right)
        rightGoal.transform.localScale = new Vector3( rightGoal.transform.localScale.x * -1, 1, 1 );
        rightGoal.transform.position = new Vector3( rightGoal.transform.position.x + 2, rightGoal.transform.position.y, rightGoal.transform.position.z );
    }

    void SpawnPlayers()
    {
        for( int i = 0; i < 2; i++ )
        {
            Transform player = Instantiate(playerPrefab, GetPlayerSpawnPos(i), Quaternion.identity);
            PlayerController playerController = player.GetComponent<PlayerController>();
            if( playerController == null )
                Debug.LogError( "No player controller script found on player" );

            playerList.Add( playerController );
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
        Vector3 ballSpawnPos = GetTileCenterPos(gameWidth / 2, gameHeight / 2);
        ball = Instantiate(ballPrefab, ballSpawnPos, Quaternion.identity);
    }

    Vector3 GetTileCenterPos(int x, int y)
    {
        Vector3Int cellPos = new Vector3Int(x, y, 0);
        return environmentTilemap.GetCellCenterWorld( cellPos );
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
}
