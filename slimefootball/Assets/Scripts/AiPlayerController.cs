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

        PerformAi();
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
        if( playerController != null )
            playerController.SetManualInputEnabled( false );
    }

    void PerformAi()
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

        float playerY = playerController.transform.position.y;
        float ballY = ball.transform.position.y;

        // Blindly move towards the ball
        if( playerX > ballX )
            playerController.MoveLeft();
        else if( playerX < ballX )
            playerController.MoveRight();

        // If the ball is within a specific vertical window then jump
        float jumpXRange = 1f;
        float jumpYRange = 2f;
        //if( ballX > ( playerX - jumpXRange / 2f) && ballX < ( playerX + jumpXRange / 2f ) )
        {
            //if( ballY > playerY && ballY < ( playerY + jumpYRange ) )
            {
                playerController.Jump();
            }
        }
    }
}
