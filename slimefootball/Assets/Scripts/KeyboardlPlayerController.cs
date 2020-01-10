using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardPlayerController : MonoBehaviour, ICustomPlayerController
{
    PlayerController playerController;
    Transform ball;
    int playerInputIndex = -1;
    bool hasReleasedJumpSinceLastJump = true;

    public string GetDisplayTag()
    {
        return "Keyboard";
    }

    public void SetPlayerInputIndex( int n )
    {
        playerInputIndex = n;
    }

    public void SetTeamIndex( int index )
    {


    }

    public void SetPlayerController( PlayerController playerController )
    {
        this.playerController = playerController;
    }

    public void HandleRoundStarted( Transform ball, Transform[] goals, int[] scores, int winningScore )
    {

    }

    public void HandleRoundFinished()
    {

    }

    void Update()
    {
        if( playerInputIndex != -1 )
        {
            ProcessKeyboardInput();
        }
    }

    void ProcessKeyboardInput()
    {
        if( playerController == null )
            return;

        string horizontalAxisName = "Horizontal" + playerInputIndex + "_key";
        string verticalAxisName = "Vertical" + playerInputIndex + "_key";

        float horizontalInput = Input.GetAxis( horizontalAxisName );
        playerController.MoveHorizontal( horizontalInput );

        float verticalInput = Input.GetAxisRaw( verticalAxisName );
        bool pressingJump = (verticalInput > 0.0f);

        if( !pressingJump )
        {
            hasReleasedJumpSinceLastJump = true;
            playerController.MoveVertical( 0f );
        }

        // We only want to apply jump force while the user presses the jump button for the first jump cycle
        else if( pressingJump && hasReleasedJumpSinceLastJump )
        {
            hasReleasedJumpSinceLastJump = false;
            playerController.MoveVertical( verticalInput );
        }

        // Once the jump is started we want to apply jump force only while the jump state is jumping
        else if( pressingJump && !hasReleasedJumpSinceLastJump && playerController.GetJumpState() == PlayerController.JumpState.jumping)
        {
            playerController.MoveVertical( verticalInput );
        }

        else
        {
            playerController.MoveVertical( 0f );
        }
    }
}
