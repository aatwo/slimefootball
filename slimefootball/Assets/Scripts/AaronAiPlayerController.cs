using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AaronAiPlayerController : MonoBehaviour, ICustomPlayerController
{
    Common.Direction direction = Common.Direction.left;
    PlayerController playerController;
    Transform ball;
    List<Transform> opposingTeamPositions;

    Transform myGoal;
    int myIndex = -1;

    float lastTimeStateChange = 0f;

    enum AiState
    {
        attacking,
        defending
    }

    AiState aiState = AiState.defending;

    public string GetDisplayTag()
    {
        return Common.ToString(Common.AiImplementations.Aaron);
    }

    public void SetPlayerController( PlayerController playerController )
    {
        this.playerController = playerController;
    }

    public void SetTeamIndex( int index )
    {
        myIndex = index;
        if(index == 0)
            direction = Common.Direction.right;
        else
            direction = Common.Direction.left;
    }

    public void HandleRoundStarted( Transform ball, List<Transform> teamPositions, List<Transform> opposingTeamPositions, Transform[] goals, int[] scores, int winningScore )
    {
        this.ball = ball;
        this.myGoal = goals[myIndex];
        this.opposingTeamPositions = opposingTeamPositions;

        lastTimeStateChange = Time.time;
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

        /*
        float timeElapsedSinceLastStateChange = Time.time - lastTimeStateChange;
        if(timeElapsedSinceLastStateChange > 10f)
        {
            if (aiState == AiState.attacking)
                aiState = AiState.defending;
            else
                aiState = AiState.attacking;
            lastTimeStateChange = Time.time;
        }
        */

        switch (aiState)
        {
            case AiState.attacking:
            {
                PerformAttackingAi();
                break;
            }

            case AiState.defending:
            {
                PerformDefendingAi();
                break;
            }

            default:
            {
                Debug.LogError("unhandled AI state in aaron ai implementation");
                break;
            }
        }
    }

    void PerformAttackingAi()
    {
        float playerX = playerController.transform.position.x;
        float ballX = ball.transform.position.x;

        float playerY = playerController.transform.position.y;
        float ballY = ball.transform.position.y;

        float distanceToBall = Mathf.Abs(playerX - ballX);

        // Blindly move towards the ball
        if (playerX > ballX)
        {
            if (direction == Common.Direction.left)
            {
                if (distanceToBall < 1f)
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
        else if (playerX < ballX)
        {
            if (direction == Common.Direction.right)
            {
                if (distanceToBall < 1f)
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
        if (ballX > (playerX - jumpXRange / 2f) && ballX < (playerX + jumpXRange / 2f))
        {
            if (ballY > playerY && ballY < (playerY + jumpYRange))
            {
                playerController.Jump();
            }
        }
    }

    void PerformDefendingAi()
    {
        if (myGoal == null)
            Debug.LogError("PerformDefendingAi: myGoal is null");

        float playerX = playerController.transform.position.x;
        float ballX = ball.transform.position.x;

        float playerY = playerController.transform.position.y;
        float ballY = ball.transform.position.y;

        float distanceToBall = Mathf.Abs(playerX - ballX);

        float goalOffset = 3f;
        if (direction == Common.Direction.left)
            goalOffset *= -1f;

        float myGoalX = myGoal.position.x + goalOffset;
        float distanceToMyGoal = Mathf.Abs(playerX - myGoalX);

        // Always move towards the ball if it is behind us
        if (ballX > (playerX - 1f) && direction == Common.Direction.left)
        {
            playerController.MoveHorizontal(1f);
        }
        else if (ballX < (playerX + 1f) && direction == Common.Direction.right)
        {
            playerController.MoveHorizontal(-1f);
        }
        else if (distanceToMyGoal > 0.5f)
        {
            float movementMultiplier = 1;
            if(playerX > myGoalX)
            {
                movementMultiplier = -1f;
            }
            playerController.MoveHorizontal(movementMultiplier);
        }
        else
        {
            playerController.MoveHorizontal(0f);
        }

        // If the ball is within a specific vertical window then jump
        if (distanceToBall < 3f)
        {
            float jumpOffset = 1f;
            if (ballY > playerY + jumpOffset && ballY < playerY + 4)
            {
                playerController.Jump();
            }
        }
    }
}
