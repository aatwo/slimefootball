using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    Tilemap floorTilemap;
    [SerializeField]
    Tile floorTile;

    [SerializeField]
    Tilemap environmentTilemap;
    [SerializeField]
    Tile wallTile;

    [SerializeField]
    Transform playerPrefab;

    Vector2Int[] playerSpawnLocations = new Vector2Int[4];
    List<PlayerController> playerList = new List<PlayerController>();

    private void Start()
    {
        CalculatePlayerSpawnLocations();
        GenerateLevel();
        SpawnPlayers();
    }

    void CalculatePlayerSpawnLocations()
    {
        playerSpawnLocations[0] = new Vector2Int( 1, 1 );
        playerSpawnLocations[1] = new Vector2Int( Common.gameWidth - 2, 1 );
    }

    void GenerateLevel()
    {
        for( int x = 0; x < Common.gameWidth; x++ )
        {
            bool isHorizontalEdge = (x == 0 || x == Common.gameWidth - 1);
            for( int y = 0; y < Common.gameHeight; y++ )
            {
                bool isVerticalEdge = (y == 0 || y == Common.gameHeight - 1);

                // Place floor tile
                Vector3Int p = new Vector3Int( x, y, 0 );
                floorTilemap.SetTile( p, floorTile );

                // Place environment tile
                if( isHorizontalEdge || isVerticalEdge )
                    environmentTilemap.SetTile( p, wallTile );
            }
        }
    }

    void SpawnPlayers()
    {
        // TODO: support multiple players

        for( int i = 0; i < 2; i++ )
        {
            Transform player = Instantiate(playerPrefab, GetPlayerSpawnPos(i), Quaternion.identity);
            PlayerController playerController = player.GetComponent<PlayerController>();
            if( playerController == null )
                Debug.LogError( "No player controller script found on player" );

            playerController.SetPlayerIndex( i );
            playerList.Add( playerController );
        }
    }

    Vector3 GetTileCenterPos(int x, int y)
    {
        Vector3Int cellPos = new Vector3Int(x, y, 0);
        return environmentTilemap.GetCellCenterWorld( cellPos );
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
