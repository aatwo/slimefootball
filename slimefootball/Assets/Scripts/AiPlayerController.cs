using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlayerController : MonoBehaviour
{
    public enum Direction
    {
        left,
        right
    };

    public Direction direction = Direction.left;
    public PlayerController playerController;

    Transform ball;
    float searchIntervalS = 1f;
    float lastBallSearchTime = 0f;
    float lastPlayerSearchTime = 0f;

    // Update is called once per frame
    void Update()
    {
        SearchForBall();
        SearchForPlayerController();

        MoveTowardsBall();
    }

    void SearchForBall()
    {
        if( ball != null )
            return;

        float timeSinceLastSearchS = Time.time - lastBallSearchTime;
        if( timeSinceLastSearchS < searchIntervalS )
            return;

        lastBallSearchTime = Time.time;
        GameObject ballObject = GameObject.FindGameObjectWithTag( "Ball" );
        if( ballObject != null )
            ball = ballObject.transform;
    }

    void SearchForPlayerController()
    {
        if( playerController != null )
            return;

        float timeSinceLastSearchS = Time.time - lastPlayerSearchTime;
        if( timeSinceLastSearchS < searchIntervalS )
            return;

        lastPlayerSearchTime = Time.time;
        playerController = GetComponent<PlayerController>();
    }

    void MoveTowardsBall()
    {
        if( playerController == null || ball == null )
            return;

        if( direction == Direction.right )
        {
            Debug.LogError( "AI facing Direction.right not currently supported" );
            return;
        }

        float playerX = playerController.transform.position.x;
        float ballX = ball.transform.position.x;

        if( direction == Direction.left )
        {
            if( playerX > ballX )
                playerController.MoveLeft();

            else if( playerX < ballX )
                playerController.MoveRight();
        }
    }
}
