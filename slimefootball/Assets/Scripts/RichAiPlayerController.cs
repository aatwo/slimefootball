using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RichAiPlayerController : MonoBehaviour, ICustomPlayerController
{
    Common.Direction direction = Common.Direction.left;
    PlayerController playerController;
    Transform ball;

    public string GetDisplayTag()
    {
        return Common.ToString( Common.AiImplementations.Rich );
    }

    public void SetPlayerController( PlayerController playerController )
    {
        this.playerController = playerController;
    }

    public void SetTeamIndex( int index )
    {
        if(index == 0)
            direction = Common.Direction.right;
        else
            direction = Common.Direction.left;
    }

    public void HandleRoundStarted( Transform ball, List<Transform> teamPositions, List<Transform> opposingTeamPositions, Transform[] goals, int[] scores, int winningScore )
    {
        this.ball = ball;
    }

    public void HandleRoundFinished()
    {
        this.ball = null;
    }

    // Update is called once per frame
    void Update()
    {
        PerformAi();
    }

    void PerformAi()
    {
        if( playerController == null || ball == null )
            return;

        float playerX = playerController.transform.position.x;
        float ballX = ball.transform.position.x;

        float playerY = playerController.transform.position.y;
        float ballY = ball.transform.position.y;

        float distanceToBall = Mathf.Abs(playerX - ballX);

        // Blindly move towards the ball
        if( playerX > ballX )
        {
            if( direction == Common.Direction.left )
            {
                if( distanceToBall < 1f )
                {
                    playerController.MoveRight();
                }
                else
                {
                    playerController.MoveLeft();
                }
            }
            else
            {
                playerController.MoveLeft();
            }
        }
        else if( playerX < ballX )
        {
            if( direction == Common.Direction.right )
            {
                if( distanceToBall < 1f )
                {
                    playerController.MoveLeft();
                }
                else
                {
                    playerController.MoveRight();
                }
            }
            else
            {
                playerController.MoveRight();
            }
        }

        // If the ball is within a specific vertical window then jump
        float jumpXRange = 2f;
        float jumpYRange = 2f;
        if( ballX > ( playerX - jumpXRange / 2f) && ballX < ( playerX + jumpXRange / 2f ) )
        {
            if( ballY > playerY && ballY < ( playerY + jumpYRange ) )
            {
                playerController.Jump();
            }
        }
    }
}
