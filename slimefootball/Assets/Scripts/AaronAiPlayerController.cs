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
    Transform enemyGoal;
    List<Transform> goals;
    int myIndex = -1;

    public enum AiState
    {
        attacking,
        defending
    }

    bool aiStateFixed = false;
    AiState aiState = AiState.defending;

    public void SetFixedAiState(AiState state)
    {
        aiStateFixed = true;
        aiState = state;
    }

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
        if (myIndex == 0)
            enemyGoal = goals[1];
        else
            enemyGoal = goals[0];
        this.opposingTeamPositions = opposingTeamPositions;
        this.goals = new List<Transform>();
        for (int i = 0; i < goals.Length; i++)
            this.goals.Add(goals[i]);

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

        if(!aiStateFixed)
            CalculateAiState();

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

    Vector3 GetAveragePos(List<Transform> transforms)
    {
        if (transforms.Count == 0)
            return new Vector3(0f, 0f, 0f);

        int count = 0;
        float averageX = 0f;
        float averageY = 0f;
        foreach(Transform t in transforms)
        {
            count++;
            averageX += t.position.x;
            averageY += t.position.y;
        }
        averageX /= (float)count;
        averageY /= (float)count;

        return new Vector3(averageX, averageY, 0f);
    }

    void CalculateAiState()
    {
        if (goals.Count != 2)
            return;

        Vector3 averageEnemyPosition = GetAveragePos(opposingTeamPositions);
        bool ballIsCloserToMyGoal = Vector3.Distance(myGoal.transform.position, ball.transform.position) < Vector3.Distance(enemyGoal.transform.position, ball.transform.position);
        bool enemyIsCloserToMyGoal = Vector3.Distance(averageEnemyPosition, myGoal.transform.position) < Vector3.Distance(averageEnemyPosition, enemyGoal.transform.position);

        // ballX and enemyX in their half = defend
        // ballX and enemyX in our half = attack
        // ballX in theirs and enemyX in ours = attack
        // ballX in ours and enemyX in theirs = attack

        if (ballIsCloserToMyGoal && enemyIsCloserToMyGoal)
        {
            aiState = AiState.defending;
        }
        else if (ballIsCloserToMyGoal && !enemyIsCloserToMyGoal)
        {
            // Default to attacking
            aiState = AiState.attacking;

            // If the ball is above a certain velocity it should be defending instead
            Rigidbody2D ballRb = ball.gameObject.GetComponent<Rigidbody2D>();
            if(ballRb && ballRb.velocity.x > 10f)
            {
                aiState = AiState.defending;
            }
        }
        else if (!ballIsCloserToMyGoal && enemyIsCloserToMyGoal)
        {
            aiState = AiState.attacking;
        }
        else if (!ballIsCloserToMyGoal && !enemyIsCloserToMyGoal)
        {
            aiState = AiState.defending;
        }
    }

    float currentHorizontalMovement = 0f;
    float currentHorizontalMovementTarget = 0f;
    float speedChangePerSecond = 4f;
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
                if (distanceToBall < 0.8f)
                {
                    currentHorizontalMovementTarget = 1f;
                }
                else
                {
                    currentHorizontalMovementTarget = -1f;
                }
            }
            else
            {
                currentHorizontalMovementTarget = -1f;
            }
        }
        else if (playerX < ballX)
        {
            if (direction == Common.Direction.right)
            {
                if (distanceToBall < 0.8f)
                {
                    currentHorizontalMovementTarget = -1f;
                }
                else
                {
                    currentHorizontalMovementTarget = 1f;
                }
            }
            else
            {
                currentHorizontalMovementTarget = 1f;
            }
        }

        if(currentHorizontalMovementTarget < currentHorizontalMovement)
        {
            currentHorizontalMovement -= Time.deltaTime * speedChangePerSecond;
            if (currentHorizontalMovement < currentHorizontalMovementTarget)
                currentHorizontalMovement = currentHorizontalMovementTarget;
        }
        else if(currentHorizontalMovementTarget > currentHorizontalMovement)
        {
            currentHorizontalMovement += Time.deltaTime * speedChangePerSecond;
            if (currentHorizontalMovement > currentHorizontalMovementTarget)
                currentHorizontalMovement = currentHorizontalMovementTarget;
        }

        playerController.MoveHorizontal(currentHorizontalMovement);

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
            float jumpOffset = 0.5f;
            if (ballY > playerY + jumpOffset && ballY < playerY + 4)
            {
                playerController.Jump();
            }
        }
    }
}
